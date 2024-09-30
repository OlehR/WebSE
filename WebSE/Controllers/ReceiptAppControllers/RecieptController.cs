using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using Supplyer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebSE;
using WebSE.Controllers.ReceiptAppControllers.ReceiptAppModels;
using WebSE.RecieptModels;

namespace WebSE.Controllers.ReceiptAppControllers
{
    public class RecieptController:BaseController
    {
        [HttpGet]
        [Route("Get/All/Warehouses")]
        public  IEnumerable<WareHouse> GetAllWareHouses()
        {
           ReceiptPostgres recieprPostgres = new ReceiptPostgres();
           var res= recieprPostgres.GetAllWareHouse();
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
        

        /*     [HttpGet]
             [Route("Get/All/Workplace/ByWarehouse/{id}")]
             public IEnumerable<WorkPlace> GetAllWorkPlacesByWarehouse(int id)
             {
                 ReceiptPostgres recieprPostgres = new ReceiptPostgres();
                 var res = recieprPostgres.GetAllWorkPlacesByWarehouse(id);
                 return res;
             }
             [HttpGet]
             [Route ("Get/AllLast/RecieptsByWorkPlace/{codeWorkplace}")]
             public  IEnumerable<Receipt> GetLastReciepts(int codeWorkplace)
             {
                 ReceiptPostgres receiptPostgres = new ReceiptPostgres();
                 var res= receiptPostgres.GetLastReciepts(codeWorkplace);
                 return res;
             }*/
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
        public IEnumerable<ReceiptWithNames> GetReceiptsByDate([FromBody] RequestPayload payload)
        {
            // Extract values from the payload
            var workplacesIds = payload.WorkplacesIds;
            var begin = payload.Begin;
            var end = payload.End;

            ReceiptPostgres receiptPostgres = new ReceiptPostgres();
            var res = receiptPostgres.GetReceiptsByDate(workplacesIds, begin, end);
            return res;
        }
        [HttpPost]
        [Route("Get/All/ReceiptsByDate/Filltered")]
        public IEnumerable<Receipt> GetReceiptsFilltered([FromBody] ReceiptFillter filters)
        {
            ReceiptMsSQL receiptMsSQL = new ReceiptMsSQL();
           
            ReceiptPostgres receiptPostgres = new ReceiptPostgres();
            var res = receiptPostgres.GetReceiptsByDateFilltered(filters);
            return res;
        }
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
    public List<int> WorkplacesIds { get; set; }
    public DateTime Begin { get; set; }
    public DateTime End { get; set; }
}
