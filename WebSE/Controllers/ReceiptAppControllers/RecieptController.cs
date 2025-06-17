using LibApiDCT.SQL;
using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using WebSE.Controllers.ReceiptAppControllers.ReceiptAppModels;
using WebSE.Controllers.ReceiptAppControllers.ReceiptBL;
using WebSE.RecieptModels;

namespace WebSE.Controllers.ReceiptAppControllers
{
    public class RecieptController:BaseController
    {
        ReceiptPostgres recieprPostgres;
        ReceiptAppBL receiptAppBL;
        BL Bl;
        public RecieptController()
        {
            receiptAppBL = new ReceiptAppBL();
            Bl = BL.GetBL;           
            recieprPostgres = new ReceiptPostgres();
        }
        [HttpDelete("Delete/Receipt")]
        public void DeleteReceipt([FromBody] ChangeReceiptModel changeModel)
        {

            receiptAppBL.DeleteReceipt(changeModel);
        }
        [HttpGet]
        [Route("Get/All/Warehouses")]
        public  IEnumerable<WareHouse> GetAllWareHouses()
        {
            ReceiptPostgres recieprPostgres = new ReceiptPostgres();

       var res = recieprPostgres.GetAllWareHouse();
            return res;
        }
        [HttpPost]
        [Route("Get/All/WorkplacesById")]
        public IEnumerable<WorkPlace> GetWorkplaces([FromBody] List<int> warehousesId)
        {
            ReceiptPostgres recieprPostgres = new ReceiptPostgres();
            var res = recieprPostgres.GetWorkplaces(warehousesId);
            return res;
            
        }

        [HttpPost]
        [Route("Update/Receipt")]
        public LogInput UpdateReceipt([FromBody] ChangeReceiptModel changeModel)
        {
         
            return receiptAppBL.UpdateReceipt(changeModel);
        }
        
        [HttpGet]
             [Route ("Get/All/WaresByReceipt/{workPlaceId}/{recieptId}/{codePeriodId}")]
             public IEnumerable<ModelMID.ReceiptWares> GetAllWaresByReciept(int workPlaceId, int recieptId, int codePeriodId)
             {
                 ReceiptPostgres receiptPostgres = new ReceiptPostgres();
                 var res = receiptPostgres.GetAllWaresByReciept(workPlaceId,recieptId,codePeriodId);
                 return res;
             }
             [HttpGet]          // check
             [Route("Get/Single/LogByReceipt/{workPlaceId}/{recieptId}/{codePeriodId}")]
             public LogRRO GetLogByReceipt(int workPlaceId, int recieptId, int codePeriodId)
             {
                 ReceiptPostgres receiptPostgres = new ReceiptPostgres();
                 var res = receiptPostgres.GetLogByReceipt(workPlaceId, recieptId, codePeriodId);
                 return res;
             }
             [HttpGet]
             [Route ("Get/All/EventsByReceipt/{workPlaceId}/{recieptId}/{codePeriodId}")]
             public IEnumerable<ReceiptEvent> GetEventByReceipt(int workPlaceId, int recieptId, int codePeriodId)
             {
                 ReceiptPostgres receiptPostgres = new ReceiptPostgres();
                 var res = receiptPostgres.GetEventByReceipt(workPlaceId, recieptId, codePeriodId);
                 return res;
             }
            [HttpGet]
            [Route("Get/FullInfo/ByReceipt/{workPlaceId}/{recieptId}/{codePeriodId}")]
            public IActionResult GetFullInfoByReceipt(int workPlaceId, int recieptId, int codePeriodId)
            {
                 ReceiptPostgres receiptPostgres = new ReceiptPostgres();
                 var res = receiptPostgres.GetAllWaresByReciept(workPlaceId, recieptId, codePeriodId);
                 var res1 = receiptPostgres.GetLogByReceipt(workPlaceId, recieptId, codePeriodId);
                 var res2 = receiptPostgres.GetEventByReceipt(workPlaceId, recieptId, codePeriodId);

            var result = new
            {
                Wares = res,
                Logs = res1,
                Events = res2
            };
            return Ok(result);
            }
        [HttpPost]
        [Route("Get/All/ReceiptsByDate")]
        public IEnumerable<Receipt> GetReceiptsByDate([FromBody] RequestPayload  payload)
        {
            // Extract values from the payload
            var workplacesIds = payload.WorkplacesIds;
            var begin = payload.Begin;
            var end = payload.End;

            ReceiptPostgres receiptPostgres = new ReceiptPostgres();
            var res = receiptPostgres.GetReceiptsByDate(workplacesIds, begin, end,payload.fillter);
            return res;
            }
      /*  [HttpPost]
        [Route("Get/All/ReceiptsByDate/Filltered")]
        public IEnumerable<Receipt> GetReceiptsFilltered([FromBody] ReceiptFillter filters)
        {
            //ReceiptMsSQL receiptMsSQL = new ReceiptMsSQL();      
            //ReceiptPostgres receiptPostgres = new ReceiptPostgres();
            //var res = receiptPostgres.GetReceiptsByDateFilltered(filters);
            //return res;
            return new List<Receipt>();
        }*/
        [HttpGet]
        [Route("Get/All/ClientNameByPrefix/{prefix}")]
        public IEnumerable<Client> GetClientsByPrefix(string prefix)
        {
            ReceiptPostgres receiptPostgres = new ReceiptPostgres();
            var res = receiptPostgres.GetClientsByPrefix(prefix);
            return res;
        }

    }
}

// Define a class to model the request payload
public class RequestPayload
{
    public ReceiptFillter fillter { get; set; } 
    public List<int> WorkplacesIds { get; set; }
    public DateTime Begin { get; set; }
    public DateTime End { get; set; }
}
