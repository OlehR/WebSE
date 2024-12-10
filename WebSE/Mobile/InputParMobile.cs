using System;
namespace WebSE.Mobile
{
    public class InputParMobile()
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public int limit { get; set; } = 0;
        public int offset { get; set; } = 0;
        public Int64 reference_card { get; set; } = 0;

    }

  
    public class InputParCardsMobile() : InputParMobile
    {
        public int campaign_id { get; set; } = 0;
        public string code { get; set; }
    }

    public class InputParReceiptMobile() : InputParMobile
    {
        public bool is_all_receipt { get; set; } = false;
        
    }
}