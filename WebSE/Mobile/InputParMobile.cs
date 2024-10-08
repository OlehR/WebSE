using System;
namespace WebSE.Mobile
{
    public class InputParMobile()
    {
        public DateTime from { get; set; }
        public DateTime to { get; set; }
        public int limit { get; set; } = 0;
        public int offset { get; set; } = 0;
    }
}