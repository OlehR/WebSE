using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebSE
{

    public class RegisterUser: InputPhone
    {
        //public string phone { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string birthday { get; set; }
        public DateTime GetBirthday { get {return DateTime.ParseExact(birthday, "yyyy-MM-dd", CultureInfo.InvariantCulture); } }
        public string sex { get; set; }
        public int GetSex { get { return sex.Equals("female") ? 2 : 1; } }

        public int family { get; set; }
        public string locality { get; set; }
        public string type_of_employment { get; set; }
    }

    public class Status
    {
        public int State { get; set; } = 0;
        public string TextState { get; set; } = "Ok";
        public bool status { get { return State == 0; } }

        public Status(bool pState )
        {
            if(!pState)
            {
                State = -1;
                TextState = "Error";
            }
        }
        public Status(int pState = 0, string pTextState = "Ok")
        {
            State = pState;
            TextState = pTextState;
        }
    }
    public class InputPhone
    {
        public string phone { get; set; }
        public string ShortPhone { get { return phone.StartsWith("+38") ? phone.Substring(3) : phone; } }
    }
    public class InfoBonus
    {
        public decimal bonus { get; set; }
        public decimal rest { get; set; }
        public string card { get; set; }
    }

    public class Product
    {
        public Product() { }
        public static Product GetFileName(string pFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(pFileName);
            var N = fileName.Substring(1,1);
            int n = Convert.ToInt32(N);
            return new Product() { id = -n, img = pFileName, folder = true, name = $"Сторінка №{N}", };
        }

        public static Product GetPicture(string pFileName)
        {
            var fileName = Path.GetFileNameWithoutExtension(pFileName);
            var N2 = fileName.Substring(fileName.Length - 1);
            var N = fileName.Substring(1,1);
            int n = Convert.ToInt32(N)*1000+ Convert.ToInt32(N2);
            return new Product() { id = -n, img = pFileName, folder = false };
        }

        public static Product GetProduct(Direction pDirection,string pPath)
        {            
            return new Product() { id = pDirection.Code, name=pDirection.Name, img = Path.Combine(pPath,$"Dir_{pDirection.Code}.jpg"), folder = true };
        }
        public static Product GetProduct(Wares pWares, string pPath)
        {
            return new Product() { id = pWares.Code, name = pWares.Name, img = Path.Combine(pPath, $"W_{pWares.Code}.jpg"), folder = true };
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

}
