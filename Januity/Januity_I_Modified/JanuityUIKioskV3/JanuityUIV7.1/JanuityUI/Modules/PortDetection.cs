using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using MANTRA;
using System.Threading.Tasks;
using System.Windows.Forms;
using USBClassLibrary;
using SuperWebSocket;
using System.Management;
using MTCMS;

namespace JanuityUI.Modules
{
    class PortDetection
    {
        //Serial Port 
        static SerialPort _serialPort_Weight, _serialPort_BP, _serialPort_SpO2, _serialPortlight;
        static Thread Thread_Bloodpressure, Thread_Weight, Thread_SpO2, read_light_port,Thread_Temperature, Thread_oDynamo, Thread_Fingerprint;
        static byte[] GetReadingByte = { 0x02, 0x31, 0x53, 0x03 };

        public static WebSocketSession csession;
        static byte[] ExpectedWeightResponseByte = { 0x02, 0x31, 0x20, 0x20 };
        //Light module
        static byte[] Get_lighthandshake = { 0x69 };
        private static byte[] Weight_GET_MODULE_DATA = new byte[] { 0x3A, 0x00, 0xC6 };
        private static byte[] GET_RETURN_STRING = new byte[] { 0x3A, 0x79, 0x02, 0x40, 0x0B };

        public static byte[] CMD_CheckBPModule = { 0xFA, 0x0A, 0x02, 0x02, 0x03, 0x06, 0x00, 0x00, 0x00, 0x17 };
        // Latest update starts
        public static int globali = 0;
        public static bool portfoundlight = false, Bloodpressure_Port_Found = false, Weight_Port_Found = false, SpO2_Port_Found = false, Temperature_Port_Found = false;

        public static byte[] Get_temprature = { 0x74 };
        public static byte[] Get_Temperature_Port = { 0x69 };
        public static bool timeoutexceptionOccured = false;
        private static SerialPort _serialPort = new SerialPort();

        public static Dictionary<string, string> device_status = new Dictionary<string, string>();

        public static String port_detection;

        public static String[] portss;

        //Nonin SpO2 
        private static USBClassLibrary.USBClass USBPort;
        private static System.Collections.Generic.List<USBClassLibrary.USBClass.DeviceProperties> ListOfUSBDeviceProperties;
        static bool NoninSpO2_USBDeviceConnected;

        static String NoninSpO2_VID_value = "1C3D", NoninSpO2_PID_Value = "0005"; // SPO2 DEVICE CODE
        static String oDynamo_VID_value = "1C3D", oDynamo_PID_Value = "0005"; // Payment1 DEVICE CODE

        //static String Temp_VID_value = "10C4", Temp_PID_Value = "EA60"; // TEMP DEVICE CODE
        static String Temp_VID_value = "0483", Temp_PID_Value = "5740";
        static String status = "";
        //oDynamo Payment Module
        private MTDevice mDevice;
        private MTConnectionType mConnectionType;
        private MTConnectionState mConnectionState;
        protected string mPINEntryDisplay = "";
        protected string mBaudRate = "9600";
        protected string mDataBits = "8";
        protected string mParity = "NONE";
        protected string mStopBits = "1";
        protected string mHandshake = "NONE";
        protected string mStartingByte = "";
        protected string mEndingByte = "0x0A";
        public static string key = "";
        public static void PortStart()
        {
            portss = System.IO.Ports.SerialPort.GetPortNames();
            _serialPort_BP = new SerialPort();
            _serialPort_Weight = new SerialPort();
            _serialPort_SpO2 = new SerialPort();
            _serialPortlight = new SerialPort();

            USBPort = new USBClass();
            ListOfUSBDeviceProperties = new List<USBClass.DeviceProperties>();
            NoninSpO2_USBDeviceConnected = false;

            Thread_Bloodpressure = new Thread(Detect_Bloodpressure_Port);
            Thread_Bloodpressure.Start();

            kioskLog.SrushtyLog_Port("Port Detection Started");
            
        }
        public static void PortStatus(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Port("Port Detection Status Request from UI");
            csession = gsession;
            csession.Send("device_status " + status);
        }  
      
