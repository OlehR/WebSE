using BRB5.Model;
using ModelMID;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Utils;

namespace WebSE
{
    public class InputPhone
    {
        string _phone;
        public string phone { get { return _phone; } set { _phone = value.StartsWith("+") ? value.Substring(1) : (IsShortNumber(value) ? "38" + value : value); } }
        [JsonIgnore]
        public string ShortPhone { get { return phone.StartsWith("38") ? phone.Substring(2) : phone; } }
        [JsonIgnore]
        public string FullPhone { get { return "+" + phone; } }
        [JsonIgnore]
        public string FullPhone2 { get { return _phone; } }

        bool IsShortNumber(string pPhone)
        {
            return pPhone.Length == 10 && pPhone.StartsWith("0");
        }
    }

    public class RegisterUser : InputPhone
    {
        //public string phone { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string birthday { get; set; }
        public DateTime GetBirthday { get { return DateTime.ParseExact(birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture); } }
        public int sex { get; set; } //стать 1-чоловіча 2 -жіноча
        //public int GetSex { get { return sex.Equals("female") ? 2 : 1; } }

        public int family { get; set; }
        public int locality { get; set; }
        public int type_of_employment { get; set; } //статус 1 - не працюючий 2 - працюючий 3 - студент 4 пенсіонер
        public int IdExternal { get; set; } = 0;
        public string BarCode { get; set; }
    }

    public class AllInfoBonus : Status
    {
        public AllInfoBonus(int pState = 0, string pTextState = "Ok") : base(pState, pTextState) { }
        public IEnumerable<InfoBonus> cards { get; set; }
    }

    public class InfoBonus
    {
        public InfoBonus() { }
        public string title { get; set; }
        public decimal bonus { get; set; }
        public decimal rest { get; set; }
        public string card { get; set; }
        public string pathCard { get; set; }
    }

    public class Product
    {
        public Product() { }
        public static Product GetFileName(string pFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(pFileName);
            var N = fileName.Substring(1, 1);
            int n = Convert.ToInt32(N);
            return new Product() { id = -n, img = pFileName.Replace("\\", "/"), folder = true, name = $"Сторінка №{N}", };
        }

        public static Product GetPicture(string pFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(pFileName);
            var N2 = fileName.Substring(fileName.Length - 1);
            var N = fileName.Substring(1, 1);
            int n = Convert.ToInt32(N) * 1000 + Convert.ToInt32(N2);
            return new Product() { id = -n, img = pFileName.Replace("\\", "/"), folder = false };
        }

