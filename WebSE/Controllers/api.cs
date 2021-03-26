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
        public Status Auth([FromBody] InputPhone Par)
        {
            return Bl.Auth(Par);
        }

        [HttpPost]
        [Route("register/")]
        public Status Register([FromBody] RegisterUser pUser)
        {
            return Bl.Register(pUser);
        }

        [HttpPost]
        [Route("discounts/")]
        public InfoBonus Discounts([FromBody] InputPhone pUser)
        {
            return  Bl.GetBonusAsync(Global.GenBarCodeFromPhone(pUser.phone)).Result;
        }

        [HttpPost]
        [Route("actionsList/")]
        public object ActionsList([FromBody] InputPhone pUser)
        {
            return Bl.GetPromotion();
        }

    }

   


}
