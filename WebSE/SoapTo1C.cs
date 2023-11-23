using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Utils;

namespace WebSE
{
    public class Parameters
    {
        public Parameters() { }
        public Parameters(string parName, string parValue)
        {
            Name = parName;
            Value = parValue;
        }
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class SoapTo1C
    {
        public string GenBody(string parFunction, IEnumerable<Parameters> parPar)
        {
            string parameters = "";
            if (parPar != null)
                foreach (var el in parPar)
                    parameters += $"\n<{el.Name}>{el.Value}</{el.Name}>";

            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                 "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">\n" +
                    $"<soap:Body>\n<{parFunction} xmlns=\"vopak\">{parameters}</{parFunction}>\n</soap:Body>\n</soap:Envelope>";
        }

        public async System.Threading.Tasks.Task<StatusD<string>> RequestAsync(string pUrl,string pBody,int parWait=1000,string pContex= "text/xml",string pAuth=null)
        {
            try
            {
                string res = null;
                HttpClient client = new HttpClient();

                client.Timeout = TimeSpan.FromMilliseconds(parWait);
                if (pAuth != null)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(pAuth)));
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, pUrl);

                requestMessage.Content = new StringContent(pBody, Encoding.UTF8, pContex);
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();
                    res = res.Substring(res.IndexOf(@"-instance"">") + 11);
                    res = res.Substring(0, res.IndexOf("</m:return>")).Trim();
                    return new StatusD<string>() { Data = res };
                }
                else
                {
                    return new StatusD<string>((int)response.StatusCode, response.StatusCode.ToString()) { Data = res };
                    //Global.OnSyncInfoCollected?.Invoke(new SyncInformation {  Exception = null, Status = eSyncStatus.NoFatalError, StatusDescription = "RequestAsync=>" + response.RequestMessage });
                }
            }
            catch (Exception e)
            {
                return new StatusD<string>(-1, e.Message);
            }
            
        }

    }
}
