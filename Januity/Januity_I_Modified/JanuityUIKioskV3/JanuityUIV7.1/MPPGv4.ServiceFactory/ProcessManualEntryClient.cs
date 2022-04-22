using Microsoft.Extensions.Configuration;
using MPPGv4.Dtos;
using System;
using System.IO;
using System.Net;

namespace MPPGv4.ServiceFactory
{
    public class ProcessManualEntryClient : IProcessManualEntryClient
    {
        private IConfiguration _config;
        public Uri Host { get; private set; }
        public string CertificateFileName { get; private set; }
        public string CertificatePassword { get; private set; }
        public ProcessManualEntryClient(IConfiguration config)
        {
            _config = config;
            Host = new Uri(_config.GetValue<string>(Constants.MPPGV4SERVICEURL));
            CertificateFileName = null;
            CertificatePassword = null;
        }
        /// <summary>
        /// ProcessManualEntryResponseDto process the input request and returns the response
        /// </summary>
        /// <param name="processManualEntryRequestDto"></param>
        /// <returns></returns>
        public ProcessManualEntryResponseDto ProcessManualEntry(ProcessManualEntryRequestDto dto)
        {
            var response = new ProcessManualEntryResponseDto();
            string pageContent = string.Empty;
            string soapAction = "http://www.magensa.net/MPPGv4/IMPPGv4Service/ProcessManualEntry";

            try
            {
                string soapBody = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:mpp=""http://www.magensa.net/MPPGv4/"" xmlns:mpp1=""http://schemas.datacontract.org/2004/07/MPPGv4WS.Core"" xmlns:sys=""http://schemas.datacontract.org/2004/07/System.Collections.Generic"">
	<soapenv:Header/>
	<soapenv:Body>
		<mpp:ProcessManualEntry>         
			<mpp:ProcessManualEntryRequests>            
				<mpp1:ProcessManualEntryRequest>               
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
					<mpp1:ManualEntryInput>						
						<mpp1:AddressLine1>{dto.AddressLine1}</mpp1:AddressLine1>						
						<mpp1:AddressLine2>{dto.AddressLine2}</mpp1:AddressLine2>						
						<mpp1:City>{dto.City}</mpp1:City>						
						<mpp1:Country>{dto.Country}</mpp1:Country>
						<mpp1:ExpirationDate>{dto.ExpirationDate}</mpp1:ExpirationDate>						
						<mpp1:NameOnCard>{dto.NameOnCard}</mpp1:NameOnCard>
						<mpp1:PAN>{dto.PAN}</mpp1:PAN>						
						<mpp1:State>{dto.State}</mpp1:State>						
					</mpp1:ManualEntryInput>
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
				</mpp1:ProcessManualEntryRequest>
			</mpp:ProcessManualEntryRequests>
		</mpp:ProcessManualEntry>
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