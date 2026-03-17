using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using UtilNetwork;

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
        public Result<MidData> LoadData([FromBody] ModelMID.InLoadData pLD)=> Bl.LoadData(pLD);

        [HttpGet]
        [Route("BuildMID")]
        public string BuildMID()=> Bl.BuldMID();

        [HttpPost]
        [Route("SetPhoneNumber")]
        public Result SetPhoneNumber([FromBody] ModelMID.SetPhone pSPN)=> Bl.SetPhoneNumber(pSPN);

        [HttpPost]
        [Route("SetWeightReceipt")]
        public Result SetWeightReceipt([FromBody] IEnumerable<WeightReceipt> pWR)=> Bl.SetWeightReceipt(pWR);

        [HttpPost]
        [Route("/Receipt")]
        public Result Receipt([FromBody] Receipt pR) => Bl.SaveReceipt(pR);


        [HttpPost]
        [Route("/CheckExciseStamp")]
        public Result<ExciseStamp> CheckExciseStamp([FromBody] ExciseStamp pES) => Bl.CheckExciseStamp(pES);

        [HttpPost]
        [Route("/CheckOneTime")]
        public Result<OneTime> IsOneTime([FromBody] OneTime pOT) => Bl.CheckOneTime(pOT);

        [HttpPost]
        [Route("/CoffeeMachine")]
        public Task<Result> CoffeeMachine([FromBody] DateTime pD) => WebSE.CoffeeMachine.SendAsync(pD);

        [HttpPost]
        [Route("/VisitingShopCenter")]
        public async Task<Result> VisitingShopCenter([FromBody] DateTime pD) => await VisitingSC.RequestAsync(pD);
            
    }
}
