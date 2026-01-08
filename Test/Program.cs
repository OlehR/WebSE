using Front.Equipments;
using Model;
using ModelMID;
using Newtonsoft.Json;

//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Utils;
public class HashExample
{
    public static void Main(string[] args)
    {
        decimal sum=0,sumR = 0;
        int n=0;
        var Line = File.ReadAllLines("D:\\Log_0_20260104.log");
        List<ReceiptBukovel> RB = new List<ReceiptBukovel>();
        foreach (string el in Line.Where(el => el.Contains("SendBukovel")))
        {
            //File.AppendAllText(@"D:\log.log", el + Environment.NewLine);

            int ind= el.IndexOf("data=>");
            string data = el.Substring(ind+6);
            data=data.Remove(data.Length - 1);
            //Console.WriteLine(data);
            var Res= JsonConvert.DeserializeObject<dd>(data);

            ReceiptBukovel r = Res.data;
            
            foreach (var item in r.payments)
            {
                r.SumReceipt = +item.value.ToDecimal();
            }

            sum += r.SumReceipt;
            foreach (var w in r.items)
            {
                r.SumPay += w.quantity.ToDecimal() * w.price.ToDecimal() - w.discount.ToDecimal();
            }
            sumR += r.SumPay;
            n++;
        }

        Console.WriteLine($"Total Line:{n} sum: {sum}   {sumR}");
        /* var rr= StaticModel.CreateGiftCard(0,77);
         Console.WriteLine($"Original data: {rr}");
         var dd= StaticModel.CheckGiftCard(rr);
         Console.WriteLine($"Original data: {dd}");*/
    }

   public class dd
    {
        public ReceiptBukovel data { get; set; }
    }
    public class DiscountCard
    {
        string category { get; set; }
        int discount_rate { get; set; }
        string number { get; set; }
        string owner { get; set; }
        string validity_date { get; set; } = "2099-12-31";

        
    }

    public class Item
    {
        public string name { get; set; }
        public string discount { get; set; }
        public string price { get; set; }
        public string quantity { get; set; }
        public bool is_total_discount { get; set; }
        public Item()
        {
            
        }
    }

    public class payment
    {
        public string value { get; set; }
        public string type { get; set; }
        public payment()
        {
            
            
        }
    }

    public class ReceiptBukovel
    {
        public decimal  SumReceipt { get; set; }
        public decimal SumPay { get; set; }   
        public DateTime date_payment { get; set; }
        public string document_id { get; set; }
        public bool difference_in_amounts { get; set; }
        public DiscountCard discount_card { get; set; }
        public string discount { get; set; }
        public bool is_return { get; set; }
        public string number { get; set; }
        public IEnumerable<Item> items { get; set; }
        public IEnumerable<payment> payments { get; set; }

        public ReceiptBukovel()
        {
            
        }
    }


    public static bool ValidateWindowsCredentials(string domainName,string username, string password)
    {
        // Determine if the username is a domain user or a local machine user
        // A simple way is to check for a backslash, indicating a domain (e.g., DOMAIN\username)
        // or if the username is just the account name for a local machine.
        ContextType contextType = ContextType.Domain; // Default to local machine
        
        try
        {
            // Create a PrincipalContext for the appropriate context (Domain or Machine)
            using (PrincipalContext pc = new PrincipalContext(contextType, domainName))
            {
                // Validate the credentials
                return pc.ValidateCredentials(username, password);
            }
        }
        catch (PrincipalServerDownException)
        {
            // Handle cases where the domain controller or local machine cannot be reached
            // This might indicate a network issue or an invalid domain name.
            return false;
        }
        catch (Exception ex)
        {
            // Handle other potential exceptions during validation
            Console.WriteLine($"Error validating credentials: {ex.Message}");
            return false;
        }
    }
}

