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
using WebSE.Mobile;

namespace WebSE.Controllers.Mobile
{
    [Route("api/Mobile")]
    public class ApiMobileController : Controller
    {
        BL Bl;
        public ApiMobileController()
        {
            Bl = new BL();

        }
        /*public IActionResult Index()
        {
            return View();
        }*/
        
        [Route("cards")]
        [HttpPost]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Cards([FromBody]  InputParCardsMobile pIP)
        {
            return Bl.GetCard(pIP); //Bl.GetCard(pIP).ToString();            
        }
                

        [Route("receipts")]
        [HttpPost]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Receipts([FromBody] InputParReceiptMobile pIP)
        {
            return Bl.GetReceipt(pIP);
        }

        [Route("bonuses")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultBonusMobile Bonuses(InputParMobile pIP)
        {
            return Bl.GetBonuses(pIP);
        }

        [Route("funds")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultFundMobile Funds(InputParMobile pIP)
        {
            return Bl.GetFunds(pIP);
        }

        [Route("guide")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultFixGuideMobile ProductsFix()
        {
            return Bl.GetFixGuideMobile();
        }

        [Route("products")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile products(InputParMobile pIP)
        {
            return Bl.GetGuideMobile(pIP);
        }

        [Route("promotion")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultPromotionMobile  Promotion()
        {
            return Bl.GetPromotionMobile();
        }
    }    
}
