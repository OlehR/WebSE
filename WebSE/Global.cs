using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSE
{
    public class Global
    {
        public static string Server1C = "http://10.1.0.23/utppsu/ws/ws1.1cws";

        public static string GenBarCodeFromPhone(string pPhone)
        {
            return "Ph" + pPhone;
        }
    }
}
