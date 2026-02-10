using BRB5.Model;
using System.Net;

namespace WebSE
{
    public class CategorieSU
    {
        public string external_id { get; set; }
        public string name { get; set; }
        public string parent_external_id { get; set; }
    }

    public class ProductSU
    {
        public string sku { get; set; }
        public string name { get; set; }
        public bool is_weight_based { get; set; }
        //public string parent_external_id { get; set; }
        public string category_id { get; set; }
        public string description { get; set; }
        public int meker_id { get; set; }
        public string image { get; set; }
    }
    public class MekersSU
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class ShopSU
    {
        public int shop_id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string GPS { get; set; }
    }

    public class BaseSU
    {
        public IEnumerable<CategorieSU> categories { get; set; }
        public IEnumerable<ProductSU> products { get; set; }
        public IEnumerable<MekersSU> mekers { get; set; }
        public IEnumerable<ShopSU> Shop { get; set; }
    }

    public class ResidueSU
    {
        public ResidueSU() { }
        public ResidueSU(WaresPrice pWP,int pCodeWarehouse)
        {
            id = $"{pWP.CodeWares:D9}";
            price = pWP.Price;
            stock = pWP.Rest;
            shop_id = pCodeWarehouse;
        }
        public string id { get; set; }
        public decimal price { get; set; }
        public decimal stock { get; set; }
        public int shop_id { get; set; }
    }

    public class RestSU
    {
        public IEnumerable<ResidueSU> residue { get; set; }
    }
}