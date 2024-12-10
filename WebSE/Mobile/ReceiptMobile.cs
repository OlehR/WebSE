using Microsoft.OpenApi.Models;
using ModelMID;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;

namespace WebSE.Mobile
{
    public class ReceiptMobile
    {
        /// <summary>
        /// Код чеку в 1С E0217040004
        /// </summary>
        public string reference { get; set; }
        /// <summary>
        ///  Тип операції(1 – Чек, 2 - Повернення)  1
        /// </summary>
        public int operation_type { get; set; }
        /// <summary>
        /// Дата чеку	2023-09-01 07:07:44
        /// </summary>
        public DateTime receipt_date { get; set; }
        /// <summary>
        /// Сума чеку	72.69
        /// </summary>
        public decimal receipt_sum { get; set; }
        /// <summary>
        ///   Сума округлення	0.01
        /// </summary>
        public decimal rounding_sum { get; set; }
        /// <summary>
        /// Номер чеку	231390
        /// </summary>
        public string receipt_number { get; set; }
        /// <summary>
        ///  ЧекПробит на ККМ	1
        /// </summary>
        public int is_processed { get; set; }
        /// <summary>
        /// Код інформаційної карти в 1С	000132894
        /// </summary>
        public string reference_card { get; set; }
        /// <summary>
        ///  Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        public string code { get; set; }
        /// <summary>
        ///  Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        public string code1 { get; set; }
        /// <summary>
        /// Тип карти   Code128
        /// </summary>
        public string type_code { get; set; }
        /// <summary>
        /// Вид карти Штриховая
        /// </summary>
        public string card_kind { get; set; }
        /// <summary>
        /// Тип карти   Дисконтная
        /// </summary>
        public string card_type { get; set; }
        /// <summary>
        /// Код випуску картки	406
        /// </summary>
        public int code_release { get; set; }
        /// <summary>
        /// Володар картки  Мурвич Сергій Сергійович
        /// </summary>
        public string person_name { get; set; }
        /// <summary>
        /// Код власника картки в 1С	000069567
        /// </summary>
        public string person_code { get; set; }
        /// <summary>
        /// Код магазину 1С	000000148
        /// </summary>
        public string store_code { get; set; }
        /// <summary>
        /// Найменування магазину   Ера Торговий Зал
        /// </summary>
        public string store_name { get; set; }
        /// <summary>
        ///  Код каси 1С	000000076
        /// </summary>
        public string cash_code { get; set; }
        /// <summary>
        /// Найменування каси 1С Каса ККМ Ера №2
        /// </summary>
        public string cash_name { get; set; }
        /// <summary>
        /// Сума готівки	0.00
        /// </summary>
        public decimal cash_out_sum { get; set; }
        /// <summary>
        /// Сума доступних бонусів	0.00
        /// </summary>
        public decimal available_bonus_sum { get; set; }
        /// Сума дисконтної карти	0.00
        /// </summary>
        public decimal discount_card_sum { get; set; }
        /// <summary>
        /// Коментар чеку в 1С
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// Оплата	72.69
        /// </summary>
        public decimal payment { get; set; }
        /// <summary>
        /// Код виду оплати 1С	00001
        /// </summary>
        public string payment_type_code { get; set; }
        /// <summary>
        /// Виду оплати Готівкою
        /// </summary>
        public string payment_type_name { get; set; }

