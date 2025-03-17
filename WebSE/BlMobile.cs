//using ModelMID;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using Utils;
using WebSE.Mobile;

namespace WebSE
{
    public partial class BL
    {

        public ResultReceiptMobile GetReceipt(InputParReceiptMobile pIP)
        {
            try
            {
                List<ReceiptMobile> Res = new();
                var L = Pg.GetReceipts(pIP);
                foreach (var el in L)
                {
                    var R = el.Receipt;
                    if (R != null)
                        foreach (var IdPay in R.IdWorkplacePays)
                        {
                            R.IdWorkplacePay = IdPay;
                            var RR = new ReceiptMobile(R,el.DateCreate);
                            Res.Add(RR);
                        }
                }
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name ,$"{pIP.ToJson()}=>{Res.Count}");
                return new ResultReceiptMobile() { receipts = Res };
            }
            catch (Exception e) {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + pIP.ToJson(), e);
                return new ResultReceiptMobile(e.Message); }
        }

        public ResultCardMobile GetCard(InputParCardsMobile pIP)
        {
            var R = msSQL.GetClientMobile(pIP);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pIP.ToJson()}=>{R.Count()}");
            return new ResultCardMobile() { cards = R };
        }

        public ResultBonusMobile GetBonuses(InputParMobile pIP)
        {
            var R = msSQL.GetBonusMobile(pIP.from.AddYears(2000), pIP.to.AddYears(2000), pIP.reference_card, pIP.limit, pIP.offset);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pIP.ToJson()}=>{R.Count()}");
            return new ResultBonusMobile() { bonuses = R };
        }

        public ResultFundMobile GetFunds(InputParMobile pIP)
        {
            var R = msSQL.GetMoneyMobile(pIP.from.AddYears(2000), pIP.to.AddYears(2000), pIP.reference_card, pIP.limit, pIP.offset);
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pIP.ToJson()}=>{R.Count()}");
            return new ResultFundMobile() { fundses = R };
        }

        public ResultFixGuideMobile GetFixGuideMobile()
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "");
            return msSQL.GetFixGuideMobile(); 
        }

        public ResultGuideMobile GetGuideMobile(InputParMobile pIP)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pIP.ToJson()}=>");
            return msSQL.GetGuideMobile(pIP); 
        }

        public ResultPromotionMobile<ProductsPromotionMobile> GetPromotionMobile()
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "");
            return msSQL.GetPromotionMobile();
        }

        public ResultPromotionMobile<ProductsKitMobile> GetPromotionKitMobile()
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "");
            return msSQL.GetPromotionKitMobile();
        }
        

    }
}
