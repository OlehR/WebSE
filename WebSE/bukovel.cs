﻿using ModelMID;
using System.Collections.Generic;
using System.Linq;
using Utils;
using System;

namespace WebSE
{
    //string date_payment { get; set; }
    public class DiscountCard
    {
        string category { get; set; }
        int discount_rate { get; set; }
        string number { get; set; }
        string owner { get; set; }
        string validity_date { get; set; } = "2099-12-31";

        public DiscountCard(Client pC)
        {
            category = pC.NameDiscount;
            discount_rate = (int)pC.PersentDiscount;
            number = pC.CodeClient.ToString();
            owner = pC.NameClient;
        }
    }

    public class Item
    {
        string name { get; set; }
        string discount { get; set; }
        string price { get; set; }
        string quantity { get; set; }
        public string is_total_discount { get; set; }
        public Item(ReceiptWares pRW)
        {
            name = pRW.NameDiscount;
            discount = "true";
            is_total_discount=pRW.Discount.ToS();
            price=pRW.Price.ToS();
            quantity = pRW.Quantity.ToS();
        }
    }

    public class payment
    {
        public string value { get; set; }
        public string type { get; set; }
        public payment(Payment pP)
        {
            value = pP.SumPay.ToS();
            type = pP.TypePay switch
            {
                eTypePay.Cash => "CASH",
                eTypePay.Card => "CARD",
                eTypePay.Bonus => "DEPOSIT",
                eTypePay.Wallet => "DEPOSIT",
                _ =>""
            };
        }
    }

    public class ReceiptBukovel
    {
        public DateTime date_payment { get; set; }
        public string document_id { get; set; }
        public DiscountCard discount_card { get; set; }
        public string discount { get; set; }
        public bool is_return { get; set; }
        public string number { get; set; }
        public IEnumerable<Item> items { get; set; }
        public IEnumerable<payment> payments { get; set; }

        public ReceiptBukovel(Receipt pR)
        {
            date_payment = pR.DateReceipt;
            document_id =pR.NumberReceipt1C;
            number= pR.NumberReceipt1C;
            discount_card = new DiscountCard(pR.Client);
            items = pR.Wares.Select(Wares => new Item(Wares));
            payments = pR.Payment.Where(x=> x.TypePay== eTypePay.Cash|| x.TypePay == eTypePay.Card || x.TypePay == eTypePay.Bonus || x.TypePay == eTypePay.Wallet).
                Select(x => new payment(x));
            is_return= pR.TypeReceipt==eTypeReceipt.Refund;
        }
    }
}
