using ModelMID;
using System.Collections;
using System.Collections.Generic;

namespace WebSE.Controllers.ReceiptAppControllers.ReceiptAppModels
{
    public class ChangeReceiptModel
    {
        public IEnumerable<ReceiptWares>? Wares { get; set; }
        public int IdWorkplace { get; set; }
        public int CodePeriod { get; set; }
        public int CodeReceipt { get; set; }
    }
}
