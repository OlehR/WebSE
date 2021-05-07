using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSE;

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


        [HttpPost]
        [Route("auth/")]
        public Status Auth([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Status(-1, "Невірні вхідні дані");
            return Bl.Auth(pUser);
        }

        [HttpPost]
        [Route("register/")]
        public Status Register([FromBody] RegisterUser pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Status(-1, "Невірні вхідні дані");
            return Bl.Register(pUser);
        }

        [HttpPost]
        [Route("discounts/")]
        public InfoBonus Discounts([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new InfoBonus(-1, "Невірні вхідні дані");
            return Bl.GetBonusAsync(Global.GenBarCodeFromPhone(pUser.phone)).Result;
        }

        [HttpPost]
        [Route("actionsList/")]
        public Promotion ActionsList([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Promotion(-1, "Невірні вхідні дані");

            return Bl.GetPromotion();
        }

        [HttpPost]
        [Route("infoForRegister/")]
        public InfoForRegister GetInfoForRegister([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new InfoForRegister(-1, "Невірні вхідні дані");

            return Bl.GetInfoForRegister();
        }


    }


}
