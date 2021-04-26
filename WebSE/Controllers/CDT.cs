using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


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
<<<<<<< HEAD
            return http.RequestAsync("http://znp.vopak.local:8088/Print", output,5000,"application/json");
        }       
        
=======
            return http.RequestAsync("http://znp.vopak.local:8088/Print", output, 5000, "application/json");
        }

        [HttpPost]
        [Route("api/")]
        public string api([FromBody] dynamic pStr)
        {
            return Bl.ExecuteApi(pStr);
        }

>>>>>>> feb84f9f39c2a4a831c15e65c4ae807ac102f7a3
    }

    
public class Pr
    {
        public string CodeWares { get; set; }
        public string CodeWarehouse { get; set; }

    }
}
