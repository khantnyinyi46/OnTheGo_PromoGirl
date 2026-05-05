using OnTheGoPromoGirl.Common;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace OnTheGoPromoGirl.ServiceAccess
{
    public class WebService
    {
        public static string tempUri = "http://tempuri.org/";
        //public static string URL = "http://waso.onthego.cloud/promogirlws/";
        public static string SOAPAction = "SOAPAction";
        public static string textXmlUtf8 = "text/xml; charset=utf-8";
        public static class XName
        {
            public const string Activate = "ActivateResult";
            public const string LoginAuth = "LoginAuthResult";
            public const string ItemSync = "ItemSyncResult";
            public const string SalesPersonSync = "SalesPersonSyncResult";
            public const string CreateSync = "CreateSyncResult";
        }
        public static class Namespace
        {
            public const string Activate = "Activate";
            public const string LoginAuth = "LoginAuth";
            public const string ItemSync = "ItemSync";
            public const string SalesPersonSync = "SalesPersonSync";
            public const string CreateSync = "CreateSync";
        }
        public static class ASMX
        {
            public const string DeviceService = "deviceservice.asmx";
            public const string LoginService = "Loginservice.asmx";
            public const string MasterService = "masterservice.asmx";
            public const string TransferService = "TransferService.asmx";

        }
        public static class SoapRequest
        {
            public static string Activate(string deviceInfo)
            {
                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <Activate xmlns=""http://tempuri.org/"">
      <deviceInfo>{deviceInfo}</deviceInfo>
    </Activate>
  </soap:Body>
</soap:Envelope>";
                return soapRequest;

            }
            public static string LoginAuth(string deviceSecurityCode, string userID, string deviceID)
            {
                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <LoginAuth xmlns=""http://tempuri.org/"">
      <deviceSecurityCode>{deviceSecurityCode}</deviceSecurityCode>
      <userID>{userID}</userID>
      <deviceID>{deviceID}</deviceID>
    </LoginAuth>
  </soap:Body>
</soap:Envelope>";
                return soapRequest;

            }
            public static string ItemSync()
            {
                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <ItemSync xmlns=""http://tempuri.org/"" />
  </soap:Body>
</soap:Envelope>";
                return soapRequest;

            }
            public static string SalesPersonSync()
            {
                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <SalesPersonSync xmlns=""http://tempuri.org/"" />
  </soap:Body>
</soap:Envelope>";
                return soapRequest;

            }
            public static string CreateSync(string transfer, string transferline)
            {
                string soapRequest = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <CreateSync xmlns=""http://tempuri.org/"">
      <transfer>{transfer}</transfer>
      <transferline>{transferline}</transferline>
    </CreateSync>
  </soap:Body>
</soap:Envelope>";
                return soapRequest;

            }

        }
        public static async Task<string> soapRequestResponseWithToken(string soapRequest, string soapAction, string url, CancellationToken cancellationToken)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    // Configure the SOAP request message
                    HttpContent httpContent = new StringContent(soapRequest);
                    HttpResponseMessage response;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add(SOAPAction, soapAction);
                    request.Method = HttpMethod.Get;
                    request.Content = httpContent;
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(textXmlUtf8);

                    // Send the SOAP request and retrieve the response
                    response = await httpClient.SendAsync(request, cancellationToken);

                    // Read and return the response content as a string
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    return responseString;

                }

            }
            catch (Exception ex)
            {
                await GeneralClass.ShowMessageAsync(GeneralClass.Message.errorsoaprequest+ ex);
                return string.Empty;

            }

        }
        public static string extractJson(string responseString, string XName)
        {
            try
            {
                XDocument doc = XDocument.Parse(responseString);

                XNamespace ns = tempUri;
                
                XElement resultElement = doc.Descendants(ns + XName).FirstOrDefault();
                if (resultElement == null)
                {

                    return string.Empty;

                }

                string json = resultElement.Value;
                return json;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());

            }

        }
        public static async Task<string> FetchSyncData(Func<string> createSoapRequest,string soapActionNamespace,string xName,string modelNameForFailMessage,string ASMX,string serverUrl,CancellationToken cancellationToken)
        {
            try
            {
                string soapRequest = createSoapRequest();
                
                string soapAction = tempUri + soapActionNamespace;
                
                string url = serverUrl + ASMX;
                
                var responseString = await soapRequestResponseWithToken(soapRequest, soapAction, url,cancellationToken);

                var json = extractJson(responseString, xName);
                return json;

            }
            catch (Exception ex)
            {
                await GeneralClass.ShowMessageAsync($"{string.Format(GeneralClass.Message.failpleasecontact, modelNameForFailMessage)} {ex}");
                return string.Empty;

            }

        }

    }

}
