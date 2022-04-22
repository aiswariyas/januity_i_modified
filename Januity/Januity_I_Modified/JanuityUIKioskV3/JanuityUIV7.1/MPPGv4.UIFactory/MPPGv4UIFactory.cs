using MPPGv4.Dtos;
using MPPGv4.ServiceFactory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MPPGv4.UIFactory
{
    public class Mppgv4UIfactory : IMppgv4UIFactory
    {
        IServiceProvider _serviceProvider;

        public Mppgv4UIfactory(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }


        public void ShowUI(MPPGv4UI mPPGv4UI, string data, string kSN, int amount, string customerTransactionID)
        {
            ShowProcessDataUI(data, kSN, amount, customerTransactionID); 
        }

        private void ShowProcessDataUI(string data, string kSN, int amount, string customerTransactionID)
        {
            SrushtyLog_oDynamo("data =" + data + "\n");
            SrushtyLog_oDynamo("kSN =" + kSN + "\n");
            SrushtyLog_oDynamo("amount =" + amount + "\n");
            SrushtyLog_oDynamo("customerTransactionID =" + customerTransactionID + "\n");

            var requestDto = new ProcessDataRequestDto();
            try
            {
                requestDto.CustomerCode = "IJ45467513";
                requestDto.Username = "MAG062509731";
                requestDto.Password = "ea#Q8p@Nu!5UvH";
                requestDto.CustomerTransactionID = customerTransactionID;//
                requestDto.Data = data;
                requestDto.DataFormatType = "TLV";
                requestDto.EncryptionType = "";
                requestDto.KSN = kSN;
                requestDto.NumberOfPaddedBytes = 0;
                requestDto.IsEncrypted = "true";
                requestDto.PaymentMode = "EMV";
                requestDto.Amount = amount;
                requestDto.ProcessorName = "Rapid Connect v3 - Pilot";
                requestDto.TransactionType = "AUTHORIZE";
                var svc = _serviceProvider.GetService<IProcessDataClient>();
                var responseDto = svc.ProcessData(requestDto);
                
                
                if (responseDto != null)
                {
                    var response = responseDto as ProcessDataResponseDto;
                    SrushtyLog_oDynamo("=====================Response Start======================");
                    SrushtyLog_oDynamo("Status Code:" + response.StatusCode);
                    SrushtyLog_oDynamo("Response:");
                    SrushtyLog_oDynamo(PrettyXml(response.PageContent) + "\n");
                    SrushtyLog_oDynamo("=====================Response End======================");
                }
                else
                {
                    SrushtyLog_oDynamo("Response is null, Please check with input values given and try again");
                }
            }
            catch (Exception ex)
            {
                SrushtyLog_oDynamo("Error Occurred while Processing ProcessData" + ex.Message.ToString());
            }
        }


        
        static string LogpathoDynamoPort = @"C:\Srushty Global Solutions\oDynamo.txt";
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

        public static bool IsValidXml(string xml)
        {
            try
            {
                XDocument.Parse(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string PrettyXml(string xml)
        {
            if (IsValidXml(xml)) //print xml in beautiful format
            {
                var stringBuilder = new StringBuilder();
                XElement element = XElement.Parse(xml);
                var settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;
                settings.Indent = true;
                settings.NewLineOnAttributes = true;


                XNamespace aw1 = "http://schemas.datacontract.org/2004/07/MPPGv4WS.Core";
                XNamespace aw2 = "http://schemas.datacontract.org/2004/07/System.Collections.Generic";


                string key = (string)
                    (from el in element.Descendants(aw2 + "key")
                     select el).First();
                Console.WriteLine(key + "\n");

                string value = (string)
                    (from el in element.Descendants(aw2 + "value")
                     select el).First();
                Console.WriteLine(value + "\n");

                string MagTranID = (string)
                    (from el in element.Descendants(aw1 + "MagTranID")
                     select el).First();
                Console.WriteLine(MagTranID + "\n");

                bool IsTransactionApproved = (bool)
                    (from el in element.Descendants(aw1 + "IsTransactionApproved")
                     select el).First();
                Console.WriteLine(IsTransactionApproved + "\n");

                Int32 TransactionStatus = (Int32)
                    (from el in element.Descendants(aw1 + "TransactionStatus")
                     select el).First();
                Console.WriteLine(TransactionStatus + "\n");

                string TransactionUTCTimestamp = (string)
                    (from el in element.Descendants(aw1 + "TransactionUTCTimestamp")
                     select el).First();
                Console.WriteLine(TransactionUTCTimestamp + "\n");

                string TransactionMessage = (string)
                    (from el in element.Descendants(aw1 + "TransactionMessage")
                     select el).First();
                Console.WriteLine(TransactionMessage + "\n");

                string TransactionID = (string)
                    (from el in element.Descendants(aw1 + "TransactionID")
                     select el).First();
                Console.WriteLine(TransactionID + "\n");


                string CustomerTransactionID = (string)
                    (from el in element.Descendants(aw1 + "CustomerTransactionID")
                     select el).First();
                Console.WriteLine(CustomerTransactionID + "\n");


                string value2 = (string)
                    (from el in element.Descendants(aw2 + "KeyValuePairOfstringstring")
                     select el).First();
                Console.WriteLine(value2 + "\n");


                SrushtyLog_oDynamo("CustomerTransactionID = " + CustomerTransactionID);
                SrushtyLog_oDynamo("key = " + key);
                SrushtyLog_oDynamo("value = " + value);
                SrushtyLog_oDynamo("MagTranID = " + MagTranID);
                SrushtyLog_oDynamo("TransactionStatus = " + TransactionStatus);
                SrushtyLog_oDynamo("TransactionUTCTimestamp = " + TransactionUTCTimestamp);
                SrushtyLog_oDynamo("TransactionMessage = " + TransactionMessage);
                SrushtyLog_oDynamo("TransactionID = " + TransactionID);
                SrushtyLog_oDynamo("CustomerTransactionID = " + CustomerTransactionID);
                SrushtyLog_oDynamo("value2 = " + value2);

                string FaultCode = "";
                string FaultReason = "";


                try
                {
                    FaultCode = (string)
                    (from el in element.Descendants(aw1 + "FaultCode")
                     select el).First();
                    Console.WriteLine(FaultCode + "\n");

                    FaultReason = (string)
                        (from el in element.Descendants(aw1 + "FaultReason")
                         select el).First();
                    Console.WriteLine(FaultReason + "\n");

                    SrushtyLog_oDynamo("FaultCode = " + FaultCode);
                    SrushtyLog_oDynamo("FaultReason = " + FaultReason);
                }
                catch
                {
                    SrushtyLog_oDynamo("FaultCode = " + FaultCode);
                    SrushtyLog_oDynamo("FaultReason = " + FaultReason);
                }
                
                using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
                {
                    element.Save(xmlWriter);
                }
                return stringBuilder.ToString();
            }
            else
            {
                return xml;
            }
        }
    }
}
