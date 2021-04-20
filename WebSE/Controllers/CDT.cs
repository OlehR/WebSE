using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSE;

namespace WebSE.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CDT : ControllerBase
    {
        private readonly ILogger<api> _logger;
        BL Bl = new BL();

        public CDT(ILogger<api> logger)
        {
            _logger = logger;
        }


        [HttpPost]
        [Route("print/")]
        public string print([FromBody] Pr pStr)
        {
            //   if (string.IsNullOrEmpty(pStr))
            //       return null;//new Status(-1, "Невірні вхідні дані");
            string output = JsonConvert.SerializeObject(pStr);
            return http.RequestAsync("http://znp.vopak.local:8088/Print", output,5000, "application/json");
        }       
        
    }

    public class Pr
    {
        public string CodeWares { get; set; }
        public int CodeWarehouse { get; set; }

    }
}
