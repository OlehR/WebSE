using ModelMID;
using WebSE.Controllers.ReceiptAppControllers.ReceiptAppModels;
using SharedLib;
using System;
using System.Collections.Generic;
namespace WebSE.Controllers.ReceiptAppControllers.ReceiptBL
{
    public class ReceiptAppBL
    {

        public LogInput DeleteReceipt(ChangeReceiptModel changeModel)
        {
            Postgres postgres = new Postgres();
            var param = new IdReceipt();
            param.IdWorkplace = changeModel.IdWorkplace;
            param.CodePeriod = changeModel.CodePeriod;
            param.CodeReceipt = changeModel.CodeReceipt;
            var res = postgres.GetReceipt(param);
            if (res != null)
            {
                var r = res.Receipt;
            }
                return res;
        }

            

        public LogInput UpdateReceipt(ChangeReceiptModel changeModel)
        {
            
            Postgres postgres = new Postgres();
            var param = new IdReceipt();
            param.IdWorkplace = changeModel.IdWorkplace;
            param.CodePeriod = changeModel.CodePeriod;
            param.CodeReceipt = changeModel.CodeReceipt;
            var res = postgres.GetReceipt(param);
            if (res != null)
            {
                var r = res.Receipt;
                foreach (var item in r.Wares)
                {
                    foreach (var newItem in changeModel.Wares)
                    {
                        if (item.CodeWares == newItem.CodeWares)
                        {
                           
                            item.Quantity = newItem.Quantity;
                            item.Price = newItem.Price;
                        }
                    }
                }
                BL bL =new BL();
                r.SumReceipt = 0;
                foreach (var ware in r.Wares)
                {
                    r.SumReceipt += ware.Price * ware.Quantity;
                }
                 bL.SaveReceipt(r);

                
            }

            return res;
        }

    }
}
