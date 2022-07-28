using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebSE
{
    public class Global
    {
        static public IEnumerable<Locality> Citys = null;
        static Dictionary<int, string> Citi = new Dictionary<int, string>();

        static Global()
        {
            var r = http.RequestAsync("http://loyalty.zms.in.ua/api/store/cities", HttpMethod.Get, null, 5000, "application/json;charset=UTF-8", http.GetAuthorization());
            if (!string.IsNullOrEmpty(r))
            {
                var l = Newtonsoft.Json.JsonConvert.DeserializeObject<L>(r);
                if (l != null && l.cities != null)
                    Citys = l.cities;
                else
                    Citys = new MsSQL().GetLocality();
                if(Citys!=null)
                    foreach (var el in Citys)
                        Citi.Add(el.Id, el.title);

            }
        }

        static public string GetCity(int pId)
        {
            if (Citi.ContainsKey(pId))
                return Citi[pId];
            return "Невизначене місце";
        }

        public static string Server1C = "http://bafsrv/psu_utp/ws/ws1.1cws";

        public static string GenBarCodeFromPhone(string pPhone)
        {
            return "Ph" + pPhone;
        }

    }
}
