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
        [Route("IsSync")]
        public Result IsSync() => new();

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
        [Route("Receipt")]
        public Result Receipt([FromBody] Receipt pR) => Bl.SaveReceipt(pR);


        [HttpPost]
        [Route("/CheckExciseStamp")]
        [Route("CheckExciseStamp")]
        public Result<ExciseStamp> CheckExciseStamp([FromBody] ExciseStamp pES) => Bl.CheckExciseStamp(pES);

        [HttpPost]
        [Route("/CheckOneTime")]
        [Route("CheckOneTime")]
        public Result<OneTime> IsOneTime([FromBody] OneTime pOT) => Bl.CheckOneTime(pOT);

        [HttpPost]
        [Route("/CoffeeMachine")]
        public Task<Result> CoffeeMachine([FromBody] DateTime pD) => WebSE.CoffeeMachine.SendAsync(pD);

        [HttpPost]
        [Route("/VisitingShopCenter")]
        public async Task<Result> VisitingShopCenter([FromBody] DateTime pD) => await VisitingSC.RequestAsync(pD);

        [HttpPost]
        [Route("InOutMoney")]
        public async Task<Result> InOutMoney([FromBody] InOutMoney pIOM) => new();// await Bl.InOutMoney(pIOM);

        [HttpPost]
        [Route("OpenCloseShift")]
        public async Task<Result> OpenCloseShift([FromBody] OpenCloseShift pOCS) => new();//await Bl.OpenCloseShift(pOCS);

        [HttpPost]
        [Route("SendLogRRO")]
        public Result SendLogRRO([FromBody] LogRRO pL) => Bl.SendLogRRO(pL);

        [HttpPost]
        [Route("/ReloadReceiptDB")]
        public string ReloadReceiptDB([FromBody] ParamReloadDB pRDB)
        {
            Bl.ReloadReceiptDB(pRDB.IdWorkPlace, pRDB.Begin, pRDB.End);
            return null;
        }

        [HttpPost]
        [Route("/ReloadReceipt")]
        public void ReloadReceipt([FromBody] IdReceipt pIdR)
        {
            Bl.ReloadReceipt(pIdR);
        }


        [HttpPost]
        [Route("/SendReceipt1C")]
        public async Task<string> SendReceipt1CAsync([FromBody] IdReceipt pIdR)
        {
            return await Bl.SendReceipt1CAsync(pIdR);
        }

        [HttpPost]
        [Route("/ReloadBadReceipt")]
        public async Task<string> ReloadBadReceipt([FromBody] int pCodePeriod)
        {
            return await Bl.ReloadBadReceipt(pCodePeriod);
        }


        [HttpPost]
        [Route("/LoadReceiptNo1C")]
        public async Task<string> LoadReceiptNo1C([FromBody] int pCodeReceipt)
        {
            return await Bl.LoadReceiptNo1C(pCodeReceipt);
        }

        [HttpPost]
        [Route("/ReloadReceiptToPG")]
        public async Task<string> ReceiptWaresPromotionNoPrice([FromBody] IdReceipt pIdR)
        {
            return await Bl.ReloadReceiptToPG(pIdR);
        }
        [HttpPost]
        [Route("/ReloadReceiptTo1CQuery")]
        public async Task<string> ReloadReceiptTo1CQuery([FromBody] string pSQL)
        {
               return await Bl.ReloadReceiptTo1CQuery(pSQL.Replace("'", "\""));
        }

        [HttpPost]
        [Route("/ReloadReceiptToPGQuery")]
        public async Task<string> ReloadReceiptToPGQuery([FromBody] string pSQL)
        {
            return await Bl.ReloadReceiptToQuery(pSQL.Replace("''", "\""));
        }

        [HttpPost]
        [Route("/SendBukovel")]
        public async Task<string> SendBukovel([FromBody] IdReceipt pIdR)
        {
            //await Bl.SendAllBukovelAsync();
            await Bl.SendReceiptBukovelAsync(pIdR);
            return "";
        }

        [HttpPost]
        [Route("ReSendBukovel")]
        public async Task<string> ReSendBukovel()
        {
            //await Bl.SendAllBukovelAsync();
            string res = await Bl.ReSendBukovelAsync();
            return res;
        }

        [HttpPost]
        [Route("/Send1CClient")]
        [Route("Send1CClient")]
        public async Task<eReturnClient> Send1CClient([FromBody] ClientNew pC) => await Bl.Send1CClient(pC);

        [HttpPost]
        [Route("/Send1CReceiptWaresDeleted")]
        [Route("Send1CReceiptWaresDeleted")]
        public async Task<bool> Send1CReceiptWaresDeletedAsync([FromBody] IEnumerable<ReceiptWaresDeleted1C> pRWD) => await Bl.Send1CReceiptWaresDeletedAsync(pRWD);


        [HttpPost]
        [Route("/GetClientOrder")]
        [Route("GetClientOrder")]
        public IEnumerable<ReceiptWares> GetClientOrder([FromBody] string pNumberOrder) => Bl.GetClientOrder(pNumberOrder);

        [HttpPost]
        [Route("/GetReceipt1C")]
        [Route("GetReceipt1C")]
        public Dictionary<string, decimal> GetReceipt1C([FromBody] IdReceipt pIdR) => Bl.GetReceipt1C(pIdR);

        [HttpPost]
        [Route("GetDiscount")]
        [Route("/api/GetDiscount")]
        public async Task<Result<Client>> GetDiscountAsync([FromBody] FindClient pFC)
        {
            return await Bl.GetDiscountAsync(pFC);
        }
    }
}
