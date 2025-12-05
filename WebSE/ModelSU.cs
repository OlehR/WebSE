using BRB5.Model;

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
    public class BaseSU
    {
        public IEnumerable<CategorieSU> categories { get; set; }
        public IEnumerable<ProductSU> products { get; set; }
        public IEnumerable<MekersSU> mekers { get; set; }
    }

    public class ResidueSU
    {
        public ResidueSU() { }
        public ResidueSU(WaresPrice pWP)
        {
            id = (int)pWP.CodeWares;
            price = pWP.Price;
            stock = pWP.Rest;
        }
        public int id { get; set; }
        public decimal price { get; set; }
        public decimal stock { get; set; }
    }

    public class RestSU
    {
        public IEnumerable<ResidueSU> residue { get; set; }
    }
}