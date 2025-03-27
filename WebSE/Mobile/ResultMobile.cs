using System.Collections.Generic;
//using static System.Runtime.InteropServices.JavaScript.JSType;
namespace WebSE.Mobile
{
    public class ErrorMobile(string pError = null)
    {
        public bool status { get; set; } = !string.IsNullOrEmpty(pError);
        public string error { get; set; } = pError;
    }

    public class ResultMobile(string pError = null)
    {
        public bool status { get; set; } = string.IsNullOrEmpty(pError);
        public ErrorMobile Error { get; set; } = new(pError);        
    }

    public class ResultCardMobile(string pError = null) : ResultMobile(pError)
    {
        public IEnumerable<CardMobile> cards { get; set; } = null;
    }
    public class ResultReceiptMobile(string pError = null) : ResultMobile(pError)
    {
        public IEnumerable<ReceiptMobile> receipts { get; set; } = null;
    }
    public class ResultBonusMobile(string pError = null) : ResultMobile(pError)
    {
        public IEnumerable<Bonus> bonuses { get; set; } = null;
    }

    public class ResultFundMobile(string pError = null) : ResultMobile(pError)
    {
        public IEnumerable<Funds> fundses { get; set; } = null;
    }

    public class ResultGuideMobile(string pError = null) : ResultMobile(pError)
    {
        public IEnumerable<WaresMobile> products { get; set; } = null;
        public IEnumerable<BarCodeMobile> BarCode { get; set; } = null;
        public IEnumerable<PriceMobile> Price { get; set; } = null;
    }

    public class ResultFixGuideMobile(string pError = null) : ResultMobile(pError)
    {
        /// <summary>
        /// Тип номенклатури (товар, тара)
        /// </summary>
        public IEnumerable<GuideMobile> TypeWares { get; set; } = null;
        /// <summary>
        /// Одиниці виміру
        /// </summary>
        public IEnumerable<GuideMobile> Unit { get; set; } = null;
        /// <summary>
        /// ТМ
        /// </summary>
        public IEnumerable<GuideMobile> TM { get; set; } = null;
        /// <summary>
        /// Бренд
        /// </summary>
        public IEnumerable<GuideMobile> Brand { get; set; } = null;
        /// <summary>
        /// Тип ціни.
        /// </summary>
        public IEnumerable<GuideMobile> TypePrice { get; set; } = null;
        /// <summary>
        /// Тип штрихкода.
        /// </summary>
        public IEnumerable<GuideMobile> TypeBarCode { get; set; } = null;
        /// <summary>
        /// Склади
        /// </summary>
        public IEnumerable<GuideMobile> Warehouse { get; set; } = null;
        /// <summary>
        /// TM магазина (Спар, Вопак, Любо)
        /// </summary>
        public IEnumerable<GuideMobile> Campaign { get; set; } = null;
        /// <summary>
        /// Каси
        /// </summary>
        public IEnumerable<GuideMobile> CashDesk { get; set; } = null;

    }


    public class ResultPromotionMobile<D>(string pError = null) : ResultMobile(pError)
    {
        public IEnumerable<PromotionMobile<D>> Promotions { get; set; }
    }

    public class ResultCouponMobile(string pError = null) : ResultMobile(pError)
    {
        /// <summary>
        /// Активні купони
        /// </summary>
        public IEnumerable<CouponMobile> coupon { get; set; }
        
        /// <summary>
        /// кількість накопичених кав, які ще не "використані" для купона Якщо на вхід подано код клієнта
        /// </summary>
        public IEnumerable<ReceiptGiftCoupon> count_for_coupon { get; set; }
    }

    public class ReceiptGiftCoupon
    {
        public Int64 CodePS { get; set; }
        public decimal Quantity { get; set; }
    }
}
