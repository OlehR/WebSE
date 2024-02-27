﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using WebSE.Filters;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Utils;
using System.Net.Http;
using System.Collections.Generic;
using BRB5.Model;
using ModelMID;
using ModelMID.DB;
using System.Reflection;
using System.Threading.Tasks;

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
        public Status<string> SetActiveCard([FromBody] InputCard pCard)
        {
            if (pCard == null)
                return new Status<string>(-1, "Невірні вхідні дані");
            return Bl.SetActiveCard(pCard);
        }
        #endregion

        #region Card
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("FindByPhoneNumber/")]
        public Status<string> FindByPhoneNumber([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.ShortPhone))
                return new Status<string>(-1, "Невірні вхідні дані");

            return Bl.FindByPhoneNumber(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("CreateCustomerCard/")]
        public Status<string> CreateCustomerCard([FromBody] Contact pContact)
        {
            if (pContact == null)
                return new Status<string>(-1, "Невірні вхідні дані");

            return Bl.CreateCustomerCard(pContact);
        }

        [HttpPost]
        [Route("FindClient/")]
        public Status<Client> FindClient([FromBody] FindClient pFC)
        {
            Client client = null;
            if (pFC == null) return null ;            
            return new Status<Client>() { Data= Bl.GetClientPhone(pFC.BarCode) };
        }

        [HttpPost]
        [Route("GetDiscount/")]
        public async Task<Status<Client>> GetDiscountAsync([FromBody] FindClient pFC)
        {
            return await Bl.GetDiscountAsync(pFC);      
        }

            #endregion


        /*[HttpPost]
        [Route("/OldPrint")]
        public string OldPrint([FromBody] Pr pStr)
        {
            //   if (string.IsNullOrEmpty(pStr))
            //       return null;//new Status(-1, "Невірні вхідні дані");
            string output = JsonConvert.SerializeObject(pStr);
            return http.RequestAsync("http://znp.vopak.local:8088/Print", HttpMethod.Post, output, 5000, "application/json");
        }*/
        
        [HttpPost]
        [Route("/SMS")]
        public Status<string> SMS([FromBody] VerifySMS pV)
        {
            try
            {
                //VerifySMS pV = new(); http://loyalty.zms.in.ua/api
                var r = http.RequestAsync( $"{http.Url}sms?phone={pV.Phone}&campaign={pV.Company}", HttpMethod.Get, null, 3000, "application/json;charset=UTF-8", http.GetAuthorization());

                var Ans = JsonConvert.DeserializeObject<answer>(r);

                if (Ans != null && Ans.status.ToLower().Equals("success"))
                    return new Status<string>() { Data = Ans.verify };
                return new Status<string>(-1, "SMS не відправлено");
            }
            catch (Exception e) { return new Status<string>(-1, e.Message); }
        }

        [HttpPost]
        [Route("/Receipt")]
        public Utils.Status Receipt([FromBody] Receipt pR)=> Bl.SaveReceipt(pR);
        

        [HttpPost]
        [Route("/CheckExciseStamp")]
        public Status<ExciseStamp> CheckExciseStamp(ExciseStamp pES) => Bl.CheckExciseStamp(pES);

        [HttpPost]
        [Route("/znp")]
        public string Znp([FromBody] dynamic pStr)
        {
            string Res;
            login l;
            (string, login) tt = Bl.Znp( pStr);            
            (Res, l) = tt;
            if (l != null)
                GetSetHttpContext(l);
            return Res;
        }

        [HttpPost]
        [Route("/Print")]
        public string Print([FromBody] WaresGL pWares) => Bl.Print(pWares);
        

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
        [Route("/DCT/SaveDocData")]
        public Result SaveDocData([FromBody] ApiSaveDoc pD)
        {
            return Bl.SaveDocData(pD);
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
        [Route("/DCT/CheckPromotion/GetPromotionData")]
        public Result<IEnumerable<DocWares>> GetPromotionData([FromBody] string pNumberDoc)
        {
            return Bl.GetPromotionData(pNumberDoc);
        }

        #endregion
        [HttpGet]
        [Route("/GetInfo")]
        public string GetInfo()
        {
            return @$"GC=>{GC.GetTotalMemory(false)/(1024*1024)}Mb
FileLogger=>{FileLogger.GetFileName}
{Bl.GenCountNeedSend()}";
        }

        [HttpPost]
        [Route("/ReloadReceiptDB")]
        public string ReloadReceiptDB([FromBody]  ParamReloadDB pRDB)
        {
            Bl.ReloadReceiptDB(pRDB.IdWorkPlace, pRDB.Begin, pRDB.End);
            return null;
        }

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

    public class ParamReloadDB
    {
        public int IdWorkPlace { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
    }
    
}
