//using Newtonsoft.Json;
using ModelMID;
using Newtonsoft.Json;
using Utils;

namespace WebSE
{
    public partial class BL
    {
        public async Task<string> SendReceiptBukovelAsync(IdReceipt pIdR)
        {
            var el = Pg.GetReceipt(pIdR);
            await SendBukovelAsync(el.Receipt, el.Id);
            string res = null;
            return res;
        }

        public async Task SendAllBukovelAsync()
        {
            IEnumerable<LogInput> R = Pg.GetNeedSend(eTypeSend.SendBukovel, 200);
            if (R?.Any() == true)
                foreach (var el in R)
                    await SendBukovelAsync(el.Receipt, el.Id);

        }

        class ResBuk { public DataResBuk data { get; set; } }
        class DataResBuk { public string id { get; set; } }
        public async Task SendBukovelAsync(Receipt pR, long pId)
        {
            try
            {
                pR.IdWorkplacePay = 0;
                ReceiptBukovel r = new(pR);

                var Res = await http.RequestBukovelAsync("https://bills.bukovel.net/api/v1" + "/bills/cart-1", HttpMethod.Post, r.ToJSON("yyyy-MM-dd HH:mm:ss"));
                if (Res != null && Res.status && !string.IsNullOrEmpty(Res.Data))
                {
                    var R = JsonConvert.DeserializeObject<ResBuk>(Res.Data);

                    FileLogger.WriteLogMessage(this, "SendBukovel", $"({pR.IdWorkplace},{pR.CodePeriod} ,{pR.CodeReceipt},{pR.NumberReceipt1C})=> ({Res.status} Id=>{R?.data?.id} data=>{Res.Data})");
                    if (!string.IsNullOrEmpty(R?.data?.id))
                        Pg.ReceiptSetSend(pId, eTypeSend.SendBukovel, Res.Data);
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, $"SendBukovel CodeClient={pR.CodeClient}, ({pR.IdWorkplace},{pR.CodePeriod} ,{pR.CodeReceipt})", e);
            }
        }
        class DataBukovel
        {
            public string date_payment { get; set; }
            public string total_sum_prices { get; set; }
            public string total_sum_payments { get; set; }

        }
        class meta { public int count { get; set; } }
        class r { public meta meta { get; set; } public IEnumerable<DataBukovel> data { get; set; } }

        public async Task<string> ReSendBukovelAsync()
        {
            int skip = 0, add = 0;
            IEnumerable<LogInput> R = Pg.GetNeedSend(eTypeSend.SendBukovel, 0, 20260101, true);
            if (R?.Any() == true)
                foreach (var el in R)
                {
                    var Res = await http.RequestBukovelAsync("https://bills.bukovel.net/api/v1" + $"/bills?document_id={el.NumberReceipt1C}", HttpMethod.Get, null, 5000, "");
                    try
                    {
                        var rr = JsonConvert.DeserializeObject<r>(Res.Data);
                        if (rr?.data?.Any() == true)
                        {
                            var zz = rr?.data?.FirstOrDefault().date_payment;
                            if (zz != null && zz.StartsWith("20"))
                            {
                                if (zz.StartsWith("2025"))
                                {
                                    await SendBukovelAsync(el.Receipt, el.Id);
                                    add++;
                                }
                                else
                                    skip++;
                            }
                        }
                        else skip++;

                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }

                }
            return $" skip=>{skip} add=>{add}";
        }

        bool IsBukovel(int pIdWorkplace) => pIdWorkplace.In(104,105);
    }


    //string date_payment { get; set; }
     class DiscountCard
    {
        public string category { get; set; }
        public int discount_rate { get; set; }
        public string number { get; set; }
        public string owner { get; set; }
        public string validity_date { get; set; } = "2099-12-31";

        public DiscountCard(Client pC)
        {
            category = pC.NameDiscount;
            discount_rate = (int)pC.PersentDiscount;
            number = pC.CodeClient.ToString();
            owner = pC.NameClient;
        }
    }

     class Item
    {
        public string name { get; set; }
        public string discount { get; set; }
        public string price { get; set; }
        public string quantity { get; set; }
        public string code { get; set; }
        public bool is_total_discount { get; set; }
        public Item(ReceiptWares pRW)
        {
            name = pRW.NameWares;
            if (pRW.SumDiscountEKKA > 0)
            {
                discount = pRW.SumDiscountEKKA.ToS();
                is_total_discount = true;
            }
            price = pRW.PriceEKKA.ToS();
            quantity = pRW.Quantity.ToS();
            code = pRW.CodeWares.ToString();
        }
    }

     class payment
    {
        public string value { get; set; }
        public string type { get; set; }
        public string category { get; set; }
        public payment(Payment pP)
        {
            value = pP.SumPay.ToS();
            type = pP.TypePay switch
            {
                eTypePay.Cash => "CASH",
                eTypePay.Card => "CARD",
                eTypePay.Bonus => "DEPOSIT",
                eTypePay.Wallet => "DEPOSIT",
                _ => ""
            };
            category = pP.TypePay switch
            {
                eTypePay.Bonus => "Розрахунок бонусами",
                eTypePay.Wallet => "Завкруглення з скарбнички",
                _ => ""
            };
        }
    }

     class ReceiptBukovel
    {
        public DateTime date_payment { get; set; }
        public string document_id { get; set; }
        public bool difference_in_amounts { get; set; }
        public DiscountCard discount_card { get; set; }
        public string discount { get; set; }
        public bool is_return { get; set; }
        public string number { get; set; }
        public IEnumerable<Item> items { get; set; }
        public IEnumerable<payment> payments { get; set; }

        public ReceiptBukovel(Receipt pR)
        {
            difference_in_amounts = true;
            date_payment = pR.DateReceipt;
            document_id = $"{pR.CodePeriod}-{pR.IdWorkplace}-{pR.NumberReceipt1C}";
            number = pR.NumberReceipt1C;
            if (pR.Client != null)
                discount_card = new DiscountCard(pR.Client);
            items = pR.Wares.Select(Wares => new Item(Wares));
            payments = pR.Payment.Where(x => x.TypePay == eTypePay.Cash || x.TypePay == eTypePay.Card || x.TypePay == eTypePay.Bonus || x.TypePay == eTypePay.Wallet).
                Select(x => new payment(x));
            is_return = pR.TypeReceipt == eTypeReceipt.Refund;
        }
    }
}


