using System;
using System.Collections.Generic;
using System.IO.Ports;
using SuperWebSocket;
using SuperWebSocket.PubSubProtocol;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;
using System.Timers;

namespace JanuityUI.Modules
{
    class SpO2
    {
        static SerialPort _serialPort_SpO2;
        public static WebSocketSession csession;
        static Thread Thread_SpO2;
        static System.Timers.Timer aTimer;
        static String SpO2_incomingdata2 = "";
        static int SpO2_number_of_char_incoming = 0, SpO2_packet_length_chk = 0;
        static int  SpO2_Pass_count = 0, SpO2_HighValue=0,SpO2_Error_Count=0;
        static int SpO2_Packet_length = 0, SpO2_Packet_status = 0, SpO2_Packet_PI = 0, SpO2_Packet_counter = 0, SpO2_Packet_SpO2data = 0, SpO2_Packet_pulse = 0;
        public static bool device_status;

        static JanuityUI.Modules.LightLed Light_Connect;
        static Boolean SpO2_Status_Correct_Check = false, SpO2_DataRequest = false;
        public static void Spo2PortOpen(String port, WebSocketSession gsession)//Check every port
        {
            csession = gsession;

            Light_Connect = new JanuityUI.Modules.LightLed();
            _serialPort_SpO2 = new SerialPort();
            _serialPort_SpO2.PortName = port;
            _serialPort_SpO2.BaudRate = 9600;
            _serialPort_SpO2.ReadTimeout = 4500;
            _serialPort_SpO2.WriteTimeout = 4500;
            Thread.Sleep(200);
                                   
            aTimer = new System.Timers.Timer(2000); //one hour in milliseconds
            aTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
            

            try
            {
                if (!_serialPort_SpO2.IsOpen)
                {
                    _serialPort_SpO2.Open();
                }
                _serialPort_SpO2.DiscardInBuffer();
                _serialPort_SpO2.DiscardOutBuffer();
                device_status = true;

            }
            catch (TimeoutException e)
            {
                //csession.Send("Error Sp02_Device_Error TimeoutException " + e);
                kioskLog.SrushtyLog_SpO2("TimeoutException: " + e);
                device_status = false;
            }
            catch (Exception e)
            {
                if (csession != null)
                {
                    csession.Send("Error Sp02_Device_Error Other " + e);
                }
                device_status = false;
                kioskLog.SrushtyLog_SpO2("Exception: " + e);
            }
            finally
            {
                Thread_SpO2 = new Thread(Read_SpO2_Value);
                Thread_SpO2.Start();
            }
        }




        public static void Read_SpO2_Value()
        {
            kioskLog.SrushtyLog_SpO2("SpO2 Reading Started");
            while (true)
            {
                try
                {
                    Byte SpO2_hex_in_data = (Byte)_serialPort_SpO2.ReadByte();
                    String SpO2_Stng_indata = SpO2_hex_in_data.ToString();
                    SpO2_number_of_char_incoming++;
                    if (SpO2_hex_in_data == 0x03)
                    {
                        String[] data = SpO2_incomingdata2.Split(',');
                        kioskLog.SrushtyLog_SpO2("incoming" + SpO2_incomingdata2);
                        SpO2_Packet_length = (int.Parse(data[1]) + 2);
                        if (data.Length == SpO2_Packet_length)
                        {
                            kioskLog.SrushtyLog_SpO2("Data Packet Length Check Passed");
                            SpO2_Packet_status = int.Parse(data[2]);
                            string Status_bit = Convert.ToString(SpO2_Packet_status, 2).PadLeft(8, '0');
                            SpO2_Device_Status(Status_bit);
                            SpO2_Packet_PI = int.Parse(data[4] + data[5]);
                            SpO2_Packet_counter = int.Parse(data[6] + data[7]);
                            SpO2_Packet_SpO2data = int.Parse(data[8]);
                            SpO2_Packet_pulse = int.Parse(data[10]) + (int.Parse(data[9]) << 8);


                            if (SpO2_Packet_SpO2data != 127 && SpO2_Status_Correct_Check && SpO2_Packet_SpO2data > 50 && SpO2_Packet_pulse != 511)
                            {
                                SpO2_Pass_count++;
                                if (SpO2_DataRequest) {
                                    if (SpO2_HighValue < SpO2_Packet_SpO2data) {
                                        SpO2_HighValue = SpO2_Packet_SpO2data;
                                    }
                                    if (SpO2_Pass_count>5) {
                                        Light_Connect.Stop_LED();
                                        csession.Send("SpO2 " + SpO2_HighValue + " Pulse "+ SpO2_Packet_pulse);
                                        SpO2_DataRequest = false;
                                        SpO2_Pass_count = 0;
                                        SpO2_HighValue = 0;
                                        SpO2_Error_Count = 0;
                                        aTimer.Stop();
                                    }                                    
                                }
                                
                                kioskLog.SrushtyLog_SpO2("SpO2 correct" + SpO2_Packet_SpO2data +" SpO2 Pulse "+ SpO2_Packet_pulse+" SpO2 Count: "+SpO2_Pass_count);
                                device_status = true;
                                SpO2_Packet_SpO2data = 0;
                            }
                            else
                            {
                                SpO2_Pass_count = 0;
                                if (SpO2_DataRequest)
                                {
                                    csession.Send("SpO2Error Insert Finger");
                                }                                    
                                kioskLog.SrushtyLog_SpO2("SpO2Error " + SpO2_Packet_SpO2data);
                                //device_status = false;
                            }
                            kioskLog.SrushtyLog_SpO2("SpO2 Length " + SpO2_Packet_length + " Status: " + Status_bit + " PI: " + SpO2_Packet_PI + " Counter: " + SpO2_Packet_counter + " SpO2: " + SpO2_Packet_SpO2data);

                        }
                        else
                        {
                            SpO2_Pass_count = 0;
                            kioskLog.SrushtyLog_SpO2("Data Packet Length Check Failed" + data.Length + "" + SpO2_Packet_length);
                        }


                        SpO2_incomingdata2 = "";
                        SpO2_number_of_char_incoming = 0;
                    }
                    else
                    {
                        if (SpO2_number_of_char_incoming == 2)
                        {
                            SpO2_packet_length_chk = int.Parse(SpO2_Stng_indata);
                        }
                        SpO2_incomingdata2 += SpO2_Stng_indata + ",";
                        SpO2_Packet_length--;
                    }
                }
                catch (NullReferenceException e)
                {
                    Light_Connect.Stop_LED();
                    if (csession != null)
                    {
                        csession.Send("Error Sp02_Device_Error NullReferenceException " + e);
                    }
                    else
                    {
                        System.Console.Write("Websocket Not Connected");
                        kioskLog.SrushtyLog_Weight("Websocket Not Connected");
                    }

                    device_status = false;
                }
                catch (TimeoutException)
                { }
                catch (InvalidOperationException)
                {
                    device_status = false;
                }
                catch (IOException)
                {
                    device_status = false;
                }
                catch (IndexOutOfRangeException arrayexception)
                {
                    kioskLog.SrushtyLog_SpO2(typeof(Program).Name + " Exception: " + arrayexception);
                }
                catch (Exception ee)
                {
                    if (csession != null)
                    {
                        csession.Send("Error Sp02_Device_Error " + typeof(Program).Name + " " + ee);
                    }
                    else
                    {
                        System.Console.Write("Websocket Not Connected");
                        kioskLog.SrushtyLog_Weight("Websocket Not Connected");
                    }

                    kioskLog.SrushtyLog_SpO2(typeof(Program).Name + " Exception: " + ee);
                }
            }
        }

