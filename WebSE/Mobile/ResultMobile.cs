using System.Collections.Generic;
namespace WebSE.Mobile
{
    public class ErrorMobile(string pError = null)
    {
        public bool status { get; set; } = !string.IsNullOrEmpty(pError);
        public string error { get; set; } = pError;
    }

    public class ResultMobile(string pError = null)
    {
        public bool status { get; set; } = !string.IsNullOrEmpty(pError);
        public ErrorMobile Error { get; set; } = new(pError);
        public IEnumerable<WebSE.Mobile.CardMobile> cards { get; set; } = null;
        public IEnumerable<WebSE.Mobile.ReceiptMobile> receipts { get; set; } = null;
        public IEnumerable<WebSE.Mobile.Bonus> bonuses { get; set; } = null;
        public IEnumerable<WebSE.Mobile.Funds> fundses { get; set; } = null;
    }    
}