        public static Product GetProduct(Direction pDirection, string pPath)
        {
            var FFileName = Path.Combine(pPath, $"Dir_{pDirection.Code}.png");
            if (!File.Exists(FFileName))
                FFileName = null;
            return new Product() { id = pDirection.Code, name = pDirection.Name, img = FFileName?.Replace(@"\", "/"), folder = true };
        }
        public static Product GetProduct(Wares pWares, string pPath)
        {
            string FFileName = null, FileName = Path.Combine(pPath, $"{pWares.Code:000000000}");
            if (File.Exists($"{FileName}.png"))
                FFileName = $"{FileName}.png";
            else
                if (File.Exists($"{FileName}.jpg"))
                FFileName = $"{FileName}.jpg";

            return new Product() { id = pWares.Code, name = pWares.Name, img = FFileName?.Replace(@"\", "/"), folder = true };
        }

        public int id { get; set; }
        public bool folder { get; set; }
        public string name { get; set; }
        public string img { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public Product[] folderItems { get; set; }

    }

    public class Direction
    {
        public int Code { get; set; }
        public string Name { get; set; }
    }

    public class Wares
    {
        public int Code { get; set; }
        public int CodeDirection { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Promotion : Status
    {
        public Promotion() { }
        public Promotion(int pState = 0, string pTextState = "Ok") : base(pState, pTextState) { }
        public Product[] products { get; set; }
    }

    public class Locality
    {
        public int Id { get; set; }
        public string title { get; set; }
    }

    public class TypeOfEmployment
    {
        public int Id { get; set; }
        public string title { get; set; }
    }

    public class InfoForRegister : Status
    {
        public InfoForRegister() { }
        public InfoForRegister(int pState = 0, string pTextState = "Ok") : base(pState, pTextState) { }
        public IEnumerable<Locality> locality { get; set; }
        public IEnumerable<TypeOfEmployment> typeOfEmployment { get; set; }
    }

    public class Contact : InputPhone
    {
        //ім'я
        public string first_name { get; set; }
        ///прізвище
        public string last_name { get; set; }
        //телефон
        //public string phone { get; set; }
        //city_id
        public string city_id { get; set; }

        public string city_name { get { return Global.GetCity(int.Parse(city_id)); } }
        //email
        public string email { get; set; }
        //день народження
        public string birthday { get; set; }
        //стать 1-чоловіча 2 -жіноча",
        public string gender { get; set; }
        //статус 1 - не працюючий 2 - працюючий 3 - студент 4 пенсіонер",
        public string status { get; set; }
        //кількисть членів сім'ї
        public string family_members { get; set; }
        //1 - Єкартка 2 - Бажаю отримати в магазині 3 - Бажаю отрмати по адресу 4 - Електронна картка
        public string card { get; set; } = "4";
        //якщо card =1
        public string card_number { get; set; }
        /*        //"Ідентифікатор міста магазину. Якщо card=2. Метод store/cities",
                public string card_city { get; set; }
                //Ідентифікатор магазину. Якщо card=2. Метод stores
                public string card_store { get; set; }
                //Ідентифікатор міста отримання.Якщо card= 3.Метод cities
                public string delivery_city { get; set; }
                //Квартира. Якщо card=3
                public string delivery_flat { get; set; }
                //Будинок.Якщо card= 3
                public string delivery_house { get; set; }
                //Ідентифікатор вулиці отримання.Якщо card = 3.Метод streets
                public string delivery_street { get; set; }*/
        public int cards_type_id { get; set; }
        public int campaign_id { get; set; }

        public int trade_lable { get { switch (campaign_id) { case 1: return 2; case 2: return 1; default: return campaign_id; } } }

        public Contact() { }
        public Contact(RegisterUser pRU)
        {
            first_name = pRU.first_name;
            last_name = pRU.last_name;
            phone = pRU.ShortPhone;
            city_id = pRU.locality.ToString();
            email = pRU.email;
            birthday = pRU.birthday;
            gender = pRU.sex.ToString();
            family_members = pRU.family.ToString();
            status = pRU.type_of_employment.ToString();
            campaign_id = 1;
        }
    }

    public class ContactInfoAnsver
    {
        public int id { get; set; }
        public string ecard { get; set; }
    }

    public class ContactAnsver
    {
        public string status { get; set; }
        public ContactInfoAnsver contact;
    }

    public class login
    {
        public login() { }
        public login(Api pA)
        {
            Login = pA.Login;
            PassWord = pA.PassWord;
            BarCodeUser = pA.BarCodeUser;
        }
        string _login;
        public string Login { get { return _login; } set { _login = value?.Replace(".", ""); } }
        public string PassWord { get; set; }
        public string BarCodeUser { get; set; }
        //public string BarCodeUser { get; set; }
    }

    public class InputCard : InputPhone
    {
        public string card { get; set; }
    }

    public class Pr
    {
        public string CodeWares { get; set; }
        public int CodeWarehouse { get; set; }
        //    public string Article { get; set; }
        //     public string NameDocument { get; set; }       
        //    public DateTime Date { get; set; }

    }

    public class ECard
    {
        public string id { get; set; }
        public string ecard { get; set; }
    }

    public class ECardAnsver
    {
        public string status { get; set; }
        public ECard contact { get; set; }
    }

    public class VerifySMS
    {
        public string Phone { get; set; }
        public string Company { get; set; }
    }

    public class LogInput : IdReceipt
    {
        public int Id { get; set; }
        public string JSON { get; set; }
        public int State { get; set; }
        public Receipt Receipt { get {return string.IsNullOrEmpty(JSON)?null: System.Text.Json.JsonSerializer.Deserialize<Receipt>(JSON); } }
        public string Error { get; set; }
        public int CodeError { get; set; }
        public bool IsSend1C { get; set; }
        public int UserCreate { get; set; }
        public DateTime DateCreate { get; set; }
    }
}

