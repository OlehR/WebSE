using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using NetTools;
using Utils;
using WebSE;
namespace WebSE.Filters
{
    public class ClientIPAddressFilterAttribute : ActionFilterAttribute
    {
        private readonly IEnumerable<IPAddressRange > authorizedRanges;

        public ClientIPAddressFilterAttribute(IIPWhitelistConfiguration configuration)
        {
            
            this.authorizedRanges = configuration.AuthorizedIPAddresses
                .Select(item => IPAddressRange.Parse(item));
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {  
            var clientIPAddress = context.HttpContext.Connection.RemoteIpAddress;
            //context.HttpContext.Request.RouteValues.TryGetValue("controller", out var controller);
            FileLogger.WriteLogMessage($"ActionFilterAttribute IP =>{clientIPAddress.ToString()} ");
            if (!this.authorizedRanges.Any(range => range.Contains(clientIPAddress)))
            {
                FileLogger.WriteLogMessage($"ActionFilterAttribute Block IP =>{clientIPAddress.ToString()} ");
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
