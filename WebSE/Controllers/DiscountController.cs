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

namespace Supplyer.Controllers
{
       public class DiscountController : BaseController
       {
           private const string AuthSchemes = CookieAuthenticationDefaults.AuthenticationScheme;

           [Route("Discount/Create/Suplier")]
           [HttpPost]
           [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Supplier")]
           public Status Create([FromBody] AddDiscountVM addDiscount)
           {
            
            
               var userName = User.Identity?.Name;
               var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
               Oracle oracle = new Oracle(userName, passwordClaim);
             var status=  oracle.AddDiscount(addDiscount);
               return status;
           }
           [Route("Discount/GetAllRequests/Suplier")]
           [HttpGet]
           [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Supplier")]
           public List<MargedDiscountModel> GetAllRequestsSuplier()
               {
               try
               {
                   DiscountService.DiscountService service = new DiscountService.DiscountService();
                   var userName = User.Identity?.Name;
                   var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
                   var response = service.GetAllDiscountRequests(userName, passwordClaim,true);
                   return response;
               }
               catch (Exception ex)
               {
                   return new List<MargedDiscountModel>();
               }
           }
           [Route("Discount/GetAll/Adress")]
           [HttpGet]
           [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Supplier")]

           public Status<List<StorageAdressModel>> GetAllDiscAdress()
           {
               DiscountService.DiscountService service = new DiscountService.DiscountService();
               return service.GetAllMergedDiscounts();
           }
           [Route("Discount/GetAll/Time")]
           [HttpGet]
           [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Supplier")]

           public Status<List<DiscountPeriodsModel>> GetAllDiscTime()
           {
               MSSQL mSSQL = new MSSQL();
               return mSSQL.GetAllDiscPeriods();

           }
           [Route("Discount/GetAllRequest/Manager")]
           [HttpGet]
           [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Manager")]
           public List<MargedDiscountModel> GetAllDiscRequests()
           {
               var userName = User.Identity?.Name;
               var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
               DiscountService.DiscountService service = new DiscountService.DiscountService();
               return service.GetAllDiscountRequests(userName, passwordClaim);
           }
           [Route("Discount/Update/status/Manager")]
           [HttpPost]
           [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Manager")]
           public Status ChangeStatus([FromBody] ChangeDiscountRequestStatus model)
           {
               if (model == null)
               {
                   return new Status(-1,"Model is null");
               }
               var userName = User.Identity?.Name;
               var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
               Oracle oracle = new Oracle(userName, passwordClaim);
               return oracle.UpdateStatus(model.status, model.number, model.comment, model.codewares);
            
           }


       }
}
