//using ModelMID;
using System;
using System.Collections.Generic;
using WebSE.Mobile;

namespace WebSE
{
    public partial class BL
    {
       
        public ResultReceiptMobile GetReceipt(InputParMobile pIP)
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
                return new ResultReceiptMobile() { receipts = Res };
            }
            catch (Exception ex) { return new ResultReceiptMobile(ex.Message); }
        }

        public ResultCardMobile GetCard(InputParMobile pIP)
        {
            var R = msSQL.GetClientMobile(pIP);
            return new ResultCardMobile() {cards =R };
        }

        public ResultBonusMobile GetBonuses(InputParMobile pIP)
        {
            var R = msSQL.GetBonusMobile(pIP.from.AddYears(2000), pIP.to.AddYears(2000), pIP.limit);
            return new ResultBonusMobile() { bonuses = R };
        }

        public ResultFundMobile GetFunds(InputParMobile pIP)
        {
            var R = msSQL.GetMoneyMobile(pIP.from.AddYears(2000), pIP.to.AddYears(2000), pIP.limit);
            return new ResultFundMobile() { fundses = R };
        }

        public ResultFixGuideMobile GetFixGuideMobile()
        { return msSQL.GetFixGuideMobile(); }

        public ResultGuideMobile GetGuideMobile(InputParMobile pIP)
        { return msSQL.GetGuideMobile( pIP); }

    }
}
