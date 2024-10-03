//using ModelMID;
using System;
using System.Collections.Generic;
using WebSE.Mobile;

namespace WebSE
{
    public partial class BL
    {
       
        public ResultMobile GetReceipt(InputPar pIP)
        {
            try
            {
                List<ReceiptMobile> Res = new();
                var L = Pg.GetReceipts(pIP.from, pIP.to, pIP.limit);
                foreach (var el in L)
                {
                    var R = el.Receipt;
                    if (R != null)
                        foreach (var IdPay in R.IdWorkplacePays)
                        {
                            R.IdWorkplacePay = IdPay;
                            var RR = new ReceiptMobile(R);
                            Res.Add(RR);
                        }
                }
                return new ResultMobile() { receipts = Res };
            }
            catch (Exception ex) { return new ResultMobile(ex.Message); }
        }

        public ResultMobile GetCard(InputPar pIP)
        {
            var R = msSQL.GetClientMobile(pIP.from, pIP.to, pIP.limit);
            return new ResultMobile() {cards =R };
        }

        public ResultMobile GetBonuses(InputPar pIP)
        {
            var R = msSQL.GetBonusMobile(pIP.from.AddYears(2000), pIP.to.AddYears(2000), pIP.limit);
            return new ResultMobile() { bonuses = R };
        }

        public ResultMobile GetFunds(InputPar pIP)
        {
            var R = msSQL.GetMoneyMobile(pIP.from.AddYears(2000), pIP.to.AddYears(2000), pIP.limit);
            return new ResultMobile() { fundses = R };
        }

    }
}
