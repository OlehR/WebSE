using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using UtilNetwork;
using Utils;

namespace WebSE.Controllers
{
    //[Route("/CashRegister")]
    public class CashRegisterController : Controller
    {
        readonly BL Bl;
        public CashRegisterController()
        {
            Bl = BL.GetBL;
        }

        [HttpPost]
        [Route("/CashRegister/LoadData")]
        public Result<MidData> LoadData([FromBody] ModelMID.InLoadData pLD)// 
        {
            var r = Bl.LoadData(pLD);
            //string res = r.Info.ToJson();
            return r;
        }

        [HttpGet]
        [Route("/CashRegister/BuildMID")]
        public string BuildMID()
        {
            var r = Bl.BuldMID();
            return r;
        }

        [HttpPost]
        [Route("/CashRegister/SetPhoneNumber")]
        public Result SetPhoneNumber([FromBody] ModelMID.SetPhone pSPN)
        {
            var r = Bl.SetPhoneNumber(pSPN);
            return r;
        }

        [HttpPost]
        [Route("/CashRegister/SetWeightReceipt")]
        public Result SetWeightReceipt([FromBody] IEnumerable<WeightReceipt> pWR)
        {
            var r = Bl.SetWeightReceipt(pWR);
            return r;
        }
        
    }
}
