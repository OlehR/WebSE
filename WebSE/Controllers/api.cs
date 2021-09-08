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

namespace WebSE.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class api : ControllerBase
    {

        private readonly ILogger<api> _logger;
        BL Bl = new BL();

        public api(ILogger<api> logger)
        {
            _logger = logger;
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
        [Route("/print")]
        public string print([FromBody] Pr pStr)
        {
            //   if (string.IsNullOrEmpty(pStr))
            //       return null;//new Status(-1, "Невірні вхідні дані");
            string output = JsonConvert.SerializeObject(pStr);
            return http.RequestAsync("http://znp.vopak.local:8088/Print", HttpMethod.Post, output, 5000, "application/json");
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


    }


}
