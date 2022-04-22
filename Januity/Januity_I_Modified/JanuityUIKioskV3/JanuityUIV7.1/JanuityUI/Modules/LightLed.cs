using SuperWebSocket;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JanuityUI.Modules
{
    public class LightLed
    {
        static SerialPort _serialPortlight;
        public static byte[] Get_lighthandshake = { 0x69 };
        public static byte[] lightonBP = { 0x62 };//switch LED On b
        public static byte[] lightonSpO2 = { 0x73 };//switch LED On s
        public static byte[] lightonTemp = { 0x74 };//switch LED On t
        public static byte[] lightonFingerPrint = { 0x66 };//switch LED On f
        public static byte[] lightonoPayment = { 0x70 };//switch LED On p
        public static byte[] lightoffall = { 0x6e };//switch All LED Off n
        static Thread read_light_port;
        private static bool portfoundlight;

        public static void LightPortOpen(String port)
        {
            kioskLog.SrushtyLight_Port("Light Port: "+ port);
            _serialPortlight = new SerialPort();
            read_light_port = new Thread(LoadDeviceLight);
            read_light_port.Start();
        }
        static string selectedPort = null;
        public static void LoadDeviceLight()
        {
            var portss = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var port in portss)
            {
                try
                {
                    TryPortlight(port);
                    System.Console.WriteLine(port);
                    if (portfoundlight == true)
                    {
                        selectedPort = port;
                        _serialPortlight.PortName = selectedPort;
                        _serialPortlight.BaudRate = 9600;
                        _serialPortlight.ReadTimeout = 100;
                        _serialPortlight.WriteTimeout = 100;

                        _serialPortlight.Open();
                        _serialPortlight.DiscardInBuffer();
                        _serialPortlight.DiscardOutBuffer();
                        break;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        public static bool TryPortlight(string port)
        {
            _serialPortlight.PortName = port;
            _serialPortlight.BaudRate = 9600;
            _serialPortlight.ReadTimeout = 8000;
            _serialPortlight.WriteTimeout = 8000;
            Thread.Sleep(3500);
            try
            {
                if (!_serialPortlight.IsOpen)
                {
                    _serialPortlight.Open();
                }
                _serialPortlight.DiscardInBuffer();
                _serialPortlight.DiscardOutBuffer();
                Thread.Sleep(3500);
                string response_for_id = "";
                response_for_id = SendCommand(Get_lighthandshake, _serialPortlight);
                kioskLog.SrushtyLight_Port("Light Incoming data: " + response_for_id);

                if (response_for_id.Contains("100"))
                {
                    System.Console.WriteLine("checking");
                    portfoundlight = true;
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
        private static string SendCommand(byte[] command, SerialPort _serialPortlight)
        {
            if (!_serialPortlight.IsOpen)
            {
                _serialPortlight.Open();
            }
            _serialPortlight.Write(command, 0, command.Length);
            _serialPortlight.DiscardInBuffer();
            _serialPortlight.DiscardOutBuffer();
            Thread.Sleep(500);
            int response = _serialPortlight.ReadByte();
            kioskLog.SrushtyLight_Port("Light Incoming data: " + response.ToString());
            return response.ToString();
        }


        public void BP_LED() {
            SendCommandOn(lightonBP,"BP");
        }
        public void SpO2_LED()
        {
            SendCommandOn(lightonSpO2,"SpO2");
        }
        public void Temp_LED()
        {
            SendCommandOn(lightonTemp,"Temperature");
        }
        public void FingerPrint_LED()
        {
            SendCommandOn(lightonFingerPrint,"FingerPrint");
        }
        public void oDynamo_LED()
        {
            SendCommandOn(lightonoPayment,"oDynamo");
        }
        public void Dynaowave_LED()
        {
            SendCommandOn(lightonoPayment,"Dynawave");
        }
        public void Stop_LED()
        {
            SendCommandOn(lightoffall,"Stop");
        }
        public void SendCommandOn(byte[] lighton,String commandSent)
        {
            if (_serialPortlight != null && _serialPortlight.IsOpen)
            {
                _serialPortlight.DiscardInBuffer();
                _serialPortlight.DiscardOutBuffer();
                _serialPortlight.Write(lighton, 0, lighton.Length);
                kioskLog.SrushtyLight_Port("Light on Command Sent "+ commandSent);
            }
            else
            {
                try
                {
                    _serialPortlight.Open();
                    _serialPortlight.DiscardInBuffer();
                    _serialPortlight.DiscardOutBuffer();
                }
                catch (TimeoutException tx)
                {
                    Console.WriteLine("timeOut exception " + tx);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("other exception " + ex);
                }
            }
        }
    }
}