/*using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ModelMID;
using System.Reflection;
using System.Numerics;
using ModelMID.DB;
using Npgsql;
using Dapper;

namespace Test
{
    public enum eTypeSQL
    {
        CreateTeble,
        Insert
    }
    public class Postgres
    {
        //NpgsqlConnection con = new NpgsqlConnection(connectionString: "Server=localhost;Port=5433;User Id=postgres;Password=Nataly75;Database=DW;");
        public Postgres()
        {

        }
        public string CreateTable<T>(eTypeSQL pT = eTypeSQL.CreateTeble)
        {
            string SQL = "";
            foreach (var el in typeof(T).GetProperties())
            {
                if (el.CanWrite)
                {
                    string Type = null;
                    if (el.PropertyType == typeof(int)) Type = "INTEGER NOT null default 0";
                    if (el.PropertyType == typeof(DateTime)) Type = "TIMESTAMP";
                    if (el.PropertyType == typeof(string)) Type = "VARCHAR(2048)";
                    if (el.PropertyType == typeof(Decimal)) Type = "Decimal NOT null default 0";
                    if (el.PropertyType.BaseType == typeof(Enum))
                        Type = "INT";
                    if (el.PropertyType == typeof(System.Int64)) Type = "bigint";
                    if (el.PropertyType == typeof(System.UInt64)) Type = "bigint";
                    if (Type != null)
                    {
                        if (pT == eTypeSQL.CreateTeble) { SQL += $"  \"{el.Name}\" {Type},\n"; }
                        if (pT == eTypeSQL.Insert) { SQL += $"@\"{el.Name}\","; }
                    }
                    else
                        Console.WriteLine($"  {el.Name} {el.PropertyType}");
                }
            }
            if (pT == eTypeSQL.CreateTeble)
            {
                SQL = $"Create table \"{typeof(T).Name}\" \n (\n{SQL.Remove(SQL.Length - 2)}\n)"+
                    "PARTITION BY RANGE (\"CodePeriod\")\n TABLESPACE \"DW\";\n"+
$"CREATE  UNIQUE  INDEX ID_{typeof(T).Name} ON \"{typeof(T).Name}\" (\"IdWorkplace\",\"CodePeriod\",\"CodeReceipt\",\"CodeWares\")\n TABLESPACE \"DWI\";\n";
                for(int i=2023; i <= 2025;i++)
                    for(int j=1;j<=12;j++)
                    {
                        SQL += @$"{Environment.NewLine}CREATE TABLE ""{typeof(T).Name}_{i:D4}{j:D2}"" PARTITION OF ""{typeof(T).Name}""
    FOR VALUES FROM ({i*10000+j*100}) TO ({i*10000+j*100+99})
    TABLESPACE ""DW"";";
                    }

            }

            if (pT == eTypeSQL.Insert)
            {
                SQL = SQL.Remove(SQL.Length - 2);
                SQL = $"Insert into \"DW\".\"{typeof(T).Name}\" ({SQL.Replace("@", "")}) \n values ({SQL.Replace("\"", "").Replace(",", ", ")});";
            }
            return SQL;
        }

        public string InsertTable<T>()
        {
            NpgsqlConnection Con = new NpgsqlConnection(connectionString: "Server=localhost;Port=5433;User Id=postgres;Password=Nataly75;Database=DW;");
            //NpgsqlTransaction Transaction = null;
            Con.Open();                
            
            string Sql = $@"SELECT Column_name FROM information_schema.columns WHERE table_schema = 'public' AND table_name = '{typeof(T).Name}' order by ordinal_position;";
            var Res = Con.Query<string>(Sql);
            string SQL = "";
            foreach(var el in Res)            
                SQL += $"@\"{el}\",";            
            
            SQL = SQL.Remove(SQL.Length - 1);
            SQL = $"insert into \"{typeof(T).Name}\" ({SQL.Replace("@", "")}) \n values ({SQL.Replace("\"", "").Replace(",", ", ")});";
            return SQL;
        }

    }

    public class ReceiptPayment : Payment { }
    public class Log: LogRRO { }

    class Program
    {
        static void Main(string[] args)
        {
            Postgres pg = new Postgres();
            var a =pg.InsertTable<ExciseStamp>(); //pg.CreateTable<WaresReceiptPromotion>(eTypeSQL.CreateTeble);

            //Merge();
            Console.WriteLine(a);

            Thread.Sleep(10000000);

        }

        
      
        static void Merge()
        {
            int width = 400, height = 500;
            Image playbutton;
            try
            {
                playbutton = Image.FromFile(@"D:\Work\WebSE\WebSE\img\BarCode\8800000442402.png");
            }
            catch (Exception ex)
            {
                return;
            }

            Image frame;
            try
            {
                frame = Image.FromFile(@"d:\Spar-logo.png");
            }
            catch (Exception ex)
            {
                return;
            }

            using (frame)
            {
                using (var bitmap = new Bitmap(width, height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(frame,
                                         new Rectangle(0,0,width, height),
                                         new Rectangle(0,0, frame.Width, frame.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(playbutton, 0, 100);
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save(@"d:\res.png",System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    { 
                    }
                }
            }

        }
    }
}*/