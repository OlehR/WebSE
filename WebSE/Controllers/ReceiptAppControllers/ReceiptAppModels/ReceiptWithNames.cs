using ModelMID;

namespace WebSE.Controllers.ReceiptAppControllers.ReceiptAppModels
{
    public class ReceiptWithNames :Receipt
    {
        public string WarehousName { get; set; }
        public string WorkplaceName { get; set; }
    }
}