        public IEnumerable<Item> products { get; set; }
        public ReceiptMobile(ModelMID.Receipt pR)
        {
            reference = pR.NumberReceipt1C;
            operation_type = pR.TypeReceipt == eTypeReceipt.Sale ? 1 : 2;
            receipt_date = pR.DateReceipt;
            receipt_sum = pR.SumReceipt;
            //rounding_sum=pR.
            receipt_number = pR.NumberReceipt;
            is_processed = 1;
            reference_card = pR.CodeClient.ToString();
            code = pR.Client?.BarCode;
            code1 = pR.Client?.BarCode;
            //store_code =;
            //store_name
            cash_code = pR.IdWorkplace.ToString();
            //cash_name
            cash_out_sum = pR.Payment.Where(e => e.TypePay == eTypePay.Cash).FirstOrDefault()?.SumPay ?? 0;
            available_bonus_sum = pR.Client?.SumBonus ?? 0;
            discount_card_sum = pR.Client?.SumMoneyBonus ?? 0;
            comment = "";
            store_code=ModelMID.Global.GetWorkPlaceByIdWorkplace(pR.IdWorkplace)?.CodeWarehouse.ToString();
            var pay = pR.Payment.Where(e => e.TypePay == eTypePay.Cash || e.TypePay == eTypePay.Card).FirstOrDefault();
            if (pay != null)
            {
                payment = pay.SumPay;
                payment_type_code = pay.TypePay == eTypePay.Cash ? "1" : pay.CodeBank.ToString(); // ((int)pay.TypePay).ToString();
                ///payment_type_name = pay.TypePay.ToString();
            }
            if (pR.Wares?.Any() == true)
                products = pR.Wares.Select(r => new Item(r));            
        }
    }

    public class Item(ModelMID.ReceiptWares pRW)
    {
        /// <summary>
        ///  Номер строчки чеку	1
        /// </summary>
        public int row_num { get; set; } = pRW.Sort;
        /// <summary>
        /// Код продукції 1С	000169417
        /// </summary>
        public string product_code { get; set; } = pRW.CodeWares.ToString();
        /// <summary>
        /// Артикул в номенклатурі	00099061
        /// </summary>
        public string product_vendor_code { get; set; } = "";
        /// <summary>
        ///  Артикул в чеку	00099061
        /// </summary>
        public string vendor_code { get; set; } = "";
        /// <summary>
        /// Найменування продукції  Ролліні з бринзою та шпинатом ваг /ПекЦ/
        /// </summary>
        public string product_name { get; set; } = pRW.NameWares;
        /// <summary>
        /// Код одиниці виміру в 1С	000142743
        /// </summary>
        public string unit_code { get; set; } = pRW.CodeUnit.ToString();
        /// <summary>
        /// Одиниця виміру в 1С кг
        /// </summary>
        public string unit_name { get; set; } = pRW.AbrUnit;
        /// <summary>
        /// Кількість	0.102
        /// </summary>
        public decimal amount { get; set; } = pRW.Quantity;
        /// <summary>
        /// Коефіцієнт	1.0
        /// </summary>
        public decimal koef { get; set; } = 1;
        /// <summary>
        /// Ціна	249.9
        /// </summary>
        public decimal price { get; set; } = pRW.Price;
        /// <summary>
        /// Відсоток скидки	0.0
        /// </summary>
        public decimal discount { get; set; } = pRW.SumDiscount;
        /// <summary>
        /// Відсоток автоматичних скидок	0.0
        /// </summary>
        public decimal auto_discount { get; set; } = 0;
        /// <summary>
        ///  Умова автоматичної скидки 1С Количество одного товара в документе превысило
        /// </summary>
        public string discount_type { get; set; } = pRW.GetStrWaresReceiptPromotion;//pRW.ParPrice1.ToString();
        /// <summary>
        /// Сума	25.49
        /// </summary>
        public decimal row_sum { get; set; } = pRW.Sum;
        /// <summary>
        /// Сума бонусів	0
        /// </summary>
        public decimal bonus_sum { get; set; } = pRW.SumBonus;
        /// <summary>
        /// Штрихкод	2508920100001
        /// </summary>
        public string barcode { get; set; } = pRW.BarCode;
        /// <summary>
        /// Акція(1 – так, 0 - ні) 0
        /// </summary>
        public int is_promotion { get; set; } = pRW.SumDiscount > 0 ? 1 : 0;
        /// <summary>
        /// Акциз(1 – так, 0 - ні) 0
        /// </summary>
        public int is_excise { get; set; } = (pRW.TypeWares == eTypeWares.Alcohol || pRW.TypeWares == eTypeWares.Tobacco || pRW.TypeWares == eTypeWares.TobaccoNoExcise) ? 1 : 0;
    }
}