        public static void OnTimerEvent(object source, EventArgs e)
        {
            SpO2_Error_Count++;
            if (SpO2_Error_Count>30) {
                SpO2_Error_Count = 0;
                aTimer.Stop();
                SpO2_DataRequest = false;
                csession.Send("SpO2Error Please Retry");
            } else {
                csession.Send("SpO2Error Insert Finger");
            }            
        }

        public void getSpO2(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_SpO2("Get SpO2 Value Request from UI");
            Thread.Sleep(1000);
            csession = gsession;
            try { aTimer.Start(); } catch (Exception ecs) { }
           
            SpO2_DataRequest = true;
            kioskLog.SrushtyLog_SpO2("SpO2: " + SpO2_Packet_SpO2data + "");
        }

        public void getSpO2Status(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_SpO2("Get SpO2 Status Request from UI");
            Thread.Sleep(1000);
            csession = gsession;           

            if (device_status)
            {
                csession.Send("SpO2_Device_Connected");
                kioskLog.SrushtyLog_SpO2("SpO2 Status: SpO2_Device_Connected");
            }
            else
            {
                csession.Send("SpO2_Device_Not_Connected");
                kioskLog.SrushtyLog_SpO2("SpO2 Status: SpO2_Device_Not_Connected");
            }
        }

        public static void Close_SpO2_Port()
        {
            try
            {
                if (_serialPort_SpO2 != null && _serialPort_SpO2.IsOpen)
                {
                    _serialPort_SpO2.Close();
                }
            }
            catch (Exception ex)
            {
                kioskLog.SrushtyLog_Bloodpressure("Bloodpressure Port Close Exception" + ex);
            }
        }
        public static void SpO2_Device_Status(String Status_bit)
        {
            for (int i = 0; i < Status_bit.Length; i++)
            {
                // kioskLog.SrushtyLog_SpO2("bit: " + i + " : " + Status_bit[i]);
                if (i == 3)
                {
                    if (Status_bit[i] == '1')
                    {
                        kioskLog.SrushtyLog_SpO2("Correct Check Pass");
                        SpO2_Status_Correct_Check = true;
                    }
                    else
                    {
                        kioskLog.SrushtyLog_SpO2("Correct Check Fail,Slide Your Finger Futher");
                        SpO2_Status_Correct_Check = false;
                    }
                }
                else if (i == 4)
                {
                    if (Status_bit[i] == '1')
                    {
                        kioskLog.SrushtyLog_SpO2("Oximeter is searching for a consecutive pulse signals");
                    }
                }
                else if (i == 5)
                {
                    if (Status_bit[i] == '1')
                    {
                        kioskLog.SrushtyLog_SpO2("Data successfully passed the SmartPoint Algorithm");
                    }
                }
                else if (i == 6)
                {
                    if (Status_bit[i] == '1')
                    {
                        kioskLog.SrushtyLog_SpO2("Pulse Signal Strenth is Low");
                    }
                }
                else if (i == 7)
                {
                    if (Status_bit[i] == '1')
                    {
                        kioskLog.SrushtyLog_SpO2("Indicates that the display is in sync mode with the collector");
                    }
                }
            }
        }
    }
}
