using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using SuperWebSocket;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SourceAFIS.Simple;
using MANTRA;
using System.Drawing;

namespace JanuityUI.Modules
{
    class FingerPrint
    {
        static MFS100 mfs100;
        static string FingerprintRes = @"C:\Srushty Global Solutions\FingerPrint\";
        static AfisEngine Afis;
        public static WebSocketSession csession;
        static Person person,person1;
        static Boolean FingerPrintCheck = false;

        static JanuityUI.Modules.LightLed Light_Connect;

        public static void Fingerprint() {
            kioskLog.SrushtyLog_FingerPrint("FingerPrint Started");
            Light_Connect = new JanuityUI.Modules.LightLed();
            mfs100 = new MFS100();
            mfs100.OnCaptureCompleted += OnCaptureCompleted;
            Afis = new AfisEngine();
            DeviceInfo deviceInfo = null;

            person1 = new Person();


            int ret = mfs100.Init();
            //You can pass key here if you have not passed in initialization of class. 
            if (ret != 0)
            {
                kioskLog.SrushtyLog_FingerPrint(mfs100.GetErrorMsg(ret));
            } else
            {
                deviceInfo = mfs100.GetDeviceInfo();
                if (deviceInfo != null)
                {
                    string scannerInfo = "SERIAL NO.: " + deviceInfo.SerialNo + " MAKE: " + deviceInfo.Make + " MODEL: " + deviceInfo.Model;

                    kioskLog.SrushtyLog_FingerPrint("ScanerInfo:" + scannerInfo);
                }
                kioskLog.SrushtyLog_FingerPrint(mfs100.GetErrorMsg(ret));
            }
        }

        public void StartcaptureFingerPrint(WebSocketSession gsession) {
            kioskLog.SrushtyLog_FingerPrint("Finger Start Request from UI");
            try { 
                int ret = mfs100.StartCapture(60, 60000, true); 
                if (ret != 0)
                {
                 kioskLog.SrushtyLog_FingerPrint(mfs100.GetErrorMsg(ret));
                }
                FingerPrintCheck = false;
                csession = gsession;
                csession.Send("Fingerprint Started");
            }
            catch (NullReferenceException ExceptionFP)
            {
                if (csession != null)
                {
                    csession.Send("Error Fingerprint_Device_Error NullReferenceException " + ExceptionFP);
                }
                kioskLog.SrushtyLog_FingerPrint("Exception" + ExceptionFP);
            }
            catch (Exception ExceptionFP)
            {
                if (csession != null)
                {
                    csession.Send("Error Fingerprint_Device_Error Exception " + ExceptionFP);
                }
                kioskLog.SrushtyLog_FingerPrint("Exception" + ExceptionFP);
            }
        }

        public static void OnCaptureCompleted(bool status, int errorCode, string errorMsg, FingerData fingerprintData)
        {
            try
            {
                Light_Connect.Stop_LED();
                if (status)
                {
                    //picFinger.Image = fingerprintData.FingerImage;

                    System.IO.MemoryStream ms = new MemoryStream();
                    fingerprintData.FingerImage.Save(ms, ImageFormat.Jpeg);
                    byte[] byteImage = ms.ToArray();
                    string SigBase64 = Convert.ToBase64String(byteImage);

                    string info = "Quality: " + fingerprintData.Quality.ToString() + " Nfiq: " + fingerprintData.Nfiq.ToString() + " Bpp: " + fingerprintData.Bpp.ToString() + " GrayScale:" + fingerprintData.GrayScale.ToString() + "\nW(in):" + fingerprintData.InWidth.ToString() + " H(in):" + fingerprintData.InHeight.ToString() + " area(in):" + fingerprintData.InArea.ToString() + " Dpi/Ppi:" + fingerprintData.Resolution.ToString() + " Compress Ratio:" + fingerprintData.WSQCompressRatio.ToString();
                    String timeStamp = GetTimestamp(DateTime.Now);
                    kioskLog.SrushtyLog_FingerPrint("On Complete info:" + timeStamp + info);
                    File.WriteAllBytes(FingerprintRes + "//ISOTemplate"+ timeStamp+".iso", fingerprintData.ISOTemplate);
                    File.WriteAllBytes(FingerprintRes + "//ISOImage" + timeStamp + ".iso", fingerprintData.ISOImage);
                    File.WriteAllBytes(FingerprintRes + "//AnsiTemplate" + timeStamp + ".ansi", fingerprintData.ANSITemplate);
                    File.WriteAllBytes(FingerprintRes + "//RawData" + timeStamp + ".raw", fingerprintData.RawData);
                    fingerprintData.FingerImage.Save(FingerprintRes + "//FingerImage" + timeStamp + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    File.WriteAllBytes(FingerprintRes + "//WSQImage" + timeStamp + ".wsq", fingerprintData.WSQImage);
                    kioskLog.SrushtyLog_FingerPrint("Capture Success. Finger data is saved at application path");
/*
                    Fingerprint fp = new Fingerprint();
                    fp.AsBitmap = new Bitmap(Bitmap.FromFile(FingerprintRes + "//FingerImage.bmp"));
                    
                    person = new Person();
                    person.Fingerprints.Add(fp);
                    kioskLog.SrushtyLog_FingerPrint("FingerprintConverted2" + person.Fingerprints.ToString());
                    
                    Afis.Extract(person);
                    kioskLog.SrushtyLog_FingerPrint("FingerprintConverted3" + person.Fingerprints.ToString());
                    kioskLog.SrushtyLog_FingerPrint("On Complete FingerPrintCheck:" + FingerPrintCheck);
*/

                    csession.Send("Fingerprint Completed");
                }
                else {
                    String OncompleteFailed = "Failed: error: " + errorCode.ToString() + " (" + errorMsg + ")";
                    kioskLog.SrushtyLog_FingerPrint("On Complete: " + OncompleteFailed);
                    csession.Send("Fingerprint Exception "+ OncompleteFailed);
                }

            }
            catch (TimeoutException tx)
            {
                Console.WriteLine("timeOut exception " + tx);
            }

            catch (IOException tx)
            {
                Console.WriteLine("IOException exception " + tx);
            }

            catch (Exception ex)
            {
                kioskLog.SrushtyLog_FingerPrint(ex.ToString());
                if (csession != null)
                {
                    csession.Send("Error Fingerprint_Device_Error OtherException " + ex);
                }
                //csession.Send("Fingerprint Exception " + ex.ToString());
            }
        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }


        public void check_fingerprint_connect(WebSocketSession gsession)
        {
            kioskLog.SrushtyLog_FingerPrint("Fingerprint Status Request from UI");
            csession = gsession;
            try
            {

                if (mfs100.IsConnected())
                {
                    csession.Send("Fingerprint_Device_Connected");
                }
                else
                {
                    csession.Send("Fingerprint_Not_Device_Connected");
                }
            }
            catch (Exception e)
            {
                if (csession != null)
                {
                    csession.Send("Fingerprint_Not_Device_Connected");
                }
                kioskLog.SrushtyLog_FingerPrint("Fingerprint Error :USB Device not connected to the system: " + e);

            }

        }
    } }
