using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplyer.DiscountService;
using Supplyer.Helpers;
using Supplyer.Models.DiscountModels;
using Supplyer.Models.Enums;
using Supplyer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using WebSE.Controllers;
using WebSE.Filters;

namespace WebSE.Controllers.Mobile
{
    public class ApiMobileController : BaseController
    {
        /*public IActionResult Index()
        {
            return View();
        }*/
        [Route("Mobile/cards")]
        [HttpPost]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public Status Create([FromBody] AddDiscountVM addDiscount)
        {
            return null;
            
        }

    }
}
