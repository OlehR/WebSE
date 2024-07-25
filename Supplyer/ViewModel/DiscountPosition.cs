using Supplyer.Models;
using System.ComponentModel.DataAnnotations;

namespace Supplyer.ViewModel
{
    public class DiscountPosition
    {
        [Required]
        public double DiscountInitPrice { get; set; }
        public double CompensationAmount { get; set; }
        public double PlanedSales { get; set; }
        [Required]
        public double DiscountPrice { get; set; }
        public string CodeWares {  get; set; }
    }

}