        public static void Detect_Bloodpressure_Port()
        {
            kioskLog.SrushtyLog_Port("BP Port Search");
            //var portss = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var selectedPort in portss)
            {
                try
                {
                    TryPortBloodPressure(selectedPort);
                    if (Bloodpressure_Port_Found == true)
                    {
                        Set_Bloodpressure_portdetectmsg(selectedPort);
                        portss = portss.Where(val => val != selectedPort).ToArray();
                        JanuityUI.Modules.BloodPressure.ReadBloodPressure(selectedPort);

                        kioskLog.SrushtyLog_Port("BP Device Connected Port: " + selectedPort);

                       break;
                    }
                }
                catch (Exception BP_Exception)
                {
                    // csession.Send("Error Bloodpressure_Device_Error OtherException " + BP_Exception);
                    kioskLog.SrushtyLog_Port("BP Device Not Connected in the system | Error: " + BP_Exception);
                    
                }


            }

            if(Bloodpressure_Port_Found == true)
            {
                device_status.Add("BP_Device", "Connected");
            }
            else
            {
                device_status.Add("BP_Device", "Not Connected");
                Set_Bloodpressure_portmissing();
            }


            Thread_Weight = new Thread(Detect_Weight_Port);
            Thread_Weight.Start();
        }

        public static void Detect_Weight_Port() // Thread Port Detection for Weight
        {
            //var portss = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var selectedPort in portss)
            {
                try
                {
                    TryPortWeight(selectedPort);

                    if (Weight_Port_Found == true)
                    {


                        Set_Weight_portdetectmsg(selectedPort);
                        portss = portss.Where(val => val != selectedPort).ToArray();

                        JanuityUI.Modules.WeightScale.ReadWeightScale(selectedPort);

                        kioskLog.SrushtyLog_Port("Weight Device Connected Port: " + selectedPort);

                        break;
                    }
                }
                catch (NullReferenceException e)
                {
                    kioskLog.SrushtyLog_Port("Weight Device Not Connected Error: " + e);
                    if (csession != null)
                    {
                        csession.Send("Error Weight_Device_Error NullReferenceException " + e);
                    }
                }
                catch (Exception Weight_Exception)
                {
                    kioskLog.SrushtyLog_Port("Weight Device Not Connected in the system | Error: " + Weight_Exception);
                }

            }
            if (Weight_Port_Found == true)
            {
                device_status.Add("Weight_Device", "Connected");
            }
            else {
                device_status.Add("Weight_Device", "Not Connected");
                Set_Weight_portmissing();
            }
            Thread_SpO2 = new Thread(Detect_spo2_Port);
            Thread_SpO2.Start();
        }

