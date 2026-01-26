using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplyer.Helpers;
using Supplyer.Models;
using Supplyer.Models.Enums;
using Supplyer.ViewModel;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System;
using UtilNetwork;
using WebSE.Controllers;


namespace Supplyer.Controllers
{
    
    public class PositionController : BaseController
    {
        private const string AuthSchemes = CookieAuthenticationDefaults.AuthenticationScheme;

        public PositionController()
        {
           

        }

        [HttpGet]
        [Route("Supplyer/Positions/GetAll")]
        [Authorize(AuthenticationSchemes = AuthSchemes,Roles = "Supplier")]
        public Result<List<SuplierPostition>> GetAll()
        {
            var userName = User.Identity?.Name;
            var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
            Oracle oracle = new Oracle(userName, passwordClaim);

            List<SuplierPostition> postitions = new List<SuplierPostition>();
                var status = oracle.GetAllSuplier();
                return status;
        }
     
     
        [HttpPost]
        [Route("Supplyer/Positions/Create/Changes")]
        [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Supplier")]
        public Result CreateRequest([FromBody]PriceChangeRequestVM requestsVM)
        {

            bool isAllPased = true;
            var userName = User.Identity?.Name;
            var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
            Oracle oracle = new Oracle(userName, passwordClaim);
            if (requestsVM != null)
            {
                if (requestsVM.ProductUpdateDate > DateTime.Now.AddDays(7))
                {
                    foreach (var position in requestsVM.Supliers)
                    {
                        if (position.status == RequestStatus.Accepted &&  position.IsExpired == false)
                        {
                            isAllPased = false;
                            continue;
                        }
                    
                        PriceChangeRequest request = new PriceChangeRequest(position.CodeFirm, requestsVM.ProductUpdateDate, position.CodeWares, position.NewPrice, RequestStatus.Pennding, "");
                    var status = oracle.CreateRequest(request);

                        if (status.status == false) isAllPased = false;

                    }
                    if (isAllPased == false)
                    {
                        return new Result(-1,"Не всі товари були додані (певні позиції вже були прийняті і термін початку поставок не наступив)");
                    }
                    else
                    {
                        return new Result(0, "Все успішно додано");
                    }
                }
                return new Result(-1,"Некоректна дата");

            }
            return new Result(-1,"Некоректні дані");
        }
       
    }
}
