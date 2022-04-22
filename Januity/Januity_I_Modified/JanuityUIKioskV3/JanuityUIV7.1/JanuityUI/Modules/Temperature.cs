using System;
using SuperWebSocket;
using System.Threading;
using Websocket.Client;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Timers;


namespace JanuityUI.Modules
{
    internal class Temperature
    {

        static Thread Thread_Temperature, cmd;
        public static WebSocketSession csession;
        static Process adb = null;
        static bool temp_status = false;
        static bool temp_status2 = false;
        static JanuityUI.Modules.LightLed Light_Connect;
        static System.Timers.Timer myTimer;
        static string Temp_Value = "";
        static string Temp_Value1 = "";
        



        public static void IhealthDeviceProcess_Start()
        {
           
            kioskLog.SrushtyLog_Temp("Starting the Process");   //print the value in kiosk
            Light_Connect = new JanuityUI.Modules.LightLed();   // new function created

            if (adb != null && !adb.HasExited) // checking android debug bridge not equalto null and adb not exit means return
                return;
           
            adb = new Process(); // new process 
            adb.StartInfo.UseShellExecute = false;
            adb.StartInfo.FileName = @"C:\adb\adb.exe";
            adb.StartInfo.Arguments = "logcat | findstr com.ihealth.demo";
            adb.StartInfo.RedirectStandardOutput = true;
            adb.StartInfo.RedirectStandardError = true;
            adb.EnableRaisingEvents = true;
            adb.StartInfo.CreateNoWindow = true;
            adb.ErrorDataReceived += new DataReceivedEventHandler(adb_ErrorDataReceived);
            adb.OutputDataReceived += new DataReceivedEventHandler(adb_OutputDataReceived);

            try
            {
                var started = adb.Start();

            }
            catch (Exception Exception)
            {
                kioskLog.SrushtyLog_Temp("Process0 Start Failed Exp: " + Exception);
            }

            adb.BeginErrorReadLine();
            adb.BeginOutputReadLine();
        }
        public static void adb_OutputDataReceived(object sender, DataReceivedEventArgs e    )
            {
            // kioskLog.SrushtyLog_Temp("Log Output" + e.Data);
           
             
                try
            {

                if (e.Data.Contains("waiting for device"))
                {
                    Set_Temperature_portNotdetectmsg("ADB");
                    kioskLog.SrushtyLog_Temp("Temp Device has been power off");
                    csession.Send("TemperatureStatus Disconnected");
                    temp_status = false;
                    

                }
                else if (e.Data.Contains("action_temperature_measurement -"))
                {
                    Set_Temperature_portdetectmsg("ADB");
                    temp_status = true;
               
                    int startindex = e.Data.IndexOf("{");
                    if (startindex != 0)
                    {
                        int Endindex = e.Data.IndexOf('}');
                        string outputstring = "{" + e.Data.Substring(startindex + 1, Endindex - startindex - 1) + "}";

                        JObject json = JObject.Parse(outputstring);

                        string Tbody = json["Tbody"].Value<string>();
                        string Tobj = json["Tobj"].Value<string>();
                        string Tamb = json["Tamb"].Value<string>();
                        string Tex = json["Tex"].Value<string>();
                        string Distance = json["Distance"].Value<string>();
                        string voltage = json["voltage"].Value<string>();
                        

                        kioskLog.SrushtyLog_Temp("Tbody :" + Tbody + " Tobj " + Tobj + " Tamb " + Tamb + " Tex " + Tex + " Tobj " + Distance + " Distance " + voltage + " voltage ");
                        Light_Connect.Stop_LED();
                        String TempTBody = Tbody.Insert(2, ".");
                        decimal IHealthBodyTemp = decimal.Parse(TempTBody);
                        kioskLog.SrushtyLog_Temp("Temp Value: " + IHealthBodyTemp);
                        
                        csession.Send("Temperature " + Math.Round(IHealthBodyTemp, 1));
                        csession.Send(TempTBody);
                
                    }
                }
                else if (e.Data.Contains("The device has been connected"))
                {
                    kioskLog.SrushtyLog_Temp("Temp device has been connected");
                  //  kioskLog.SrushtyLog_Temp("Raspberry has been connected");
                    csession.Send("TemperatureStatus Connected");
                    temp_status = true;
                  //  temp_status2 = true;
                  
                    Set_Temperature_portdetectmsg("ADB");
                }
                else if (e.Data.Contains("The device has been disconnected"))
                {
                    Set_Temperature_portNotdetectmsg("ADB");
                    kioskLog.SrushtyLog_Temp("Temp Device has been disconnected");
                    csession.Send("TemperatureStatus Disconnected");
                    temp_status = false;
                }
                }
                catch (Exception ex)
                {
                    kioskLog.SrushtyLog_Temp("Temp Exception: " + ex);
                }
            
            }
        
