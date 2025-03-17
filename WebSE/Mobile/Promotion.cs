using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
namespace WebSE.Mobile
{
    public class PromotionMobile<D>
    {
        public long reference { get; set; }
        public long number { get; set; }
        public DateTime date_beg { get; set; }
        public DateTime date_end { get; set; }
        public string comment { get; set; }
        public IEnumerable<D> products { get; set; }
        public IEnumerable<int> warehouses { get; set; }
        public int data { get; set; }
    }

    public class ProductsPromotionMobile
    {
        [JsonIgnore]
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

    public class ProductsKitMobile
    {
        [JsonIgnore]
        public long number { get; set; }
        public int reference { get; set; }
        //public bool only_ { get; set; }
        public decimal price { get; set; }
    }

}
