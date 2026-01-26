using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UtilNetwork;
using Utils;

namespace WebSE
{
    public class http
    {
        static public AuthenticationHeaderValue GetAuthorization()
        {
            var jwt = "fd282e8f55c5553bc8bbee344cc0fa55cebdcbc323d261bd9c6ba15f1f51014a";
            return  new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        }
        public static string Url = "https://loyalty.sparshop.com.ua/api/";

        public ContactAnsver SendPostSiteCreate(Contact pContact)
        {
            ContactAnsver res = null;
            try
            {
                var httpClient = new HttpClient();
                
                httpClient.DefaultRequestHeaders.Authorization = GetAuthorization();

                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("first_name", pContact.first_name));
                nvc.Add(new KeyValuePair<string, string>("last_name", pContact.last_name));
                nvc.Add(new KeyValuePair<string, string>("phone", pContact.ShortPhone));
                nvc.Add(new KeyValuePair<string, string>("city_id", pContact.city_id));
                nvc.Add(new KeyValuePair<string, string>("email", pContact.email));
                nvc.Add(new KeyValuePair<string, string>("birthday", pContact.birthday));
                nvc.Add(new KeyValuePair<string, string>("gender", pContact.gender));
                nvc.Add(new KeyValuePair<string, string>("status", pContact.status));
                nvc.Add(new KeyValuePair<string, string>("family_members", pContact.family_members));
                nvc.Add(new KeyValuePair<string, string>("card", pContact.card));
                nvc.Add(new KeyValuePair<string, string>("cards_type_id", pContact.cards_type_id.ToString()));

                var req = new HttpRequestMessage(HttpMethod.Post, Url+"contact/create") { Content = new FormUrlEncodedContent(nvc) };
                var response = httpClient.SendAsync(req).Result;

                if (response.IsSuccessStatusCode)
                {
                    var r = response.Content.ReadAsStringAsync().Result;
                    res = JsonConvert.DeserializeObject<ContactAnsver>(r);
                    //res = JsonSerializer.Deserialize<ContactAnsver>(r);
                }
            }
            catch (Exception e)
            {
                res = new ContactAnsver() { status = e.Message };
            }

            return res;
        }
   
        static public string RequestAsync(string parUrl, HttpMethod pMethod , string pBody=null, int pWait = 5000, string pContex = "application/json;charset=UTF-8", AuthenticationHeaderValue pAuthentication = null)
        {
            string res = null;
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(pWait);

            if (pAuthentication != null)
                client.DefaultRequestHeaders.Authorization = pAuthentication;
            HttpRequestMessage requestMessage = new HttpRequestMessage(pMethod, parUrl);
            if(!string.IsNullOrEmpty(pBody))
                requestMessage.Content = new StringContent(pBody, Encoding.UTF8, pContex);

            var response =  client.SendAsync(requestMessage).Result;

            if (response.IsSuccessStatusCode)
            {
                res =  response.Content.ReadAsStringAsync().Result;                
            }
            else
            {
                return null;
            }
            return res;
        }

        static public async Task<UtilNetwork.Result<string>> RequestFrendsAsync(string parUrl, HttpMethod pMethod, List<KeyValuePair<string,string>> pBody , int pWait = 5000, string pContex = "application/json;charset=UTF-8")
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(pWait);
               
                HttpRequestMessage requestMessage = new HttpRequestMessage(pMethod, parUrl);

                pBody.Add(new KeyValuePair<string, string>("api_token", "9bd9267147033be2d9f1358c517b2e15559ccbb7"));
                requestMessage.Content = new FormUrlEncodedContent(pBody);

                var response = await client.SendAsync(requestMessage);
                UtilNetwork.Result<string> res = new UtilNetwork.Result<string>(response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    res.Data = await response.Content.ReadAsStringAsync();
                }
                return res;
            }
            catch (Exception ex) { return new UtilNetwork.Result<string>(ex); }
        }

        static public async Task<UtilNetwork.Result<string>> RequestBukovelAsync(string pUrl, HttpMethod pMethod, string pBody, int pWait = 5000, string pContex = "application/json")
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(pWait);
                client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Token", "53b4c8c1705cb2b79a1a2445129897eb");
                HttpRequestMessage requestMessage = new HttpRequestMessage(pMethod, pUrl);
                
                if (!string.IsNullOrEmpty(pBody))
                    requestMessage.Content = new StringContent(pBody, Encoding.UTF8, pContex);

                var response = client.SendAsync(requestMessage).Result;

                UtilNetwork.Result<string> res = new UtilNetwork.Result<string>(response.StatusCode);
                if (response.IsSuccessStatusCode)
                {
                    res.Data = await response.Content.ReadAsStringAsync();
                }
                return res;
            }
            catch (Exception ex) { return new UtilNetwork.Result<string>(ex); }
        }
    }
}
