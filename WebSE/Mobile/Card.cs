using static QRCoder.PayloadGenerator;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using System;
using System.IO;

namespace WebSE.Mobile
{
    public class Card
    {
        /// <summary>
        ///   Код інформаційної карти в 1С	000132894
        /// </summary>
        public string reference { get; set; }
        /// <summary>
        ///   Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        public string code { get; set; }
        /// <summary>
        ///   Код карти повний hm97prk81exsm або *1*0000012461 або 122071307088
        /// </summary>
        string code1 { get; set; }
        /// <summary>
        /// Тип карти   Code128
        /// </summary>
        public string type_code { get; set; }
        /// <summary>
        /// Вид карти Штриховая
        /// </summary>
        public string card_kind { get; set; }
        /// <summary>
        /// Тип карти   Дисконтная
        /// </summary>
        public string card_type { get; set; }
        /// <summary>
        ///  Статус картки Активная
        /// </summary>
        public bool status { get; set; }
        /// <summary>
        /// Код випуску картки	406
        /// </summary>
        public int code_release { get; set; }
        /// <summary>
        /// Володар картки  Мурвич Сергій Сергійович
        /// </summary>
        public string owner_name { get; set; }
        /// <summary>
        /// Код власника картки в 1С	000069567
        /// </summary>
        public string person_code { get; set; }
        /// <summary>
        /// Телефон	380667641077
        /// </summary>
        public string phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string email { get; set; }
        /// <summary>
        /// Дата народження	1976-09-17 
        /// </summary>
        public DateTime birthday { get; set; }
        /// <summary>
        /// Адреса проживання
        /// </summary>
        public string address { get; set; }
        /// <summary>
        ///  Кількість членів сім’ї
        /// </summary>
        public int family_members { get; set; }
        /// <summary>
        /// Стать   Мужской
        /// </summary>
        public string gender { get; set; }
        /// <summary>
        ///  Дата реєстрації картки	2017-12-22
        /// </summary>
        public DateTime registration_date { get; set; }
        /// <summary>
        /// Тип картки	99
        /// </summary>
        public int card_type_id { get; set; }
        /// <summary>
        /// Найменування типу катрки Звичайна
        /// </summary>
        public string card_type_name { get; set; }
        /// <summary>
        /// Місто покупки	10396
        /// </summary>
        public int card_city_id { get; set; }
        /// <summary>
        /// Найменування міста покупки м.Мукачево
        /// </summary>
        public string card_city_name { get; set; }
        /// <summary>
        /// Магазин покупки Коди потрібно узгодити
        /// </summary>
        public int shop_id { get; set; }
        /// <summary>
        /// Мережа (1 – SPAR, 2 – Вигода, 3 - Любо) 1
        /// </summary>
        public int campaign_id { get; set; }
    }
}
