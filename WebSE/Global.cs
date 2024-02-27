using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utils;

namespace WebSE
{
    public class Global
    {
        static public IEnumerable<Locality> Citys = null;
        static Dictionary<int, string> Citi = new Dictionary<int, string>();
        static public bool IsTest = false;
        static Global()
        {
            try
            {
                var PathLog = Startup.Configuration.GetValue<string>("PathLog");
                if (string.IsNullOrEmpty(PathLog))
                    PathLog = Path.Combine(Directory.GetCurrentDirectory(),"Logs");
                FileLogger.Init(PathLog, 0);
            }
            catch
            { }
            try
            {
                var r = http.RequestAsync(http.Url + "store/cities", HttpMethod.Get, null, 5000, "application/json;charset=UTF-8", http.GetAuthorization());
                if (!string.IsNullOrEmpty(r))
                {
                    var l = Newtonsoft.Json.JsonConvert.DeserializeObject<L>(r);
                    if (l != null && l.cities != null)
                        Citys = l.cities;
                    else
                        Citys = new MsSQL().GetLocality();
                    if (Citys != null)
                        foreach (var el in Citys)
                            Citi.Add(el.Id, el.title);
                }
                IsTest = Startup.Configuration.GetValue<bool>("IsTest");
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"WebSE.Global {e.Message}{Environment.NewLine}{e.StackTrace}");
            }
        }
        //private MemoryCache Cashe;

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
        public static string GetFullPhone(string pPhone)
        {
            if (string.IsNullOrEmpty(pPhone) || pPhone.Length<9|| pPhone.Length>13) return null;
            return "+380"[..(13 - pPhone.Length)]+ pPhone;
        }
    }

    class L { public IEnumerable<Locality> cities { get; set; } }

}
