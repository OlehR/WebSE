//using ModelMID;
using Model;
using SharedLib;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
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
                            var RR = new ReceiptMobile(R, el.DateCreate);
                            Res.Add(RR);
                        }
                }
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pIP.ToJson()}=>{Res.Count}");
                return new ResultReceiptMobile() { receipts = Res };
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + pIP.ToJson(), e);
                return new ResultReceiptMobile(e.Message);
            }
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
        public ResultCouponMobile GetCouponMobile(InputParMobile pIP)
        {
            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, pIP.ToJson());
            return Pg.GetCouponMobile(pIP);
        }

        public async Task<ResultBalanceMobile> GetBalanceAsync(InputParBalance pB)
        {
            //ModelMID.Client Cl = new();
            ResultBalanceMobile Res = new() { };
            List<Balance> Bl = [];
            try
            {
                foreach (var el in pB.reference_card)
                {
                    var Cls = msSQL.GetClient(null, null, null, el);
                    if (Cls?.Count() == 1)
                    {
                        var Cl = Cls.FirstOrDefault();
                        Cl = await DataSync1C.GetBonusAsync(Cl, 0);
                        Bl.Add(new Balance() { reference_card = el, bonus = Cl.SumBonus, funds = Cl.Wallet });
                    }
                }
                Res.balance = Bl;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + pB.ToJson(), e);
                return new ResultBalanceMobile(e.Message);
            }
            return Res;
        }

        public async Task<ResultMobile> CloseCard(long pCodeClient)
        {
            try
            {
                var Cls = msSQL.GetClient(null, null, null, pCodeClient);
                if (Cls?.Count() == 1)
                {
                    var Cl = Cls.FirstOrDefault();
                    var body = SoapTo1C.GenBody("SetStatusCard", [new("CodeOfCard", Cl.BarCode), new("Status", "1")]);
                    var res = await SoapTo1C.RequestAsync(Global.Server1C, body, 5000);
                    if("OK".Equals(res.Data.ToUpper()))
                        return new();
                    else
                      return new("-1".Equals(res.Data) ? "Картка не знайдена" : "Не вдалось записати зміни");                    
                }
                return new($"Знайдено {Cls?.Count()??0} карток");
            }
            catch(Exception e) { return new(e.Message); }
            
        }
    }
}
