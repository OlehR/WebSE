using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSE;
using WebSE.Filters;

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
        [Route("auth/")]
        public Status Auth([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Status(-1, "Невірні вхідні дані");
            return Bl.Auth(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("register/")]
        public Status Register([FromBody] RegisterUser pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Status(-1, "Невірні вхідні дані");
            return Bl.Register(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("discounts/")]
        public InfoBonus Discounts([FromBody] InputPhone pPh)
        {
            if (pPh == null || string.IsNullOrEmpty(pPh.phone))
                return new InfoBonus(-1, "Невірні вхідні дані");
            return Bl.GetBonusAsync(pPh).Result;
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("actionsList/")]
        public Promotion ActionsList([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new Promotion(-1, "Невірні вхідні дані");

            return Bl.GetPromotion();
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("infoForRegister/")]
        public InfoForRegister GetInfoForRegister([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.phone))
                return new InfoForRegister(-1, "Невірні вхідні дані");

            return Bl.GetInfoForRegister();
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

    }


}
