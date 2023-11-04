﻿using Microsoft.AspNetCore.Mvc;
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
using BRB5.Model;
using ModelMID;
using ModelMID.DB;

namespace WebSE.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class api : ControllerBase
    {

        private readonly ILogger<api> _logger;
        BL Bl = new BL();
        Raitting cRaitting = new Raitting();

        public api(ILogger<api> logger)
        {
            _logger = logger;
            Bl.GetConfig();
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
        public StatusData SetActiveCard([FromBody] InputCard pCard)
        {
            if (pCard == null)
                return new StatusData(-1, "Невірні вхідні дані");
            return Bl.SetActiveCard(pCard);
        }
        #endregion

        #region Card
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
            if (pContact == null)
                return new StatusData(-1, "Невірні вхідні дані");

            return Bl.CreateCustomerCard(pContact);
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
            }
            catch (Exception e) { return new StatusData(-1, e.Message); }
        }

        [HttpPost]
        [Route("/Receipt")]
        public StatusData Receipt([FromBody] Receipt pR)
        {
            return Bl.SaveReceipt(pR);
        }

        [HttpPost]
        [Route("/CheckExciseStamp")]
        Result<ExciseStamp> CheckExciseStamp(ExciseStamp pES) { return Bl.CheckExciseStamp(); }

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
