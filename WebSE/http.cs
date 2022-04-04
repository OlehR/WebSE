using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSE
{
    public class http
    {
        static public AuthenticationHeaderValue GetAuthorization()
        {
            var jwt = "fd282e8f55c5553bc8bbee344cc0fa55cebdcbc323d261bd9c6ba15f1f51014a";
            return  new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);
        }

        public ContactAnsver SendPostSiteCreate(Contact pContact)
        {
            ContactAnsver res = null;
            try
            {
                var httpClient = new HttpClient();
                var jwt = "fd282e8f55c5553bc8bbee344cc0fa55cebdcbc323d261bd9c6ba15f1f51014a";

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                             

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

                var req = new HttpRequestMessage(HttpMethod.Post, "http://loyalty.zms.in.ua/api/contact/create") { Content = new FormUrlEncodedContent(nvc) };
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

      /*  public ECardAnsver SendPostSiteGetCard(InputPhone pPhone)
        {
            ECardAnsver res = null;
            try
            {
                var httpClient = new HttpClient();
                var jwt = "fd282e8f55c5553bc8bbee344cc0fa55cebdcbc323d261bd9c6ba15f1f51014a";

                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwt);

                //httpClient.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");               

                //var nvc = new List<KeyValuePair<string, string>>();

                //nvc.Add(new KeyValuePair<string, string>("phone", pPhone.phone));                

                var req = new HttpRequestMessage(HttpMethod.Get, "http://loyalty.zms.in.ua/api/contact/get?phone="+pPhone.phone) ;
                var response =  httpClient.SendAsync(req).Result;

                if (response.IsSuccessStatusCode)
                {
                    var r = response.Content.ReadAsStringAsync().Result;
                    res = JsonConvert.DeserializeObject<ECardAnsver>(r);
                    //res = JsonSerializer.Deserialize<ContactAnsver>(r);
                }
            } catch(Exception e)
            {
                res = new ECardAnsver() { status = e.Message };
            }
            
            return res;
        }

       */
        
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

    }
}