        public static void adb_ErrorDataReceived(object sender, DataReceivedEventArgs e)
            {
                try {

                kioskLog.SrushtyLog_Temp("Log ErrorData Connect: " + e.Data);

                

                if (string.IsNullOrEmpty(e.Data)) {
                    kioskLog.SrushtyLog_Temp("Temp e.Data as error is empty");
                    Set_Temperature_portNotdetectmsg("ADB");
                   
                    temp_status = false;
                    kioskLog.SrushtyLog_Temp(Temp_Value);
                    /*temp_status2 = true;
                    if (temp_status2 == true)
                    {
                        Temp_Value1 = "Raspberry_pi is connected";

                    }
                    else
                    {
                        Temp_Value1 = "Raspberry_pi is disconnected";
                    }

                    kioskLog.SrushtyLog_Temp(Temp_Value1);
*/

                    IhealthDeviceProcess_Stop();
                    IhealthDeviceProcess_Clear();
                    IhealthDeviceProcess_Start();
                }

                
                else if (e.Data.Contains("waiting"))
                {                    
                    Set_Temperature_portNotdetectmsg("ADB");
                    temp_status = false;
                   // temp_status2 = false;
                    kioskLog.SrushtyLog_Temp(Temp_Value);
                    kioskLog.SrushtyLog_Temp("Temp adb201 as error restarting");
                    JanuityKiosk.StaticUI("label46", "Module Missing");// Status Set
                    IhealthDeviceProcess_Clear();
                    IhealthDeviceProcess_Start();
                }
                /*else if (e.Data.Contains("Device Connected"))
                {
                    Set_Temperature_portNotdetectmsg("ADB");
                    temp_status2 = true;
                    kioskLog.SrushtyLog_Temp("Rasberry pi is connected");
                   
                }*/
                }
                catch (Exception ex) {
                Set_Temperature_portNotdetectmsg("ADB");
                kioskLog.SrushtyLog_Temp("Log ErrorData Exception: " + ex);
                temp_status = false;
            }
            }



        public static void IhealthDeviceProcess_Clear()
        {
            if (adb == null || adb.HasExited)
            {

                kioskLog.SrushtyLog_Temp("Raspberry1 Device connected");
                try
                {
                    adb = new Process();
                    adb.StartInfo.UseShellExecute = false;
                    adb.StartInfo.FileName = @"C:\adb\adb.exe";//
                    adb.StartInfo.Arguments = "logcat -c";
                    adb.StartInfo.CreateNoWindow = true;
                    adb.Start();
                    adb.WaitForExit(1500);
                }
                catch { }
            }
        }

        public static void IhealthDeviceProcess_Stop()
        {
            if (adb != null && !adb.HasExited)
            {
                kioskLog.SrushtyLog_Temp("Raspberry1 Device Disconnected");
                adb.Kill();
            }
        } 

        
        public static void Set_Temperature_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label45", text);//Port name Set
          JanuityKiosk.StaticUI("label46", "Device Connected");// Status Set
        }
        
        public static void Set_Temperature_portNotdetectmsg(string text)
        {

            JanuityKiosk.StaticUI("label45", text);//Port name Set
             
            JanuityKiosk.StaticUI("label46", text);// Status Set
        }

      
    public void Check_Temperature_Device(WebSocketSession gsession)
        {
             csession= gsession;
            try
            {

               /* kioskLog.SrushtyLog_Temp("Check_Rasberry_piTemperature_Device: " + temp_status2);
                if (temp_status2 == true)
                {
                    csession.Send("Raspberry_pi is Connected");

                    gsession.Send(Temp_Value1);

                }*/
                // Thread.Sleep(2000);
                kioskLog.SrushtyLog_Temp("Check_ihealthTemperature_Device: " + temp_status);
                
              


                if (temp_status == true )
                {
                    csession.Send("TemperatureStatus Connected");
                   
                    gsession.Send(Temp_Value);
                    
                }

               /* else if(temp_status == false && temp_status2 == false )
                {
                    csession.Send("TemperatureStatus Disconnected");
                    gsession.Send("raspberry_pi is DisConnected  ");
                    gsession.Send(Temp_Value);
                }
                else if (temp_status == true && temp_status2 == false)
                {
                    csession.Send("TemperatureStatus Disconnected");
                    gsession.Send("raspberry_pi is DisConnected  ");
                    gsession.Send(Temp_Value);
                }
                else if (temp_status == false && temp_status2 == true)
                {
                   
                    gsession.Send("raspberry_pi is Connected  ");
                    gsession.Send(Temp_Value);
                }*/
            }
            catch (Exception e)
            {
                kioskLog.SrushtyLog_Temp("Log ErrorData: " + e);
            }
        }
        public static void ReadTemperature_open(WebSocketSession gsession)
        {
            csession = gsession;
            kioskLog.SrushtyLog_Temp("Temperature Started iHealth ");
            //testSkipped = false;
            Thread_Temperature = new Thread(IhealthDeviceProcess_Start);
            Thread_Temperature.Start();
           // gsession.Send(Temp_Value1);




        }

        



    }

}
