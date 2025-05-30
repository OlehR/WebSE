﻿using static QRCoder.PayloadGenerator;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using System;
using System.IO;

namespace WebSE.Mobile
{
    public class CouponMobile
    {
        /// <summary>
        ///   Код інформаційної карти в 1С	000132894
        /// </summary>
        public string reference { get; set; }

        public Int64 reference_promotion { get; set; }
        /// <summary>
        /// штрихкод купона
        /// </summary>
        public string coupon { get; set; } 
        /// <summary>
        /// 0-створений. 1- використаний
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// Час попадання в обмін
        /// </summary>
        public DateTime send_at { get; set; }
    }
}
