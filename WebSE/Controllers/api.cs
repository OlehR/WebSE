using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using WebSE.Filters;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using Utils;
using System.Net.Http;
using System.Collections.Generic;
using BRB5.Model;
using ModelMID;
using ModelMID.DB;
using System.Reflection;

namespace WebSE.Controllers
{

    [ApiController]
    [Route("[controller]")]    
    public class api : ControllerBase
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        private readonly ILogger<api> _logger;
        static BL Bl;
        static Raitting cRaitting;
        static bool Flag = false;
        public api(ILogger<api> logger)
        {
            _logger = logger;
            if (!Flag)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version}", eTypeLog.Expanded);
                Bl = BL.GetBL;
                cRaitting = new Raitting();
                //Bl.GetConfig();
                //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name,"");
                Flag = true;
            }
        }
        
        #region ChatBot
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
        public StatusD<string> SetActiveCard([FromBody] InputCard pCard)
        {
            if (pCard == null)
                return new StatusD<string>(-1, "Невірні вхідні дані");
            return Bl.SetActiveCard(pCard);
        }
        #endregion

        #region Card
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("FindByPhoneNumber/")]
        public StatusD<string> FindByPhoneNumber([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.ShortPhone))
                return new StatusD<string>(-1, "Невірні вхідні дані");

            return Bl.FindByPhoneNumber(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("CreateCustomerCard/")]
        public StatusD<string> CreateCustomerCard([FromBody] Contact pContact)
        {
            if (pContact == null)
                return new StatusD<string>(-1, "Невірні вхідні дані");

            return Bl.CreateCustomerCard(pContact);
        }

        [HttpPost]
        [Route("FindByClientByBarCode/")]
        public StatusD<Client> FindByClientByBarCode([FromBody] string pBarCode)
        {
            //Bl.GetBonusAsync(new );
            return null;
        }

        #endregion


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
        public StatusD<string> SMS([FromBody] VerifySMS pV)
        {
            try
            {
                //VerifySMS pV = new();
                var r = http.RequestAsync($"http://loyalty.zms.in.ua/api/sms?phone={pV.Phone}&campaign={pV.Company}", HttpMethod.Get, null, 3000, "application/json;charset=UTF-8", http.GetAuthorization());

                var Ans = JsonConvert.DeserializeObject<answer>(r);

                if (Ans != null && Ans.status.ToLower().Equals("success"))
                    return new StatusD<string>() { Data = Ans.verify };
                return new StatusD<string>(-1, "SMS не відправлено");
            }
            catch (Exception e) { return new StatusD<string>(-1, e.Message); }
        }

        [HttpPost]
        [Route("/Receipt")]
        public Utils.Status Receipt([FromBody] Receipt pR)
        {
            return Bl.SaveReceipt(pR);
        }

        [HttpPost]
        [Route("/CheckExciseStamp")]
        public StatusD<ExciseStamp> CheckExciseStamp(ExciseStamp pES) { return Bl.CheckExciseStamp(pES); }

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
                if (!string.IsNullOrEmpty(l.BarCodeUser))
                {
                    l = Bl.GetLoginByBarCode(l.BarCodeUser);
                }
                GetSetHttpContext(l);
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
            return Bl.Print(pWares);
        }

        #region DCT

        
        [HttpPost]
        [Route("/DCT/GetPrice")]
        public Result<WaresPrice> GetPrice([FromBody]  ApiPrice pAP)
        {
            return Bl.GetPrice(pAP);
        }
       
        [HttpPost]
        [Route("/DCT/Raitting/GetIdRaitingTemplate")]
        public Result<int> GetIdRaitingTemplate()
        {
            try
            {
                return new Result<int>() { Info = cRaitting.GetIdRaitingTemplate() };
            }
            catch (Exception e)
            {
                return new Result<int>(e);
            }
        }
         
        
        [HttpPost]
        [Route("/DCT/Raitting/GetNumberDocRaiting")]
        public Result GetNumberDocRaiting()
        {
            try
            {
                return new Result() { Info = cRaitting.GetNumberDocRaiting().ToString() };
            }
            catch (Exception e)
            {
                return new Result(e);
            }
        }
        
        [HttpPost]
        [Route("/DCT/Raitting/SaveTemplate")]
        public Result SaveTemplate([FromBody] RaitingTemplate pRT)
        {
            return cRaitting.SaveTemplate(pRT);
        }

        
        [HttpPost]
        [Route("/DCT/Raitting/SaveDocRaiting")]
        public Result SaveDocRaiting([FromBody] Doc pDoc)
        {
            return cRaitting.SaveDocRaiting(pDoc);
        }

        [HttpPost]
        [Route("/DCT/Raitting/GetRaitingTemplate")]
        public IEnumerable<RaitingTemplate> GetRaitingTemplate()
        {
            return cRaitting.GetRaitingTemplate();
        }
        
        [HttpPost]
        [Route("/DCT/Raitting/GetRaitingDocs")]
        public IEnumerable<Doc> GetRaitingDocs()
        {
            return cRaitting.GetRaitingDocs();
        }

        [HttpPost]
        [Route("/DCT/CheckPromotion/Doc")]
        public Result<IEnumerable<Doc>> GetPromotion([FromBody] int pCodeWarehouse)
        {
            return Bl.GetPromotion(pCodeWarehouse);
        }

        [HttpPost]
        [Route("/DCT/CheckPromotion/DocWares")]
        public Result<IEnumerable<DocWares>> GetPromotionData([FromBody] string pNumberDoc)
        {
            return Bl.GetPromotionData(pNumberDoc);
        }

        #endregion

        void GetSetHttpContext(login l)
        {
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
        }
        /*
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
            }*/
    }
    public class answer
    {
        public string status { get; set; }
        public string verify { get; set; }
    }
    
}
