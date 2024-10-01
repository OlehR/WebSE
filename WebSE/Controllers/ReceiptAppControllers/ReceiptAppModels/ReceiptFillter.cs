using System.Collections.Generic;
using System;
using ModelMID;

namespace WebSE.Controllers.ReceiptAppControllers.ReceiptAppModels
{
    public class ReceiptFillter
    {
        public List<int> WorkplacesIds { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public eStateReceipt StateReceipt { get; set; }
        public eTypeReceipt TypeReceipt { get; set; }
        public bool isStateReceiptneeded { get; set; }
        public bool isTypeReceiptneeded { get; set; }
        public decimal LowerAmount { get; set; } // Нижня межа суми
        public decimal HigherAmount { get; set; } // Верхня межа суми
        public bool CheckDiscount { get; set; } // Наявність дисконтної картки
        public bool CheckIsCard { get; set; } // Оплата карткою
        public string NumberReceipt { get; set; } // Номер чеку фіскальний
        public string NumberOrder { get; set; } // Номер замовлення
        public long NumberReceiptPOS { get; set; } // Номер чеку термінальний
        public int IdWorkplacePay { get; set; } // ID місця роботи оплати
        public string UserCreate { get; set; } // Номер касира
        public string NumberReceipt1C { get; set; } // Номер у 1С
        public int CodeClient { get; set; } // Код клієнтаou
        public string NameClient {  get; set; }
    }
}
