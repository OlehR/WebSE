using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

using WebSE;
using WebSE.Filters;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using Utils;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace WebSE.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class api : ControllerBase
    {
        public SortedList<int, string> PrinterWhite = new SortedList<int, string>();
        public SortedList<int, string> PrinterYellow = new SortedList<int, string>();

        private readonly ILogger<api> _logger;
        BL Bl = new BL();
        GenLabel GL = new GenLabel();

        public api(ILogger<api> logger)
        {
            _logger = logger;
            GetConfig();
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("/ChatBot/auth/")]
        public Status Auth([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Status(-1, "Невірні вхідні дані");
            return Bl.Auth(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("/ChatBot/register/")]
        public Status Register([FromBody] RegisterUser pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Status(-1, "Невірні вхідні дані");
            return Bl.Register(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("/ChatBot/discounts/")]
        public AllInfoBonus Discounts([FromBody] InputPhone pPh)
        {
            if (pPh == null || string.IsNullOrEmpty(pPh.phone))
                return new AllInfoBonus(-1, "Невірні вхідні дані");
            return Bl.GetBonusAsync(pPh).Result;
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("/ChatBot/actionsList/")]
        public Promotion ActionsList([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Promotion(-1, "Невірні вхідні дані");

            return Bl.GetPromotion();
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("/ChatBot/infoForRegister/")]
        public InfoForRegister GetInfoForRegister([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new InfoForRegister(-1, "Невірні вхідні дані");

            return Bl.GetInfoForRegister();
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("/ChatBot/SetActiveCard/")]
        public StatusData SetActiveCard([FromBody] InputCard pCard)
        {
            if (pCard == null)
                return new StatusData(-1, "Невірні вхідні дані");
            return Bl.SetActiveCard(pCard);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("FindByPhoneNumber/")]
        public StatusData FindByPhoneNumber([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.ShortPhone))
                return new StatusData(-1, "Невірні вхідні дані");

            return Bl.FindByPhoneNumber(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("CreateCustomerCard/")]
        public StatusData CreateCustomerCard([FromBody] Contact pContact)
        {
            if (pContact == null )
                return new StatusData(-1, "Невірні вхідні дані");

            return Bl.CreateCustomerCard(pContact);
        }


        [HttpPost]
        [Route("/OldPrint")]
        public string OldPrint([FromBody] Pr pStr)
        {
            //   if (string.IsNullOrEmpty(pStr))
            //       return null;//new Status(-1, "Невірні вхідні дані");
            string output = JsonConvert.SerializeObject(pStr);
            return http.RequestAsync("http://znp.vopak.local:8088/Print", HttpMethod.Post, output, 5000, "application/json");
        }

        [HttpPost]
        [Route("/SMS")]
        public StatusData SMS([FromBody] VerifySMS pV)
        {
            try
            {
                //VerifySMS pV = new();
                var r = http.RequestAsync($"http://loyalty.zms.in.ua/api/sms?phone={pV.Phone}&campaign={pV.Company}", HttpMethod.Get, null, 3000, "application/json;charset=UTF-8", http.GetAuthorization());

                var Ans = JsonConvert.DeserializeObject<answer>(r);

                if (Ans != null && Ans.status.ToLower().Equals("success"))
                    return new StatusData() { Data = Ans.verify };
                return new StatusData(-1, "SMS не відправлено");
            }catch (Exception e) { return new StatusData(-1, e.Message); }           
        }

        [HttpPost]
        [Route("/znp")]
        public string znp([FromBody] dynamic pStr)
        {
            try
            {

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };

                string res = System.Text.Json.JsonSerializer.Serialize(pStr, options);

                var l = System.Text.Json.JsonSerializer.Deserialize<login>(res);
                if (!string.IsNullOrEmpty(l.Login) && !string.IsNullOrEmpty(l.PassWord))
                {
                    HttpContext.Session.SetString("Login", l.Login);
                    HttpContext.Session.SetString("PassWord", l.PassWord);
                }
                else
                {
                    l.Login = HttpContext.Session.GetString("Login");
                    l.PassWord = HttpContext.Session.GetString("PassWord");

                }
                if (!string.IsNullOrEmpty(l.Login) && !string.IsNullOrEmpty(l.PassWord))
                    return Bl.ExecuteApi(pStr, l);
                else
                    return "{\"State\": -1,\"Procedure\": \"C#\\Api\",\"TextError\":\"Відсутній Логін\\Пароль\"}";


            }
            catch (Exception e)
            {                
                return $"{{\"State\": -1,\"Procedure\": \"C#\\Api\",\"TextError\":\"{e.Message}\"}}";
            }


        }

        [HttpPost]
        [Route("/Print")]
        public string Print([FromBody] WaresGL pWares)
        {

            try
            {
                if (pWares == null)
                    return "Bad input Data: Wares";
                Console.WriteLine(pWares.CodeWares);

                if (pWares.CodeWarehouse == 0)
                    return "Bad input Data:CodeWarehouse";

                string NamePrinterYelow= PrinterYellow[pWares.CodeWarehouse];
                string NamePrinter= PrinterWhite[pWares.CodeWarehouse];
                if (string.IsNullOrEmpty(NamePrinter))
                    return $"Відсутній принтер: NamePrinter_{pWares.CodeWarehouse}";

                //int  x = 343 / y;
                var ListWares = GL.GetCode(pWares.CodeWarehouse, pWares.CodeWares);//"000140296,000055083,000055053"
                if (ListWares.Count() > 0)
                    GL.Print(ListWares, NamePrinter, NamePrinterYelow, $"Label_{pWares.NameDCT}_{pWares.Login}", pWares.BrandName, pWares.CodeWarehouse != 89, pWares.CodeWarehouse == 9 || pWares.CodeWarehouse == 148 || pWares.CodeWarehouse == 188);  //PrintPreview();
                FileLogger.WriteLogMessage($"\n{DateTime.Now.ToString()} Warehouse=> {pWares.CodeWarehouse} Count=> {ListWares.Count()} Login=>{pWares.Login} SN=>{pWares.SerialNumber} NameDCT=> {pWares.NameDCT} \n Wares=>{pWares.CodeWares}");

                return $"Print=>{ListWares.Count()}";

            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"\n{DateTime.Now.ToString()}\nInputData=>{pWares.CodeWares}\n{ex.Message} \n{ex.StackTrace}");
                return "Error=>" + ex.Message;
            }
        }
        void GetConfig()
        {
            var Printers = new List<Printers>();
            Startup.Configuration.GetSection("PrintServer:PrinterWhite").Bind(Printers);
            foreach (var el in Printers)            
                PrinterWhite.Add(el.Warehouse, el.Printer);
            
            Printers.Clear();
            Startup.Configuration.GetSection("PrintServer:PrinterYellow").Bind(Printers);
            foreach (var el in Printers)            
                PrinterYellow.Add(el.Warehouse, el.Printer);            
        }

        string GetWhitePrinter(int pCodeWarehouse)
        {
            if (PrinterWhite.ContainsKey(pCodeWarehouse))
                return PrinterWhite[pCodeWarehouse];
            return null;
        }

        string GetYellowPrinter(int pCodeWarehouse)
        {
            if (PrinterYellow.ContainsKey(pCodeWarehouse))
                return PrinterYellow[pCodeWarehouse];
            return null;
        }
    }

    public class Printers
    {
        public int Warehouse { get; set; }
        public string Printer { get; set; }
    }

    public class answer
    {
        public string status { get; set; }
        public string verify { get; set; }
    }
    
}
