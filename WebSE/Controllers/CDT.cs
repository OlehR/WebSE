using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace WebSE.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Print : ControllerBase
    {
        private readonly ILogger<api> _logger;
        BL Bl = new BL();

        public Print(ILogger<api> logger)
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
            return http.RequestAsync("http://znp.vopak.local:8088/Print", output, 5000, "application/json");
        }

        [HttpPost]
        [Route("api/")]
        public string api([FromBody] dynamic pStr)
        {
            return Bl.ExecuteApi(pStr);
        }


        [HttpPost]
        [Route("Test/")]
        public string Test([FromBody] string pStr)
        {
            string output = Bl.DomainLogin("O.Rutkovskyj","Nataly$75").ToString();
            return output;
        }
    }

    
public class Pr
    {
        public string CodeWares { get; set; }
        public int CodeWarehouse { get; set; }       
    //    public string Article { get; set; }
   //     public string NameDocument { get; set; }       
    //    public DateTime Date { get; set; }

    }
}
