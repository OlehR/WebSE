using System;
using System.Security.Cryptography;
using System.Text;

public class HashExample
{
    public static void Main(string[] args)
    {
        // Example byte array
        byte[] dataToHash = Encoding.UTF8.GetBytes("This  is some data to hash .");

        // Compute the SHA256 hash
        byte[] hashBytes = ComputeSha256Hash(dataToHash);

        // Convert the hash byte array to a hexadecimal string for display
        string hashString = ConvertBytesToHexString(hashBytes);

        Console.WriteLine($"Original data: {Encoding.UTF8.GetString(dataToHash)}");
        Console.WriteLine($"SHA256 Hash: {hashString}");
    }

    public static byte[] ComputeSha256Hash(byte[] rawData)
    {
        //System.Security.Cryptography.SHA1
        using (var sha256Hash = SHA1.Create())
        {
            // ComputeHash returns a byte array
            byte[] bytes = sha256Hash.ComputeHash(rawData);
            byte x=0;
            for(int i=0;i< bytes.Length;i++)
                x ^= bytes[i];
            bytes[0] = x;
            return bytes;
        }
    }

    public static string ConvertBytesToHexString(byte[] bytes)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2")); // "x2" formats as two lowercase hexadecimal digits
        }
        return builder.ToString();
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