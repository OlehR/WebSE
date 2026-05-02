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
            Bl = BL.GetBL;
        }
        
        [Route("cards")]
        [HttpPost]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Cards([FromBody]  InputParCardsMobile pIP) => Bl.GetCard(pIP);

        [Route("receipts")]
        [HttpPost]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile Receipts([FromBody] InputParReceiptMobile pIP)=> Bl.GetReceipt(pIP);

        [Route("bonuses")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultBonusMobile Bonuses(InputParMobile pIP) => Bl.GetBonuses(pIP);

        [Route("funds")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultFundMobile Funds(InputParMobile pIP) => Bl.GetFunds(pIP);

        [Route("guide")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultFixGuideMobile ProductsFix()=> Bl.GetFixGuideMobile();
        
        [Route("products")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultMobile products(InputParMobile pIP)=> Bl.GetGuideMobile(pIP);        

        [Route("promotion")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultPromotionMobile<ProductsPromotionMobile>  Promotion()=>Bl.GetPromotionMobile();

        [Route("promotionKit")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultPromotionMobile<ProductsKitMobile> PromotionKit() => Bl.GetPromotionKitMobile();

        [Route("coupon")]
        [HttpGet]
        //[ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public ResultCouponMobile Сoupon(InputParMobile pIP) => Bl.GetCouponMobile(pIP);

        [Route("balance")]
        [HttpPost]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public async Task<ResultBalanceMobile> BalanceAsync([FromBody]  InputParBalance pB) => await Bl.GetBalanceAsync(pB);

        [Route("CloseCard")]
        [HttpGet]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        public async Task<ResultMobile> CloseCard(long pCodeClient)=> await Bl.CloseCard(pCodeClient);
    }    
}
