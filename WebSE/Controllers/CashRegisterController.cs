using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using UtilNetwork;
using Utils;

namespace WebSE.Controllers
{
    [Route("/CashRegister")]
    public class CashRegisterController : Controller
    {
        readonly BL Bl;
        public CashRegisterController()
        {
            Bl = BL.GetBL;
        }

        [HttpPost]
        [Route("LoadData")]
        public UtilNetwork.Result<MidData> LoadData([FromBody] ModelMID.InLoadData pLD)// 
        {
            var r = Bl.LoadData(pLD);
            //string res = r.Info.ToJson();
            return r;
        }

        [HttpGet]
        [Route("BuildMID")]
        public string BuildMID()
        {
            var r = Bl.BuldMID();
            return r;
        }

        [HttpPost]
        [Route("SetPhoneNumber")]
        public UtilNetwork.Result SetPhoneNumber([FromBody] ModelMID.SetPhone pSPN)
        {
            var r = Bl.SetPhoneNumber(pSPN);
            return r;
        }

        [HttpPost]
        [Route("SetWeightReceipt")]
        public UtilNetwork.Result SetWeightReceipt([FromBody] IEnumerable<WeightReceipt> pWR)
        {
            var r = Bl.SetWeightReceipt(pWR);
            return r;
        }

        [HttpPost]
        [Route("/Receipt")]
        public UtilNetwork.Result Receipt([FromBody] Receipt pR) => Bl.SaveReceipt(pR);


        [HttpPost]
        [Route("/CheckExciseStamp")]
        public UtilNetwork.Result<ExciseStamp> CheckExciseStamp([FromBody] ExciseStamp pES) => Bl.CheckExciseStamp(pES);

        [HttpPost]
        [Route("/CheckOneTime")]
        public UtilNetwork.Result<OneTime> IsOneTime([FromBody] OneTime pOT) => Bl.CheckOneTime(pOT);

        [HttpPost]
        [Route("/CoffeeMachine")]
        public Task<UtilNetwork.Result> CoffeeMachine([FromBody] DateTime pD) => WebSE.CoffeeMachine.SendAsync(pD);
        

    }
}
