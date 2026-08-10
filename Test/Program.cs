using Dapper;
using Front.Equipments;
using Microsoft.Data.SqlClient;
using Model;
using ModelMID;
using Newtonsoft.Json;
using Npgsql;
using SharedLib;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UtilNetwork;
using Utils;
public class HashExample
{
    public static void Main(string[] args)
    {
        
        ReloadReceiptLess(20260728);
        //ReloadReceipt(20260728);
        //ConvertPictures.Convert(@"\\10.100.0.30\Listex", @"\\10.100.0.6\Sim23\audit\Wares");
        return;

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
    class Wp
    {
        public int IdWorkPlace { get; set; }
        public string DNSName { get; set; }
    }

    static void ReloadReceipt(int pCodePeriod)
    {


        string PGInit = "Server=10.1.0.33;Port=5432;User Id=dwreader;Password=DW_Reader;Database=DW;Timeout=300;CommandTimeout=300;Pooling=false"; //"true;Minimum Pool Size=5;Maximum Pool Size=50",
        string MsSqlInit = "Data Source=10.1.0.22;Initial Catalog=DW;User ID=dwreader;Password=DW_Reader;Connection Timeout=30;TrustServerCertificate=True";
        var ConSQL = new SqlConnection(MsSqlInit);
        string SQL = @"SELECT min(code) AS IdWorkPlace, DNSName 
FROM DW.dbo.V1C_CashDesk 
WHERE  len(DNSName)>0 AND code NOT IN (36,8) and code in (29) --(48) -- (29,34,33 ) --in (7 ,6,10, 23, 21,48)
GROUP by DNSName
 ORDER BY 2";
        ConSQL.Open();
        var Wp = ConSQL.Query<Wp>(SQL);
        ConSQL.Close();

        var ConPG = new NpgsqlConnection(connectionString: PGInit);
        ConPG.Open();

        foreach (var el in Wp)
        {
            if ("YRM-KASA-07".Equals(el.DNSName))
                el.DNSName = "10.1.17.17";
            if ("MRK-KASA-01".Equals(el.DNSName))
                el.DNSName = "10.3.5.146";
            if ("MRK-KASA-02".Equals(el.DNSName))
                el.DNSName = "10.3.5.155";
            //Console.WriteLine($"ReloadReceipt Update {el.DNSName} {el.IdWorkPlace} {pCodePeriod}");
            SQL = $@"select max(""State"") from public.""tmp_LogInput"" where  ""IdWorkplace"" ={el.IdWorkPlace} and ""CodePeriod""={pCodePeriod}";
            var Status = ConPG.Query<long?>(SQL).FirstOrDefault();
            if (Status != 100)
            {
                SQL = $@"select max(""CodeReceipt"") from public.""Receipt"" where ""IdWorkplace"" ={el.IdWorkPlace} and ""CodePeriod""={pCodePeriod}";
                long CodeReceipt = ConPG.ExecuteScalar<long>(SQL);

                string json = @"{ ""Command"": 29, ""Data"" : {""TypeDB"":2,""QueryType"":0,""CodePeriod"":" + pCodePeriod + @",""SQL"":""update RECEIPT set STATE_RECEIPT=8 where  STATE_RECEIPT=9 --and Code_receipt>"/* + CodeReceipt.ToString()*/ + @"""}}";

                IPHostEntry hostEntry = null;

                try
                {
                    hostEntry = Dns.GetHostEntry(el.DNSName);
                }
                catch { hostEntry = null; }

                if (hostEntry?.AddressList.Length > 0)
                {
                    SocketClient S = new SocketClient(hostEntry.AddressList[0], 3443);
                    try
                    {
                        var Res = S.StartAsync(json).Result;
                        S = null;
                        if (!Res.Success)
                        {
                            ConPG.Execute(@"insert into public.""tmp_LogInput""(""IdWorkplace"",""CodePeriod"",""State"") values (@IdWorkplace,@pCodePeriod,-1)", new { el.IdWorkPlace, pCodePeriod });
                            Console.WriteLine($"ReloadReceipt Update {el.DNSName} {el.IdWorkPlace} {pCodePeriod} =>Error:{Res.TextError}");
                        }
                        else
                        {
                            json = @"{ ""Command"": 29, ""Data"" : {""TypeDB"":0,""QueryType"":0,""CodePeriod"":" + pCodePeriod + @",""SQL"":""replace into CONFIG  (Name_Var,Data_Var,Type_Var) values ('LastDaySend','" + pCodePeriod.ToString().ToDateTime("yyyyMMdd").ToString("yyyy-MM-dd HH:mm:ss") + @"','System.DateTime')""}}";
                            S = new SocketClient(hostEntry.AddressList[0], 3443);
                            var r = S.StartAsync(json).Result;
                            S = null;
                            if (r.Success)
                            {
                                SQL = $@"insert into public.""tmp_LogInput""(""IdWorkplace"",""CodePeriod"",""State"") values ({el.IdWorkPlace},{pCodePeriod},1)";
                                ConPG.Execute(SQL);
                                Console.WriteLine($"ReloadReceipt Ok {el.DNSName} {el.IdWorkPlace} {pCodePeriod}");
                            }
                            else
                            {
                                ConPG.Execute(@"insert into public.""tmp_LogInput""(""IdWorkplace"",""CodePeriod"",""State"") values (@IdWorkplace,@pCodePeriod,-1)", new { el.IdWorkPlace, pCodePeriod });
                                Console.WriteLine($"ReloadReceipt Config {el.DNSName} {el.IdWorkPlace} {pCodePeriod} =>Error:{Res.TextError}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"error=> {el.DNSName} {el.IdWorkPlace} {e.Message}" );
                    }
                }
                else
                {
                    Console.WriteLine("Невизначено IP>" + el.DNSName);


                }
            }
            else
             {
                //Console.WriteLine($"ReloadReceipt  Already {el.DNSName} {el.IdWorkPlace} {pCodePeriod}");
            }
        }
        ConPG.Close();
    }

    static void ReloadReceiptLess(int pCodePeriod)
    {


        string PGInit = "Server=10.1.0.33;Port=5432;User Id=dwreader;Password=DW_Reader;Database=DW;Timeout=300;CommandTimeout=300;Pooling=false"; //"true;Minimum Pool Size=5;Maximum Pool Size=50",
        string MsSqlInit = "Data Source=10.1.0.22;Initial Catalog=DW;User ID=dwreader;Password=DW_Reader;Connection Timeout=30;TrustServerCertificate=True";
        var ConSQL = new SqlConnection(MsSqlInit);
        string SQL = @"SELECT min(code) AS IdWorkPlace, DNSName 
FROM DW.dbo.V1C_CashDesk 
WHERE  len(DNSName)>0 AND code IN (29) --and code in (29) -- (48) -- (29,34,33 ) --in (7 ,6,10, 23, 21,48)
--and CodeWarehouse in (159) --(114,9,159,345, 314) --89, 57,170,3,
GROUP by DNSName
 ORDER BY 2";
        ConSQL.Open();
        var Wp = ConSQL.Query<Wp>(SQL);
        ConSQL.Close();

        var ConPG = new NpgsqlConnection(connectionString: PGInit);
        ConPG.Open();

        foreach (var el in Wp)
        {

            if ("YRM-KASA-07".Equals(el.DNSName))
                el.DNSName = "10.1.17.17";
            if ("MRK-KASA-01".Equals(el.DNSName))
                el.DNSName = "10.3.5.146";
            if ("MRK-KASA-02".Equals(el.DNSName))
                el.DNSName = "10.3.5.155";
            //Console.WriteLine($"ReloadReceipt Update {el.DNSName} {el.IdWorkPlace} {pCodePeriod}");
            SQL = $@"select max(""State"") from public.""tmp_LogInput"" where  ""IdWorkplace"" ={el.IdWorkPlace} and ""CodePeriod""={pCodePeriod} ";
            var Status = ConPG.Query<long?>(SQL).FirstOrDefault();
            if (Status != 100)
            {
                SQL = $@"select min(""CodeReceipt"") from public.""Receipt"" where ""IdWorkplace"" ={el.IdWorkPlace} and ""CodePeriod""={pCodePeriod}";
                long CodeReceipt = ConPG.ExecuteScalar<long>(SQL);

                string json = @"{ ""Command"": 29, ""Data"" : {""TypeDB"":2,""QueryType"":0,""CodePeriod"":" + pCodePeriod + @",""SQL"":""update RECEIPT set STATE_RECEIPT=8 where  STATE_RECEIPT=9 "/*--and Code_receipt<"  + CodeReceipt.ToString()" */+ @"""}}";
                IPHostEntry hostEntry = null;

                try
                {
                    hostEntry = Dns.GetHostEntry(el.DNSName);
                }
                catch { hostEntry = null; }
                if (hostEntry?.AddressList?.Length > 0)
                {
                    SocketClient S = new SocketClient(hostEntry.AddressList[0], 3443);
                    try
                    {
                        var Res = S.StartAsync(json).Result;
                        S = null;
                        if (!Res.Success)
                        {
                            ConPG.Execute(@"insert into public.""tmp_LogInput""(""IdWorkplace"",""CodePeriod"",""State"") values (@IdWorkplace,@pCodePeriod,-1)", new { el.IdWorkPlace, pCodePeriod });
                            Console.WriteLine($"ReloadReceipt Update {el.DNSName} {el.IdWorkPlace} {pCodePeriod} =>Error:{Res.TextError}");
                        }
                        else
                        {
                            json = @"{ ""Command"": 29, ""Data"" : {""TypeDB"":0,""QueryType"":0,""CodePeriod"":" + pCodePeriod + @",""SQL"":""replace into CONFIG  (Name_Var,Data_Var,Type_Var) values ('LastDaySend','" + pCodePeriod.ToString().ToDateTime("yyyyMMdd").ToString("yyyy-MM-dd HH:mm:ss") + @"','System.DateTime')""}}";
                            S = new SocketClient(hostEntry.AddressList[0], 3443);
                            var r = S.StartAsync(json).Result;
                            S = null;
                            if (r.Success)
                            {
                                SQL = $@"insert into public.""tmp_LogInput""(""IdWorkplace"",""CodePeriod"",""State"") values ({el.IdWorkPlace},{pCodePeriod},1)";
                                ConPG.Execute(SQL);
                                Console.WriteLine($"{el.DNSName} IP=>{hostEntry.AddressList[0]} {el.IdWorkPlace} n=>{Res.Data}");
                            }
                            else
                            {
                                ConPG.Execute(@"insert into public.""tmp_LogInput""(""IdWorkplace"",""CodePeriod"",""State"") values (@IdWorkplace,@pCodePeriod,-1)", new { el.IdWorkPlace, pCodePeriod });
                                Console.WriteLine($"ReloadReceipt Config {el.DNSName} {el.IdWorkPlace} {pCodePeriod} =>Error:{Res.TextError}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"error=> {el.DNSName} IP=>{hostEntry.AddressList[0]} {el.IdWorkPlace} {e.Message[0..40]}");
                    }
                }
                else
                {
                    Console.WriteLine("Невизначено IP>" + el.DNSName);


                }
            }
            else
            {
                //Console.WriteLine($"ReloadReceipt  Already {el.DNSName} {el.IdWorkPlace} {pCodePeriod}");
            }
        }
        ConPG.Close();
    }


    public class ConvertPictures
    {
        static public int Convert(string pPathSource, string pPathDestanation, string pMsSqlInit= "Data Source=10.100.0.24;Database=DW;User ID=o.rutkovskyi;Password=EH8fj2r6;TrustServerCertificate=True;Connection Timeout=120",  string pMask= "*default.*", DateTime pDT=default, int pSize=360)
        {
            try
            {
                SqlConnection connection = new(pMsSqlInit);
                DateTime targetDate = new(2000, 01, 01);
                var directory = new DirectoryInfo(pPathSource);
                var files = directory.EnumerateFiles(pMask).Where(f => f.LastWriteTime.Date > targetDate.Date).OrderBy(f => f.LastWriteTime);
                int i = 0;
                Console.WriteLine($"Всього=>{files.Count()}");
                foreach (var file in files)
                {
                    string Code;
                    var Spl = file.Name.Split('_');

                    string SQL = @$"SELECT TRY_CONVERT(int,dn.code) AS CodeWares FROM  TK_OLAP.dbo.reg_nom_barcodes  b 
JOIN TK_OLAP.dbo.dim_nomen dn ON dn.nomen_id = b.nomen_id
WHERE b.barcode='{Spl[0]}'";
                    Code = connection.ExecuteScalar<string>(SQL);
                    if (Code?.Length > 0)
                    {
                        string DestFileName = Path.Combine(pPathDestanation, "low", $"{Code}.png");
                        try
                        {
                            LibApiDCT.ResizeImage.Convert(file.FullName, DestFileName);
                            i++;
                            Console.WriteLine($"{i} {Code}.png DT=>{file.LastWriteTime}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing file {file.FullName}: {ex.Message}");
                        }
                    }
                    else
                        Console.WriteLine($"Незнайдено {file.FullName}");
                }
                return i;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.Message}");
            }
            return -1;
        }
    }
   public class dd
    {
        public ReceiptBukovel data { get; set; }
    }
    public class DiscountCard
    {
        public string category { get; set; }
        public int discount_rate { get; set; }
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