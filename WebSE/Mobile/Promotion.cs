using System;
using System.Collections.Generic;
using System.Linq;
namespace WebSE.Mobile
{
    public class PromotionMobile
    {
        public long number { get; set; }
        public DateTime date_beg { get; set; }
        public DateTime date_end { get; set; }
        public string comment { get; set; }
        public IEnumerable<ProductsPromotionMobile> products { get; set; }
        public IEnumerable<int>  warehouses { get; set; }
    }

    public class ProductsPromotionMobile
    {
        public long number { get; set; }
        public int products { get; set; }
        public int type_price { get; set; }
        public int priority { get; set; }
        public decimal max_priority { get; set; }
    }

    public class PromotionWarehouseMobile
    {
        public long number { get; set; }
        public int warehouse { get; set; }
    }

}
