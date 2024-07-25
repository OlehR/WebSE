using System;

namespace Supplyer.Models.DiscountModels
{
    public class DiscountPeriodsModel
    {
        public DateTime DateStart {  get; set; }
        public DateTime DateEnd { get; set; }
        public string Comment { get; set; } 
        public string Number { get; set; }
    }
}
