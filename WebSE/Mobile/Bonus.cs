using System;

namespace WebSE.Mobile
{
    public class Bonus
    {
        /// <summary>
        /// Вид руху    Приход
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Дата	2017-12-25 18:14:44
        /// </summary>
        public DateTime bonus_date { get; set; }
        /// <summary>
        /// Сума 	145.04
        /// </summary>
        public decimal bonus_sum { get; set; }
        /// <summary>
        ///  Номер строчки	1
        /// </summary>
        public int row_num { get; set; }
        /// <summary>
        /// Регістратор Чек ККМ V0300001225 від 25.12.2017 18:14:44
        /// </summary>
        public string reg { get; set; }
        /// <summary>
        /// Дата документу	2017-12-25 18:14:44
        /// </summary>
        public DateTime reg_date { get; set; }
        /// <summary>
        /// Номер документу V0300001225
        /// </summary>
        public string reg_number { get; set; }
        /// <summary>
        /// Код інформаційної карти в 1С	000132894
        /// </summary>
        public string reference_card { get; set; }
        /// <summary>
        /// Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        public string code1 { get; set; }
    }

    public class Funds
    {
        /// <summary>
        /// Вид руху    Приход
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Дата	2017-12-25 18:14:44
        /// </summary>
        public DateTime funds_date { get; set; }
        /// <summary>
        /// Сума 	145.04
        /// </summary>
        public decimal funds_sum { get; set; }
        /// <summary>
        ///  Номер строчки	1
        /// </summary>
        public int row_num { get; set; }
        /// <summary>
        /// Регістратор Чек ККМ V0300001225 від 25.12.2017 18:14:44
        /// </summary>
        public string reg { get; set; }
        /// <summary>
        /// Дата документу	2017-12-25 18:14:44
        /// </summary>
        public DateTime reg_date { get; set; }
        /// <summary>
        /// Номер документу V0300001225
        /// </summary>
        public string reg_number { get; set; }
        /// <summary>
        /// Код інформаційної карти в 1С	000132894
        /// </summary>
        public string reference_card { get; set; }
        /// <summary>
        /// Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        public string code1 { get; set; }
    }
}
