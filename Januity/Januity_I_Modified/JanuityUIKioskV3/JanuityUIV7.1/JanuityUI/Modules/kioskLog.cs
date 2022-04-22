using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JanuityUI.Modules
{
    class kioskLog
    {
        static string LogpathPort = @"C:\Srushty Global Solutions\SrushtyLog_Port.txt";
        static string LogpathBloodPressure = @"C:\Srushty Global Solutions\SrushtyLog_Bloodpressure.txt";
        static string LogpathFingerPrint = @"C:\Srushty Global Solutions\SrushtyLog_FingerPrint.txt";
        static string LogpathSpO2 = @"C:\Srushty Global Solutions\SrushtyLog_SpO2.txt";
        static string LogpathWeight = @"C:\Srushty Global Solutions\SrushtyLog_Weight.txt";
        static string LogpathTempPort = @"C:\Srushty Global Solutions\SrushtyLog_Temp.txt";
        static string LogpathoDynamoPort = @"C:\Srushty Global Solutions\oDynamo.txt";
        static string LogLightpathPort = @"C:\Srushty Global Solutions\SrushtyLight_Port.txt";

        public static void SrushtyLight_Port(String Log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            try
            {
                File.AppendAllText(LogLightpathPort, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber + "" + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
        public static void SrushtyLog_SpO2(String Log,[CallerLineNumber] int lineNumber = 0,[CallerMemberName] string caller = null)
        {
            try
            {
                File.AppendAllText(LogpathSpO2, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber +""+ Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }

        public static void SrushtyLog_Temp(String Log, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            try
            {
                File.AppendAllText(LogpathTempPort, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber + "" + Environment.NewLine);

            }
            catch (Exception)
            {
            }
        }

        public static void SrushtyLog_FingerPrint(String Log, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = null)
        {
            try
            {
                File.AppendAllText(LogpathFingerPrint, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber + "" + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
        public static void SrushtyLog_Bloodpressure(String Log, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = null)
        {
            try
            {
                File.AppendAllText(LogpathBloodPressure, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber + "" + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
        public static void SrushtyLog_Port(String Log, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = null)
        {
            try
            {
                File.AppendAllText(LogpathPort, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber + "" + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }

        public static void SrushtyLog_Weight(String Log, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = null)
        {
            try
            {
                File.AppendAllText(LogpathWeight, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber + "" + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }

        public static void SrushtyLog_oDynamo(String Log, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string caller = null)
        {
            try
            {
                File.AppendAllText(LogpathoDynamoPort, DateTime.Now + ": " + Log + " (" + caller + ") Line:" + lineNumber + "" + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }
    }
}