        public static void Detect_spo2_Port() // Thread Port Detection for SpO2
        {
            if (Connect_USBTryMyDeviceConnection(NoninSpO2_VID_value, NoninSpO2_PID_Value)) {
                Set_SpO2_portdetectmsg(port_detection);
                portss = portss.Where(val => val != port_detection).ToArray();
                System.Console.WriteLine("just checking" +portss);
                JanuityUI.Modules.SpO2.Spo2PortOpen(port_detection, csession);

                kioskLog.SrushtyLog_Port("SPO2 Device Connected Port: " + port_detection);

                device_status.Add("SP02_Device", "Connected");

            }
            else
            {
                kioskLog.SrushtyLog_Port("SPO2 Device Not Connected in the system");

                device_status.Add("SP02_Device", "Not Connected");
                Set_SpO2_portmissing();
            }

            read_light_port = new Thread(LightLED);
            read_light_port.Start();

        }
        private static void LightLED()
        {
            // var portss = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var port in portss)
            {
                try
                {
                    TryPortlight(port);
                    if (portfoundlight == true)
                    {
                        Set_LightModule_portdetectmsg(port);
                        kioskLog.SrushtyLight_Port("Light Port Detected: " + port);
                        device_status.Add("Light_Module", "Connected");
                        //PortDetection.selectedPort = port;
                        JanuityUI.Modules.LightLed.LightPortOpen(port);
                        kioskLog.SrushtyLight_Port("Light Port Detected2: " + port);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Set_LightModule_portmissing();
                    kioskLog.SrushtyLight_Port("Light Port Exception: " + ex);
                }
            }


            if (!portfoundlight)
            {
                Set_LightModule_portmissing();
                device_status.Add("Light Module", "Not Connected");
            }

            Thread_Temperature = new Thread(Detect_Temperature_Port);
            Thread_Temperature.Start();
        }

        public static void Detect_Temperature_Port()
        {
            JanuityUI.Modules.Temperature.ReadTemperature_open(csession);
            Set_Temperature_port2detectmsg("ADB");
            Thread_oDynamo = new Thread(Detect_TemperatureThermal_Port); // To start finger print thread
            Thread_oDynamo.Start();
        }

        public static void Detect_TemperatureThermal_Port()
        {
            JanuityUI.Modules.TemperatureThermal.ReadTemperature_open(csession);
            Set_Temperature_Thermal1detectmsg("Searching");
            Set_Temperature_Thermal2detectmsg("Searching");
            Thread_oDynamo = new Thread(Detect_oDynamo_Port); // To start finger print thread
            Thread_oDynamo.Start();
        }

        public static void Detect_oDynamo_Port()
        {
            //kioskLog.SrushtyLog_Port("oDynamo Searching");
            //JanuityUI.Modules.oDynanoPayment.PaymentInit();

            Thread_Fingerprint = new Thread(Detect_FingerPrint_Port); // To start finger print thread
            Thread_Fingerprint.Start();
        }

        public static void Detect_FingerPrint_Port()
        {
            kioskLog.SrushtyLog_Port("FingerPrint Searching");
            MFS100 mfs100 = new MFS100(key);
            try
            {
                if (mfs100.IsConnected())
                {

                    Set_Fingerprint_portdetectmsg("COM");

                    DeviceInfo deviceInfo = mfs100.GetDeviceInfo();
                    
                    string scannerInfo = "SERIAL NO.: " + deviceInfo.SerialNo + "     MAKE: " + deviceInfo.Make + "     MODEL: " + deviceInfo.Model + "\nWIDTH: " + deviceInfo.Width.ToString() + "     HEIGHT: " + deviceInfo.Height.ToString() + "     CERT: " + mfs100.GetCertification();
                    JanuityUI.Modules.FingerPrint.Fingerprint();

                    kioskLog.SrushtyLog_Port("FingerPrint Device Connected Sucessfuly | Response: " + scannerInfo);

                    device_status.Add("Fingerprint_Device", "Connected");

                }
                else
                {
                    kioskLog.SrushtyLog_Port("FingerPrint Device Response Not connected");
                    device_status.Add("Fingerprint_Device", "Not Connected");
                    Set_Fingerprint_portmissing();
                    Set_oDynamo_portmissing();
                    Set_Dynawave_portmissing();
                }
            }
            catch (Exception ex)
            {
                if (csession != null)
                {
                    csession.Send("Error Fingerprint_Device_Error OtherException " + ex);
                }
                kioskLog.SrushtyLog_Port("FingerPrint Devide is not Connected | Exception" + ex);
                device_status.Add("Fingerprint Device:", "Not Connected");
                Set_Fingerprint_portmissing();
                Set_oDynamo_portmissing();
                Set_Dynawave_portmissing();
            }
            

            for (int i = 0; i < device_status.Count; i++)
            {
                status+= device_status.ElementAt(i).Key + ":" + device_status.ElementAt(i).Value+" ";
            }


            string joined = string.Join(" ", device_status);
            if (csession != null)
            {
                csession.Send("device_status " + status);
            }

            kioskLog.SrushtyLog_Port("Port Detection Fully Completed");
        }
        public static string SendCommands(byte[] command, SerialPort _serialPortlight)
        {
            if (!_serialPortlight.IsOpen)
            {
                _serialPortlight.Open();
            }
            _serialPortlight.Write(command, 0, command.Length);
            _serialPortlight.DiscardInBuffer();
            _serialPortlight.DiscardOutBuffer();
           // Thread.Sleep(500);
            int response = _serialPortlight.ReadByte();
            return response.ToString();
        }
        private static bool TryPortlight(string port)
        {
            _serialPortlight.PortName = port;
            _serialPortlight.BaudRate = 9600;
            _serialPortlight.ReadTimeout = 8000;
            _serialPortlight.WriteTimeout = 8000;
           // Thread.Sleep(3500);
            try
            {
                if (!_serialPortlight.IsOpen)
                {
                    _serialPortlight.Open();
                }
                _serialPortlight.DiscardInBuffer();
                _serialPortlight.DiscardOutBuffer();
              //  Thread.Sleep(3500);
                string response_for_id = "";
                response_for_id = SendCommands(Get_lighthandshake, _serialPortlight);
                if (response_for_id.Contains("100"))
                {
                    portfoundlight = true;
                }
                else
                {
                    Set_LightModule_portmissing();
                }
            }
            catch (TimeoutException tx)
            {
            }
            catch (Exception ex)
            {
            }

            finally
            {
                if (_serialPortlight.IsOpen)
                {
                    _serialPortlight.Close();
                }
            }
            return portfoundlight;
        }
        public static void TryPortBloodPressure(string port)
        {
            _serialPort.PortName = port;
            _serialPort.BaudRate = 9600;
            _serialPort.ReadTimeout = 8000;
            _serialPort.WriteTimeout = 8000;
            Thread.Sleep(200);
            try
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.Open();
                }
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                Thread.Sleep(200);
                string response_for_id = "";
                response_for_id = SendCommand(Weight_GET_MODULE_DATA, _serialPort);

                if (response_for_id.Contains("62"))
                {
                    SendCommand(GET_RETURN_STRING, _serialPort);
                    Bloodpressure_Port_Found = true;
                }
                else if (response_for_id.Contains("100"))
                {
                    // Weight_Port_Found = true;
                }
            }
            catch (TimeoutException BP_Exception)
            {
                //csession.Send("Error Bloodpressure_Device_Error TimeoutException " + BP_Exception);

                kioskLog.SrushtyLog_Port("BP TPort Exception" + BP_Exception);
            }
            catch (Exception BP_Exception)
            {
                csession.Send("Error Bloodpressure_Device_Error OtherException " + BP_Exception);

                kioskLog.SrushtyLog_Port("BP TPort Exception" + BP_Exception);
            }
            finally
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
        }
        public static void TryPortWeight(String port)//Check every port
        {
            _serialPort.PortName = port;
            _serialPort.BaudRate = 9600;
            _serialPort.ReadTimeout = 6000;
            _serialPort.WriteTimeout = 6000;

            int i,TryTwiceOnPort=0;
            for (i = 0; i < 3; i++)
            {
                string DeviceResponse = GetIdentifyDevice(_serialPort);

                byte[] DeviceResponseByte = Encoding.ASCII.GetBytes(DeviceResponse);
                kioskLog.SrushtyLog_Port(_serialPort.PortName + " Weight Response " + DeviceResponse + " " + DeviceResponseByte);

                if (DeviceResponseByte.Length >= 4)
                {
                    TryTwiceOnPort++;
                    byte[] response4Byte = new byte[4];
                    Array.Copy(DeviceResponseByte, response4Byte, response4Byte.Length);

                    int DataSTX;
                    for (DataSTX = 0; DataSTX < response4Byte.Length; DataSTX++)
                    {
                        if (ExpectedWeightResponseByte[DataSTX] != response4Byte[DataSTX])
                        {
                            kioskLog.SrushtyLog_Port(_serialPort.PortName + " Weight Response not Expected " + DataSTX);

                            break;
                        }
                    }

                    if (DataSTX < 3)
                    {
                        kioskLog.SrushtyLog_Port(_serialPort.PortName + " Weight Response DataSTX is < 3");
                        if (TryTwiceOnPort>1) {
                            break;
                        } else {
                            i = 0;
                            continue;
                        }
                        
                    }
                    else
                    {
                        Weight_Port_Found = true;
                        return;
                    }
                }
                else continue;
            }
            throw new Exception("Weight Invalid response");
        }

