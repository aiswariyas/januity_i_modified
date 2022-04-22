using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using SuperWebSocket;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JanuityUI.Modules
{
    class WeightScale
    {
        public static WebSocketSession csession;
        static decimal Weight_Result = 0.0m;
        static string[] WeightConfigValues;
        static SerialPort _serialPort_Weight;
        static String weightCalibrationException = "";
        static Thread Thread_Weight;
        static string WeightConfiguration = @"C:\Srushty Global Solutions\config.txt";
        public static bool device_status;




        public static void ReadWeightScale(String port)
        {
            try { 
            _serialPort_Weight = new SerialPort();
            _serialPort_Weight.PortName = port;

            _serialPort_Weight.Parity = Parity.None;
            _serialPort_Weight.StopBits = StopBits.One;
            _serialPort_Weight.DataBits = 8;

            _serialPort_Weight.BaudRate = 9600;
            _serialPort_Weight.ReadTimeout = 4500;
            _serialPort_Weight.WriteTimeout = 4500;

            _serialPort_Weight.Open();
            _serialPort_Weight.DiscardInBuffer();
            _serialPort_Weight.DiscardOutBuffer();
            }
            catch (Exception WeightPortExp) {
                device_status = false;
                kioskLog.SrushtyLog_Weight("Weight Port Exception: "+ WeightPortExp);
            }
            if (ReadCalibrationWeight())
            {
                kioskLog.SrushtyLog_Weight("Configured Weight values");
            }
            else
            {
                kioskLog.SrushtyLog_Weight("Exception in Cal: " + weightCalibrationException);
            }

            Thread_Weight = new Thread(Read_Weight_Value);
            Thread_Weight.Start();
        }
        static decimal weight;

        public static void Read_Weight_Value()
        {
            kioskLog.SrushtyLog_Weight("Weight Reading Started");
            while (true)
            {
                string LoadCellData = string.Empty;
                try
                {
                    _serialPort_Weight.ReadTimeout = 5000;
                    _serialPort_Weight.WriteTimeout = 5000;
                    if (!_serialPort_Weight.IsOpen)
                    {
                        _serialPort_Weight.Open();
                    }
                    _serialPort_Weight.DiscardOutBuffer();
                    _serialPort_Weight.DiscardInBuffer();
                    LoadCellData = _serialPort_Weight.ReadLine();
                    //kioskLog.SrushtyLog_Weight("Weight: " + LoadCellData);
                    device_status = true;


                    int WeightExtractedData = Convert.ToInt32(Regex.Replace(LoadCellData, "[^0-9]", ""));
                    string WeightVoltValue = WeightExtractedData.ToString();
                    //WeightVoltValue = WeightVoltValue.Substring(1);


                    int finalExtractedValue = Int32.Parse(WeightVoltValue.Substring(1));
                    // kioskLog.SrushtyLog_Weight("Weight: " + finalExtractedValue);


                    WeightCalibration.StaticUI(finalExtractedValue + "");
                    int offsetint = Int32.Parse(WeightConfigValues[1]);//Span Value
                    int spanint = Int32.Parse(WeightConfigValues[2]);// Offset Value
                    int Loadint = (int)Math.Round(Convert.ToDouble(WeightConfigValues[3]));//Load Value

                    decimal intialdifference = spanint - offsetint;
                    decimal Presentdifference = finalExtractedValue - offsetint;
                    decimal CutOffvalue = Presentdifference / intialdifference;
                    weight = CutOffvalue * Loadint;

                    // Weight_Result = Decimal.Round(weight, 0, MidpointRounding.AwayFromZero);
                    Weight_Result = Math.Round(weight, 2);

                    if (Weight_Result < 1)
                    {
                        Weight_Result = 0;
                    }

                    device_status = true;
                }


                catch (TimeoutException WeightException)
                {
                    kioskLog.SrushtyLog_Weight("TimeoutException WeightException: " + WeightException + " ");
                }
                catch (Exception WeightException)
                {
                    if (csession != null)
                    {
                        csession.Send("Error Weight_Device_Error OtherException " + WeightException);
                    }
                    else
                    {
                        System.Console.Write("Websocket Not Connected");
                        kioskLog.SrushtyLog_Weight("Websocket Not Connected");
                    }
                    kioskLog.SrushtyLog_Weight("WeightException: " + WeightException + " ");
                }
            }
        }




        public void getweight(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Weight("Get Weight Request from UI");
            if (Weight_Result < 1)
            {
                Weight_Result = 0;
                kioskLog.SrushtyLog_Weight("Zero Weight : " + Weight_Result);
                gsession.Send("Weight 0");
            }
            else if (Weight_Result > 150)
            {
                Weight_Result = Math.Round(Weight_Result, 2);

                decimal lbsformula = 2.20461M;
                decimal pounddata = Decimal.Multiply(Weight_Result, lbsformula);
                pounddata = Math.Round(pounddata, 2);

                gsession.Send("WeightPlus Overweight " + pounddata);
                kioskLog.SrushtyLog_Weight("Over Weight : " + pounddata);
            }
            else if (Weight_Result < 5)
            {
                Weight_Result = Math.Round(Weight_Result, 2);

                decimal lbsformula = 2.20461M;
                decimal pounddata = Decimal.Multiply(Weight_Result, lbsformula);
                pounddata = Math.Round(pounddata, 2);

                gsession.Send("WeightPlus Lowweight " + pounddata);
                kioskLog.SrushtyLog_Weight("Low Weight : " + pounddata);
            }
            else {
                //Weight_Result = 10.00M;
                Weight_Result = Math.Round(Weight_Result, 2);

                decimal lbsformula = 2.20461M;
                decimal pounddata = Decimal.Multiply(Weight_Result, lbsformula);
                pounddata = Math.Round(pounddata, 2);
                kioskLog.SrushtyLog_Weight(pounddata + " lbs " + weight);
                gsession.Send("Weight " + pounddata);
            }

        }

        public static void Close_Weight_Port()
        {
            try
            {
                if (_serialPort_Weight != null && _serialPort_Weight.IsOpen)
                {
                    _serialPort_Weight.Close();
                }
            }
            catch (Exception ex)
            {
                kioskLog.SrushtyLog_Bloodpressure("Bloodpressure Port Close Exception" + ex);
            }
        }


        public static bool ReadCalibrationWeight()
        {
            try
            {
                using (StreamReader sr = new StreamReader(WeightConfiguration))
                {
                    WeightConfigValues = new string[4];
                    //Read the number of lines and put them in the array
                    for (int i = 1; i < 4; i++)
                    {
                        WeightConfigValues[i] = sr.ReadLine();
                        kioskLog.SrushtyLog_Weight("The Config Weight in: " + i + " : " + WeightConfigValues[i]);
                    }
                    sr.Close();
                }
                return true;
            }
            catch (FileNotFoundException Exp)
            {
                weightCalibrationException = Exp.ToString();

                if (csession != null)
                {
                    csession.Send("Error Weight_Device_Error FileNotFoundException " + Exp);
                }


                kioskLog.SrushtyLog_Weight("The file could not be read: " + Exp);
                return false;
            }
            catch (Exception Exp)
            {
                weightCalibrationException = Exp.ToString();
                if (csession != null)
                {
                    csession.Send("Error Weight_Device_Error OtherException " + Exp);
                }

                kioskLog.SrushtyLog_Weight("The file could not be read: " + Exp);
                return false;
            }

        }








        public void check_weight_connect(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_Weight("Weight Status Request from UI");
            device_status = false;
            Thread.Sleep(1000);

            if (device_status)
            {
                gsession.Send("Weight_Device_Connected");
                kioskLog.SrushtyLog_Weight("Weight Status: Weight_Device_Connected");
            }
            else
            {
                gsession.Send("Weight_Device_Not_Connected");
                kioskLog.SrushtyLog_Weight("Weight Status: Weight_Device_Not_Connected");

            }

        }
    }
}
