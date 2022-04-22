using System;
using SuperWebSocket;
using System.Windows.Forms;
using System.Net.WebSockets;
using System.Threading;
using Websocket.Client;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management;
using System.Linq;
using System.IO.Ports;
using System.Timers;

namespace JanuityUI.Modules
{
    internal class TemperatureThermal
    {
        static Thread Thread_Temperature, cmd;
        static Uri url;
        static ManualResetEvent exitEvent;
        static IWebsocketClient client;
        public static WebSocketSession csession;
        static string[] portnames;
        static bool testRunning = false;

        public static void Serverstart()
        {
            kioskLog.SrushtyLog_Temp("Temperature Started");
            cmd = new Thread(TerabeePythonApiProcess);
            exitEvent = new ManualResetEvent(false);
            url = new Uri("ws://localhost:9872/ws");
            using (client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info =>
                    Log.Information($"Reconnection happened, type: {info.Type}"));
                client.MessageReceived.Subscribe(msg => Receiveddata("" + msg));
                if (cmd != null || !cmd.IsAlive)
                {
                    cmd.Start();
                }
                kioskLog.SrushtyLog_Temp("Socket Started");
                client.Start();
                exitEvent.WaitOne();
            }

        }


        static Process GlobalProcess;
        private static void TerabeePythonApiProcess()
        {
            kioskLog.SrushtyLog_Temp("Starting the Process");
            try
            {

                ProcessStartInfo start = new ProcessStartInfo();
                string root = Application.StartupPath;
                start.FileName = @"pythonw";
                start.Arguments = string.Format("{0}", root + @"\ThermalAPI\temperature.py");
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.CreateNoWindow = true;
                kioskLog.SrushtyLog_Temp("Starting the Process0");
                using (Process process = Process.Start(start))
                {
                    GlobalProcess = process;
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        kioskLog.SrushtyLog_Temp("cmd: " + result);
                    }
                }
                kioskLog.SrushtyLog_Temp("Starting the Process0 Started " + GlobalProcess.StartInfo);

            }
            catch (Exception PythonProcessException)
            {
                kioskLog.SrushtyLog_Temp("Process0 Start Failed Exp: " + PythonProcessException);
            }
        }

        public void GetTemperature(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Temp("Get Temperature command from UI");

            csession = gsession;
            client.Send("opencamera");
            testRunning = true;
            /* Random random = new Random();
             double tempHardcoded = random.NextDouble() * (36.7 - 36.3) + 36.3;
             csession.Send("Temperature "+ tempHardcoded);*/
        }

        public void CloseTemperature(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Temp("Close Temperature command from UI");
            try
            {
                kioskLog.SrushtyLog_Temp("CloseTemperature Close Camera 1");
                csession = gsession;
                client.Send("closecamera");
                testRunning = false;
            }
            catch (Exception) { }
        }

        static String evo_thermal;
        static String evo_mini;
        static int stoptemp = 0;
        static float AvgTemperature, totTemp;
        static bool testSkipped = false;
        public static void Receiveddata(String Message)
        {
            if (!Message.Contains("COM"))
            {
                kioskLog.SrushtyLog_Temp("Recevied data " + Message);
            }

            // System.Console.WriteLine("Recevied Data " + Message);
            try
            {
                if (Message.Contains("evo_thermal_port"))
                {
                    var details = JObject.Parse(Message);
                    //kioskLog.SrushtyLog_Temp("evo_thermal_port: " + details["evo_thermal_port"] + " evo_mini_port: " + details["evo_mini_port"]);
                    String str = Message;
                    String[] spearate = { "{", "'evo_thermal_port':", ",", "'evo_mini_port':", "}" };
                    String[] strlist1 = str.Split(spearate,
                        StringSplitOptions.RemoveEmptyEntries);

                    evo_mini = strlist1[1];
                    evo_thermal = strlist1[0];
                    evo_mini = evo_mini.Substring(evo_mini.IndexOf(':') + 1);
                    evo_thermal = evo_thermal.Substring(evo_thermal.IndexOf(':') + 1);
                    evo_thermal = evo_thermal.Trim(new Char[] { ' ', '"', '"' });
                    evo_mini = evo_mini.Trim(new Char[] { ' ', '"', '"' });

                    //kioskLog.SrushtyLog_Temp("evo_thermal port " + evo_thermal);
                    //kioskLog.SrushtyLog_Temp("evo_mini port " + evo_mini);

                    PortDetection.Set_Temperature_Thermal1detectmsg(evo_thermal);
                    PortDetection.Set_Temperature_Thermal2detectmsg(evo_mini);
                }
                else if (Message.Contains("DistanceValue"))
                {// TempValue,0.0 DistanceValue,40
                    String[] ThermalValue = Message.Split(' ');

                    String[] TemperatureValue = ThermalValue[0].Split(',');//TempValue,0.0
                    String[] DistanceValue = ThermalValue[1].Split(',');//DistanceValue,40

                    csession.Send("Temperature_Distance " + TemperatureValue[1] + " " + DistanceValue[1]);
                    float ThermalTemperature = float.Parse(TemperatureValue[1]);
                    if (ThermalTemperature != 0.0 && !testSkipped)
                    {
                        stoptemp++;
                        totTemp += ThermalTemperature;
                        if (stoptemp >= 3)
                        {
                            AvgTemperature = totTemp / stoptemp;
                            client.Send("closecamera");
                            kioskLog.SrushtyLog_Temp(stoptemp + " AvgTemperature " + AvgTemperature);
                            csession.Send("Temperature " + Math.Round(AvgTemperature, 2));
                            stoptemp = 0;
                            totTemp = 0;
                            testRunning = false;
                        }
                    }
                    else
                    {
                        stoptemp = 0;
                        totTemp = 0;
                    }
                }
                else if (Message.Contains("TemperatureSkipped"))
                {
                    //testSkipped = true;
                    csession.Send("Temperature Skipped");
                }
                else
                {
                    System.Console.WriteLine("port Error " + Message);
                }
            }
            catch (Exception exc)
            {
                kioskLog.SrushtyLog_Port("Webscoket Exception " + exc);
            }
        }

        public void Check_Temperature_Device(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Temp("Temperature Device start");
            using (var searcher = new ManagementObjectSearcher
                    ("SELECT * FROM WIN32_SerialPort"))
            {
                portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                var tList1 = (from n in portnames
                              join p in ports on n equals p["DeviceID"].ToString()
                              select p["Caption"]).ToList();
                if (tList1.Contains("Evo Mini") && tList1.Contains("Evo Thermal"))
                {
                    kioskLog.SrushtyLog_Temp("Temperature Device Connected");
                    gsession.Send("Temperature_Device_Connected");
                }
                else
                {
                    kioskLog.SrushtyLog_Temp("Temperature Device Not Connected");
                    gsession.Send("Temperature_Device_Not_Connected");
                }
            }

        }
        public static void ReadTemperature_open(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Temp("Temperature Started Camera");
            testSkipped = false;
            Thread_Temperature = new Thread(Serverstart);
            Thread_Temperature.Start();

        }


        public static void Restart_Process()
        {
            if (!testRunning)
            {
                kioskLog.SrushtyLog_Temp("Temperature Restarting the Backgorund Process");
                try
                {
                    foreach (System.Diagnostics.Process myProc in System.Diagnostics.
                    Process.GetProcesses())
                    {
                        if (myProc.ProcessName.Contains("python"))
                        {
                            myProc.Kill();
                            kioskLog.SrushtyLog_Temp("Processes: " + myProc);
                        }

                    }
                }
                catch (Exception processexception)
                {
                    kioskLog.SrushtyLog_Temp("Processes: " + processexception);
                }
                Thread.Sleep(2000);
                stoptemp = 0;
                totTemp = 0;
                Serverstart();
                testRunning = false;
            }
        }
    }
}