        private static void Set_Bloodpressure_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label24", text);//Port name Set
            JanuityKiosk.StaticUI("label33", "Port Detected");// Status Set
        }
        private static void Set_Bloodpressure_portmissing()
        {
            JanuityKiosk.StaticUI("label33", "Module Missing");// Status Set
        }
        private static void Set_Weight_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label25", text);//Port name Set
            JanuityKiosk.StaticUI("label34", "Port Detected");// Status Set
        }
        private static void Set_Weight_portmissing()
        {
            JanuityKiosk.StaticUI("label34", "Module Missing");// Status Set
        }
        private static void Set_SpO2_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label26", text);//Port name Set
            JanuityKiosk.StaticUI("label35", "Port Detected");// Status Set
        }
        private static void Set_SpO2_portmissing()
        {
            JanuityKiosk.StaticUI("label35", "Module Missing");// Status Set
        }

        public static void Set_LightModule_portmissing()
        {
            JanuityKiosk.StaticUI("label36", "Module Missing");// Status Set
        }
        private static void Set_LightModule_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label27", text);//Port name Set
            JanuityKiosk.StaticUI("label36", "Port Detected");// Status Set
        }
        public static void Set_Temperature_portmissing(string text)
        {
            JanuityKiosk.StaticUI("label46", text);// Status Set
            kioskLog.SrushtyLog_Temp("Disconnected");
        }
        public static void Set_Temperature_port2detectmsg(string text)
        {
            JanuityKiosk.StaticUI("label45", text);//Port name Set
            JanuityKiosk.StaticUI("label46", "DETECTED");// Status Set
            kioskLog.SrushtyLog_Temp("Searching");
        }
        public static void Set_Temperature_port2missing(string text)
        {
            JanuityKiosk.StaticUI("label46", text);// Status Set

            kioskLog.SrushtyLog_Temp(text);
        // JanuityKiosk.StaticUI("label46", "Module Missing");// Status Set

         }
        public static void Set_Temperature_Thermal1detectmsg(string text)
        {
            JanuityKiosk.StaticUI("label30", text);//Port name Set
            JanuityKiosk.StaticUI("label39", "Seraching");// Status Set
        }
        public static void Set_Temperature_Thermal1missing(string text)
        {
            JanuityKiosk.StaticUI("label39", text);// Status Set
        }

        public static void Set_Temperature_Thermal2detectmsg(string text)
        {
            JanuityKiosk.StaticUI("label48", text);//Port name Set
            JanuityKiosk.StaticUI("label50", "Seraching");// Status Set
        }
        public static void Set_Temperature_Thermal2missing(string text)
        {
            JanuityKiosk.StaticUI("label50", text);// Status Set
        }

        private static void Set_oDynamo_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label28", text);//Port name Set
            JanuityKiosk.StaticUI("label37", "Port Detected");// Status Set
        }
        private static void Set_oDynamo_portmissing()
        {
            JanuityKiosk.StaticUI("label37", "Module Missing");// Status Set
        }
        private static void Set_Dynawave_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label29", text);//Port name Set
            JanuityKiosk.StaticUI("label38", "Port Detected");// Status Set
        }        
        private static void Set_Dynawave_portmissing()
        {
            JanuityKiosk.StaticUI("label38", "Module Missing");// Status Set
        }

        private static void Set_Fingerprint_portdetectmsg(string text)
        {
            JanuityKiosk.StaticUI("label30", text);//Port name Set
            JanuityKiosk.StaticUI("label39", "Port Detected");// Status Set
        }
        private static void Set_Fingerprint_portmissing()
        {
            JanuityKiosk.StaticUI("label39", "Module Missing");// Status Set

        }

        public static string GetIdentifyDevice(SerialPort _serialPort)
        {
            string response = string.Empty;
            try
            {
                _serialPort.ReadTimeout = 5000;
                _serialPort.WriteTimeout = 5000;
                _serialPort.Open();

                Thread.Sleep(100);
                _serialPort.Write(GetReadingByte, 0, GetReadingByte.Length);
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
                Thread.Sleep(100);
                response = _serialPort.ReadLine();
            }
            catch (TimeoutException t)
            {
                //MessageBox.Show(t.ToString(), "Information");
                //csession.Send("Error GetIdentifyDevice TimeoutException " + t);
            }
            finally
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            return response;
        }
        public static string SendCommand(byte[] command, SerialPort _serialPort)
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
            _serialPort.Write(command, 0, command.Length);
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            Thread.Sleep(100);
            int response = _serialPort.ReadByte();
            return response.ToString();
        }
        #region USB
        private static bool Connect_USBTryMyDeviceConnection(string VID_Value, string PID_Value)
        {
            Nullable<UInt32> MI = 0;
            MI = null;
            if (USBClass.GetUSBDevice(uint.Parse(VID_Value, System.Globalization.NumberStyles.AllowHexSpecifier), uint.Parse(PID_Value, System.Globalization.NumberStyles.AllowHexSpecifier), ref ListOfUSBDeviceProperties, true, MI))
            {
                //NumberOfFoundDevicesLabel.Text = "Number of found devices: " + ListOfUSBDeviceProperties.Count.ToString() + " " + ListOfUSBDeviceProperties[0].COMPort + "" + ListOfUSBDeviceProperties[0].FriendlyName;
                port_detection = ListOfUSBDeviceProperties[0].COMPort;
                return true;
            }
            else
            {
                port_detection = null;
                return false;
            }
        }
   
        #endregion

    }
}
