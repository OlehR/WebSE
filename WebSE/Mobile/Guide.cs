using ModelMID.DB;
using System;

namespace WebSE.Mobile
{

    public class GuideMobile
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }

    public class WaresMobile
    {
        public int reference { get; set; }
        /// <summary>
        /// Артикул
        /// </summary>
        public string vendor_code { get; set; }

        public string name { get; set; } //, --w.name_wares AS title, w.name_wares AS print_title,
        public string print_title { get; set; }
        public string parent_code { get; set; }
        public int is_excise { get; set; }
        public int is_weight { get; set; }
        public decimal tax { get; set; }
        /// <summary>
        /// --Код виду номенклатури
        /// </summary>
        public int type_code { get; set; }
        public int unit_code { get; set; }
        public int brand_code { get; set; }
        public int trademark_code { get; set; }
    }

    public class BarCodeMobile
    {
        /// <summary>
        /// код товару
        /// </summary>
        public int code_products { get; set; }
        public int type_code { get; set; }
        /// <summary>
        /// штрихкод
        /// </summary>
        public string code { get; set; }
    }

    public class PriceMobile
    {
        /// <summary>
        /// код товару 
        /// </summary>
        public int code_products { get; set; }
        /// <summary>
        /// цінова категорія
        /// </summary>
        public int price_type_code { get; set; }
        public int currency_code { get; set; }

        public DateTime price_date { get; set; }
        /// <summary>
        /// Ціна
        /// </summary>
        public decimal price { get; set; }
    }

}
