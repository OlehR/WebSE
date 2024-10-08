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
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Cards(InputParMobile pIP)//[FromBody] InputPar pIP)
        {
            return Bl.GetCard(pIP); //Bl.GetCard(pIP).ToString();            
        }
        
        [Route("receipts")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Receipts(InputParMobile pIP)
        {
            return Bl.GetReceipt(pIP);
        }

        [Route("bonuses")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Bonuses(InputParMobile pIP)
        {
            return Bl.GetBonuses(pIP);
        }

        [Route("funds")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Funds(InputParMobile pIP)
        {
            return Bl.GetFunds(pIP);
        }


    }    
}
