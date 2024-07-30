using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplyer.Helpers;
using Supplyer.Models;
using Supplyer.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Utils;
using WebSE.Controllers;

namespace Supplyer.Controllers
{
    public class RequestController: BaseController
    {
        private const string AuthSchemes = CookieAuthenticationDefaults.AuthenticationScheme;

        [HttpGet]
        [Route("Supplyer/Request/GetAll")]
        [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Manager")]
        public Status<List<SuplierPostition>> GetAllForSuplier()
        {
            var userName = User.Identity?.Name;
            var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
            Oracle oracle = new Oracle(userName, passwordClaim); 
            return oracle.GetAllSpecificationManager();
          
        }
        [HttpPost]
        [Route ("Supplyer/Request/Update")]
        [Authorize(AuthenticationSchemes = AuthSchemes, Roles = "Manager")]

        public Status UpdateRequest([FromBody] ChangeRequestStatus change)
        {
            var userName = User.Identity?.Name;
            var passwordClaim = User.Claims.FirstOrDefault(c => c.Type == "Password")?.Value;
            Oracle oracle = new Oracle(userName, passwordClaim); 
            return oracle.UpdateRequest(change);
        }
    }
}
