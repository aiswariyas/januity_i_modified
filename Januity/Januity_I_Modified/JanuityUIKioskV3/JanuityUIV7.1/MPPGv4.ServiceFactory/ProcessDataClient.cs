using Microsoft.Extensions.Configuration;
using MPPGv4.Dtos;
using System;
using System.IO;
using System.Net;

namespace MPPGv4.ServiceFactory
{
    public class ProcessDataClient :IProcessDataClient
    {
        private IConfiguration _config;
        public Uri Host { get; private set; }
        public string CertificateFileName { get; private set; }
        public string CertificatePassword { get; private set; }
        public ProcessDataClient(IConfiguration config)
        {
            _config = config;
            Host = new Uri("https://svc1.magensa.net/MPPGv4/MPPGv4Service.svc?wsdl");
            CertificateFileName = null;
            CertificatePassword = null;
        }
        /// <summary>
        /// ProcessDataResponseDto process the input request and returns the response
        /// </summary>
        /// <param name="processDataRequestDto"></param>
        /// <returns></returns>
        public ProcessDataResponseDto ProcessData(ProcessDataRequestDto dto)
        {
            var response = new ProcessDataResponseDto();
            string pageContent = string.Empty;
            string soapAction = "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessData";

            try
            {
                string soapBody = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:mpp=""http://www.magensa.net/MPPGv4/"" xmlns:mpp1=""http://schemas.datacontract.org/2004/07/MPPGv4WS.Core"" xmlns:sys=""http://schemas.datacontract.org/2004/07/System.Collections.Generic"">
	<soapenv:Header/>
	<soapenv:Body>
		<mpp:ProcessData>         
			<mpp:ProcessDataRequests>            
				<mpp1:ProcessDataRequest>               
					<mpp1:AdditionalRequestData>                  
						<sys:KeyValuePairOfstringstring>
							<sys:key/>
							<sys:value/>
						</sys:KeyValuePairOfstringstring>
					</mpp1:AdditionalRequestData>
					<mpp1:Authentication>
						<mpp1:CustomerCode>{dto.CustomerCode}</mpp1:CustomerCode>
						<mpp1:Password>{dto.Password}</mpp1:Password>
						<mpp1:Username>{dto.Username}</mpp1:Username>
					</mpp1:Authentication>               
					<mpp1:CustomerTransactionID>{dto.CustomerTransactionID}</mpp1:CustomerTransactionID>
					<mpp1:DataInput>
						<mpp1:Data>{dto.Data}</mpp1:Data>
						<mpp1:DataFormatType>{dto.DataFormatType}</mpp1:DataFormatType>                  
						<mpp1:EncryptionInfo>                     
							<mpp1:EncryptionType>{dto.EncryptionType}</mpp1:EncryptionType>                     
							<mpp1:KSN>{dto.KSN}</mpp1:KSN>
							<mpp1:NumberOfPaddedBytes>{dto.NumberOfPaddedBytes}</mpp1:NumberOfPaddedBytes>
						</mpp1:EncryptionInfo>
						<mpp1:IsEncrypted>{dto.IsEncrypted}</mpp1:IsEncrypted>
						<mpp1:PaymentMode>{dto.PaymentMode}</mpp1:PaymentMode>
					</mpp1:DataInput>
					<mpp1:TransactionInput>
						<mpp1:Amount>{dto.Amount}</mpp1:Amount>
						<mpp1:ProcessorName>{dto.ProcessorName}</mpp1:ProcessorName>
						<mpp1:TransactionInputDetails>
							<sys:KeyValuePairOfstringstring>
								<sys:key/>
								<sys:value/>
							</sys:KeyValuePairOfstringstring>
						</mpp1:TransactionInputDetails>
						<mpp1:TransactionType>{dto.TransactionType}</mpp1:TransactionType>
					</mpp1:TransactionInput>
				</mpp1:ProcessDataRequest>
			</mpp:ProcessDataRequests>
		</mpp:ProcessData>
	</soapenv:Body>
</soapenv:Envelope>";

                MagensaSOAPClient soapClient = new MagensaSOAPClient(host: Host, certificateFileName: CertificateFileName, certificatePassword: CertificatePassword);
                HttpWebResponse webResponse = soapClient.CallWebService(soapAction, soapBody);
                Stream responseStream = webResponse.GetResponseStream();
                using (StreamReader sr = new StreamReader(responseStream))
                {
                    response.StatusCode = (int)webResponse.StatusCode;
                    response.PageContent = sr.ReadToEnd();
                }
                responseStream.Close();
                webResponse.Close();
            }
            catch (WebException ex)
            {
                HttpStatusCode sCode = ((HttpWebResponse)ex.Response).StatusCode;
                response.StatusCode = (int)sCode;
                response.PageContent = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception ex)
            {
                response.StatusCode = null;
                response.PageContent = ex.Message;
            }
            return response;
        }
    }
}
