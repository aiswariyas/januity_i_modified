using SuperWebSocket;
using SuperWebSocket.PubSubProtocol;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JanuityUI.Modules
{
    class BloodPressure
    {
        static SerialPort _serialPort_BP;
        static Thread Thread_Bloodpressure;
        public static WebSocketSession csession;

        static JanuityUI.Modules.LightLed Light_Connect;
        /// Sets initial inflation level to 160mmHg (appropriate for adult). This is called once for each patient.
        private static byte[] SET_INITIAL_INFLATE_ADULT = new byte[] { 0x3A, 0x17, 0xA0, 0x00, 0x0F };

        /// Engages the cuff for an adult greater than 13 years old
        private static byte[] START_BP_ADULT = new byte[] { 0x3A, 0x20, 0xA6 };

        /// Aborts a BP test if it is in progress
        private static byte[] ABORT_BP = new byte[] { 0x3A, 0x79, 0x01, 0x00, 0x4C };

        /// Retrieves BP reasult data Sys,dys and Pulse.
        private static byte[] GET_BP_DATA = new byte[] { 0x3A, 0x79, 0x03, 0x00, 0x4A };

        /// Retrieves the current BP cuff pressure - maximum value should be 300 mmHg
        private static byte[] GET_CUFF_PRESSURE = new byte[] { 0x3A, 0x79, 0x05, 0x00, 0x48 };

        public static byte[] CMD_CheckBPModule = { 0xFA, 0x0A, 0x02, 0x02, 0x03, 0x06, 0x00, 0x00, 0x00, 0x17 };

        //check status of bp
        private static byte[] Get_Status_BP = new byte[] { 0x3A, 0x79, 0x10, 0x03, 0x4C };


        private static byte[] Check_BP_Status = new byte[] { 0x3A, 0x79, 0x10, 0x03, 0x3D };


        static System.Timers.Timer Get_Cuff_Pressure, Get_patient_BP_Data;
        static bool BP_Started = false;
        static String incomingdata2 = "";
        static int number_of_char_incoming = 1, packet_length_chk = 1;
        static Byte[] crc_check_data;
        static bool BP_check_status = false, BP_check_module_status = false;
        public static void ReadBloodPressure(String BP_port)
        {
            Light_Connect = new JanuityUI.Modules.LightLed();
            
            try
            {
                _serialPort_BP = new SerialPort();
                _serialPort_BP.PortName = BP_port;

                _serialPort_BP.Parity = Parity.None;
                _serialPort_BP.StopBits = StopBits.One;
                _serialPort_BP.DataBits = 8;

                _serialPort_BP.BaudRate = 9600;
                _serialPort_BP.ReadTimeout = 4500;
                _serialPort_BP.WriteTimeout = 4500;

                _serialPort_BP.Open();
                _serialPort_BP.DiscardInBuffer();
                _serialPort_BP.DiscardOutBuffer();
               // _serialPort_BP.ErrorReceived += new SerialErrorReceivedEventHandler(BPDeviceErrorReceived);
            }
            catch (Exception BpPortEx) {
                kioskLog.SrushtyLog_Bloodpressure("BP Port Open: Failed ,"+ BpPortEx);
                BP_check_module_status = false;
            }

            Get_Cuff_Pressure = new System.Timers.Timer();
            Get_Cuff_Pressure.Interval = 500;
            Get_Cuff_Pressure.Elapsed += getBloodPressureLiveData;

            Get_patient_BP_Data = new System.Timers.Timer();
            Get_patient_BP_Data.Interval = 500;
            Get_patient_BP_Data.Elapsed += getBloodPressureResultData;

            Thread_Bloodpressure = new Thread(Read_BloodPressure_Value);
            Thread_Bloodpressure.Start();
        }
        /*private void BPDeviceErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            if (e.EventType == SerialError.Frame)
            {
                if (_measurementRunning)
                {
                    // Can occur when all of these are true:
                    // Device is connected directly with serial cable (not usb)
                    // User has pressed E-stop button
                    // The E-stop button is a physical power cut-out, not air pressure cut-out
                    // But also...sometimes there is no reaction when the e-stop is pressed, especially when pressed quickly
                    _measurementRunning = false;

                    string message = "BP reading aborted by hardware button";
                   
                }
            }
            else
            {
                // KP-521: Log this only once per boot to prevent spam.
                if (!_portHasFailed)
                {
                    // Note to future self: got an Overrun error here when BP was mistakenly configured to talk 
                    // to the COM port that pulse ox was on. 


                }
            }
        }*/
        static int bp_data_not_came_count = 0;
        public static void getBloodPressureLiveData(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (bp_data_not_came_count > 10)
            {
                kioskLog.SrushtyLog_Bloodpressure("BP data not came");
                Get_Cuff_Pressure.Stop();
                SendCommand_BP(ABORT_BP, "ABORT_BP");
                bp_data_not_came_count = 0;
                Thread.Sleep(100);
                SendCommand_BP(SET_INITIAL_INFLATE_ADULT, "SET_INITIAL_INFLATE_ADULT");
                Thread.Sleep(200);
                SendCommand_BP(START_BP_ADULT, "START_BP_ADULT");
                Get_Cuff_Pressure.Start();
            }
            else
            {
                SendCommand_BP(GET_CUFF_PRESSURE, "GET_CUFF_PRESSURE");
            }

        }
        static int invalidResultData = 0;
        public static void getBloodPressureResultData(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (invalidResultData < 6)
            {
                SendCommand_BP(GET_BP_DATA, "GET_BP_DATA");
                invalidResultData++;

                kioskLog.SrushtyLog_Bloodpressure("Bloodpressure GET_BP_DATA invalidResultData "+ invalidResultData);
            }
            else
            {
                string message = "Something went Wrong! Please redo the test again.";
                csession.Send("x " + message);
                SendCommand_BP(ABORT_BP, "ABORT_BP");
                BP_Started = false;
                Get_patient_BP_Data.Stop();
                invalidResultData = 0;

            }
        }

        public void StartBP(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Bloodpressure("Start Request From UI ");
            try
            {
                csession = gsession;
                invalidResultData = 0;
                inBoundpacketsCount = 0;
                SendCommand_BP(START_BP_ADULT, "START_BP_ADULT");
                Get_Cuff_Pressure.Start();
                incomingdata2 = "";
                packet_length_chk = 1;
                number_of_char_incoming = 1;
                Array.Clear(crc_check_data, 0, crc_check_data.Length);
            }
            catch (Exception) { }
        }
        public void StopBP(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Bloodpressure("Stop Request From UI ");
            try { 
            csession = gsession;
            inBoundpacketsCount = 0;
            Get_Cuff_Pressure.Stop();
            Get_patient_BP_Data.Stop();
            SendCommand_BP(ABORT_BP, "ABORT_BP");
            Thread.Sleep(100);
            Get_patient_BP_Data.Start();
            }
            catch (Exception ecs) { }
        }

        public static void SendCommand_BP(byte[] command, String cmdName)
        {
            if (!cmdName.Contains("GET_CUFF_PRESSURE"))
            {
                kioskLog.SrushtyLog_Bloodpressure("Bloodpressure Command sent " + cmdName);
            }

            try
            {
                if (_serialPort_BP != null && _serialPort_BP.IsOpen)
                {
                    _serialPort_BP.DiscardInBuffer();
                    _serialPort_BP.DiscardOutBuffer();
                    _serialPort_BP.Write(command, 0, command.Length);
                }
                else
                {
                    if (_serialPort_BP != null)
                    {
                        _serialPort_BP.Open();
                        _serialPort_BP.DiscardInBuffer();
                        _serialPort_BP.DiscardOutBuffer();
                        Thread_Bloodpressure = new Thread(Read_BloodPressure_Value);
                        Thread_Bloodpressure.Start();
                    }
                }
            }
            catch (TimeoutException ex)
            {
                //csession.Send("Error Bloodpressure_Device_Error TimeoutException " + ex);
            }
            catch (Exception ex)
            {
                if (csession != null)
                {
                    // csession.Send("Error Bloodpressure_Device_Error OtherException " + ex);
                    csession.Send("BP_Device_Not_Connected");
                }

            }
        }
        static int inBoundpacketsCount = 0;
        public static void Read_BloodPressure_Value()
        {
            crc_check_data = new Byte[90];
            BP_check_module_status = true;
            while (true)
            { 
                try
                {
                    Byte hex_incoming_data2 = (Byte)_serialPort_BP.ReadByte();
                    if (hex_incoming_data2 == 0xFF)
                    {
                        kioskLog.SrushtyLog_Bloodpressure("Stop Command *******************");
                    }

                    if (hex_incoming_data2 == 0x3E)
                    {

                        if (number_of_char_incoming == packet_length_chk)
                        {
                            inBoundpacketsCount++;
                            crc_check_data[0] = 0x3E;
                            bool CRC_BP_Packet_Check = IsValidChecksum(crc_check_data);

                            Array.Clear(crc_check_data, 0, crc_check_data.Length);
                            kioskLog.SrushtyLog_Bloodpressure("Array Cleared here");

                            kioskLog.SrushtyLog_Bloodpressure("Length P number_of_char_incoming: " + number_of_char_incoming + " packet_length_chk: " + packet_length_chk);

                            if (CRC_BP_Packet_Check)
                            {
                                String[] data = incomingdata2.Split(',');

                                if (data[1] == "5")
                                {
                                    csession.Send("BPLive " + data[2]);
                                    kioskLog.SrushtyLog_Bloodpressure("BPLive" + data[2]);

                                    int bpworking = int.Parse(data[2]);

                                    if (bpworking == 0)
                                    {
                                        bp_data_not_came_count++;
                                    }
                                    else
                                    {
                                        BP_Started = true;
                                        bp_data_not_came_count = 0;
                                    }
                                    if (bpworking < 15 && BP_Started && inBoundpacketsCount > 10)
                                    {
                                        BP_Started = false;
                                        SendCommand_BP(ABORT_BP, "ABORT_BP");
                                        Get_patient_BP_Data.Stop();
                                        Get_Cuff_Pressure.Stop();
                                        inBoundpacketsCount = 0;
                                        Light_Connect.Stop_LED();
                                        string message = "Something went Wrong! Please redo the test again.";
                                        csession.Send("x " + message);
                                        kioskLog.SrushtyLog_Bloodpressure(message);
                                    }

                                }
                                else if (data[1] == "4" && BP_Started)
                                {
                                    if (data[2] == "79")
                                    {
                                        String msg = " This is just an acknowledgement to a command";
                                        kioskLog.SrushtyLog_Bloodpressure(msg);
                                    }
                                    else if (data[2] == "75")
                                    {
                                        // Module has completed a BP measurement
                                        String msg = " Module has completed a BP measurement";
                                        kioskLog.SrushtyLog_Bloodpressure(msg);
                                        Get_Cuff_Pressure.Stop();
                                        //SendCommand_BP(ABORT_BP, "ABORT_BP");
                                        invalidResultData = 0;
                                        Get_patient_BP_Data.Start();
                                    }
                                    else if (data[2] == "65")
                                    {
                                        // Module has received abort command
                                        kioskLog.SrushtyLog_Bloodpressure("BP abort acknowledged");
                                    }
                                    else if (data[2] == "69")
                                    {
                                        //csession.Send("BP_Device_Connected");
                                        kioskLog.SrushtyLog_Bloodpressure("Error packet received from blood pressure device");
                                    }
                                    else if (data[2] == "83")
                                    {
                                        // Module has entered sleep mode
                                        kioskLog.SrushtyLog_Bloodpressure("BP has entered sleep mode");
                                    }
                                    else
                                    {
                                        //csession.Send("BP_Device_Not_Connected");
                                    }
                                }
                                else if (data[1] == "4" && BP_check_status)
                                {
                                    kioskLog.SrushtyLog_Bloodpressure("BP Status: BP_Device_Connected");
                                    csession.Send("BP_Device_Connected");

                                    BP_check_status = false;
                                }
                                else if (data[1] == "24")
                                {
                                    if (data[20] == "0")
                                    {
                                        if (data[2] != "0" && data[4] != "0" && data[16] != "0")
                                        {
                                            csession.Send("BP " + data[2] + "," + data[4] + "," + data[16]);
                                            kioskLog.SrushtyLog_Bloodpressure(incomingdata2 + " Sys: " + data[2] + " Dys: " + data[4] + " Pulse: " + data[16] + " CRC_Check: " + CRC_BP_Packet_Check);
                                        }
                                        else
                                        {
                                            kioskLog.SrushtyLog_Bloodpressure("Random Error value comes in Zero");
                                            kioskLog.SrushtyLog_Bloodpressure(incomingdata2 + " Sys: " + data[2] + " Dys: " + data[4] + " Pulse: " + data[16] + " CRC_Check: " + CRC_BP_Packet_Check);
                                            string message = "Something went Wrong! Please redo the test again.";
                                            csession.Send("x " + message);
                                        }
                                        Light_Connect.Stop_LED();
                                    }
                                    else
                                    {
                                        Light_Connect.Stop_LED();
                                        //Setlivevalues(incomingdata2 + " ErrorCode " + data[20] + " " + BP_Started + " CRC_Check: " + crc_BP);
                                        int errorCode = int.Parse(data[20]);
                                        if (errorCode == 86)
                                        {
                                            Light_Connect.Stop_LED();
                                            // It is normal to get this code when someone uses abort.
                                            string message = "BP reading aborted by user";
                                            csession.Send("BloodPressureStop " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 3)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code Out of Range BP Value, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 87)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code Inflate Timeout, Air Leak or Loose Cuff, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 88)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code Safety Timeout, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 89)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code Cuff Overpressure, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 90)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code Power supply out of range or other hardware problem, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 91)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code Permission problem such as safety link fitted or autozero out of range, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 97)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code Transducer out of range, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else if (errorCode == 99)
                                        {
                                            // See Operations Manual Rev K p. 55 for code descriptions.                                     
                                            string message = "BP error code EEPROM calibration data failure, ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                        else
                                        {
                                            // Possibly a user-recoverable error, i.e. arm not in cuff or moving too much
                                            string message = "Something went Wrong! Please redo the test again. ErrorCode: " + errorCode;
                                            csession.Send("x " + message);
                                            kioskLog.SrushtyLog_Bloodpressure(message);
                                        }
                                    }

                                    SendCommand_BP(ABORT_BP, "ABORT_BP");
                                    BP_Started = false;
                                    Get_patient_BP_Data.Stop();
                                }
                                else
                                {
                                    kioskLog.SrushtyLog_Bloodpressure("value at data1: " + data[1]);
                                }
                            }
                            else
                            {
                                kioskLog.SrushtyLog_Bloodpressure("BP CRC Failed " + incomingdata2);
                            }

                            incomingdata2 = "62,";
                            number_of_char_incoming = 1;

                        }
                        else
                        {
                            incomingdata2 = "62,";
                            crc_check_data[number_of_char_incoming] = hex_incoming_data2;
                            kioskLog.SrushtyLog_Bloodpressure("Length F number_of_char_incoming: " + number_of_char_incoming + " packet_length_chk: " + packet_length_chk);
                            number_of_char_incoming = 1;
                        }
                    }
                    else
                    {
                        crc_check_data[number_of_char_incoming] = hex_incoming_data2;
                        
                        String income = hex_incoming_data2.ToString();

                        
                        incomingdata2 += income + ",";
                        if (number_of_char_incoming == 1)
                        {
                            packet_length_chk = int.Parse(income);
                            kioskLog.SrushtyLog_Bloodpressure("Packet Length is: " + packet_length_chk);
                        }
                        number_of_char_incoming++;
                    }
                }
                catch (TimeoutException ex)
                {
                    //csession.Send("Error Bloodpressure_Device_Error TimeoutException " + ex);
                    //kioskLog.SrushtyLog_Bloodpressure("BP Read Exception: " + ex);

                }
                catch (System.IO.IOException ex)
                {
                    // csession.Send("Error Bloodpressure_Device_Error IOException " + ex);
                    //  kioskLog.SrushtyLog_Bloodpressure("BP Read Exception: " + ex);

                }
                catch (NullReferenceException ex)
                {
                    // csession.Send("Error Bloodpressure_Device_Error IOException " + ex);
                    //  kioskLog.SrushtyLog_Bloodpressure("BP Read Exception: " + ex);

                }
                catch (System.IndexOutOfRangeException ex)
                {
                    crc_check_data = new Byte[90];
                    // csession.Send("Error Bloodpressure_Device_Error IOException " + ex);
                    //  kioskLog.SrushtyLog_Bloodpressure("BP Read Exception: " + ex);

                }
                catch (Exception BP_Exception)
                {
                    if (csession != null)
                    {
                       // csession.Send("Error Bloodpressure_Device_Error OtherException " + BP_Exception);
                    }
                    BP_check_module_status = false;
                   // kioskLog.SrushtyLog_Bloodpressure("BP Read Exception: " + BP_Exception);
                }
            
            }
        }

        private static bool IsValidChecksum(byte[] dataBytes)
        {
            // Vendor Documentation: "The modulo 256 summation of all bytes in the packet (including the Checksum) 
            int summation = 0;
            String crcdata = "";
            foreach (byte d in dataBytes)
            {
                summation += d;
                crcdata += d + ",";
            }
            if (summation % 256 == 0)
            {
                return true;
            }
            else
            {
                if (summation > 65)
                {
                    kioskLog.SrushtyLog_Bloodpressure("CRC Failed: " + crcdata + " tot " + summation);
                }
                return false;
            }
        }

        public static void Close_BP_Port()
        {
            try
            {
                if (_serialPort_BP != null && _serialPort_BP.IsOpen)
                {                   
                        _serialPort_BP.Close();                        
                }
            }
            catch (Exception ex)
            {
                kioskLog.SrushtyLog_Bloodpressure("Bloodpressure Port Close Exception"+ex);
            }
        }

        public void check_bp_connect(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Bloodpressure("Bloodpressure Status Request from UI ");
            csession = gsession;
           
            if (BP_check_module_status)
            {
                BP_check_status = true;
                packet_length_chk = 4;
                SendCommand_BP(Check_BP_Status, "");
                SendCommand_BP(Check_BP_Status, "");
            }
            else
            {
                csession.Send("BP_Device_Not_Connected");
                kioskLog.SrushtyLog_Bloodpressure("BP Status: BP_Device_Not_Connected");
            }

        }

    }
}
