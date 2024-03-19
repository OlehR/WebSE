//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QRCoder;
using Utils;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using OfficeOpenXml;
using System.Drawing;
using System.Drawing.Drawing2D;
using BRB5.Model;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System.Reflection;
using System.Timers;
using System.Security.Cryptography;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WebSE
{     
    public class BL
    {
        string UrlDruzi = "http://api.druzi.cards/api/bonus/";
        readonly static object LockCreate = new();
        static BL sBL;
        public static BL GetBL { get { lock (LockCreate) { return sBL ?? new BL(); } } }
        DataSync Ds;
        SoapTo1C soapTo1C;
        WDB_MsSql WDBMsSql;
        MsSQL msSQL;
        GenLabel GL;
        Postgres Pg;
        int DataSyncTime = 0;

        public SortedList<int, string> PrinterWhite = new SortedList<int, string>();
        public SortedList<int, string> PrinterYellow = new SortedList<int, string>();
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        //IEnumerable<int> IsSend;
        //string ListIdWorkPlace;
        public BL()
        {
            if(Global.IsTest)
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version} IsTest=>{Global.IsTest}", eTypeLog.Expanded);

            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version}", eTypeLog.Expanded);
            //!!!TMP Тренба переробити по людськи.
            ModelMID.Global.Settings = new() { CodeWaresWallet = 123 };
            ModelMID.Global.Server1C = "http://bafsrv.vopak.local/psu_utp/ws/ws1.1cws";
            System.Timers.Timer t;
            try
            {
                GetConfig();
                Ds = new(null);
                soapTo1C = new();
                GL = new();
                WDBMsSql = new();
                Pg = new();
                msSQL = new();
                var DW = WDBMsSql.GetDimWorkplace();
                ModelMID.Global.BildWorkplace(DW);
                //IsSend = DW.Where(el => !el.Settings.IsSend1C).Select(el => el.IdWorkplace);
               // ListIdWorkPlace = string.Join(",", IsSend);
               // FileLogger.WriteLogMessage($"IsSend=>({ListIdWorkPlace}) DataSyncTime=>{DataSyncTime}");
                if (DataSyncTime > 0)
                {
                    t = new System.Timers.Timer(DataSyncTime);
                    t.AutoReset = true;
                    t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                    t.Start();
                    OnTimedEvent();
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            sBL = this;
        }

        readonly object Lock = new object();
        async void OnTimedEvent(Object source = null, ElapsedEventArgs e = null)
        {
            lock (Lock)
            {
                try
                {
                    Process proc = Process.GetCurrentProcess();

                    IEnumerable<LogInput> R = Pg.GetNeedSend();
                    FileLogger.WriteLogMessage(this, "WebSE.BL.OnTimedEvent", $"PrivateMemorySize64=>{proc.PrivateMemorySize64} GC=>{GC.GetTotalMemory(false)} Receipt=>{R.Count()}");
                    foreach (var el in R)
                    {
                        //if (IsSend.Any(e => e == el.IdWorkplace))
                       // {
                            Thread.Sleep(100);
                            SendReceipt1C(el.Receipt, el.Id, 0);
                       // }
                    }
                    R = Pg.GetNeedSend( eTypeSend.SendSparUkraine);
                    foreach (var el in R)
                    {
                        if (el.Receipt.CodeClient < 0)
                        {
                            Thread.Sleep(100);
                            _ = SendSparUkraineAsync(el.Receipt, el.Id);
                        }
                        else
                            Pg.ReceiptSetSend(el.Id, eTypeSend.SendSparUkraine);
                    }
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                }
            }
        }

        public string GenCountNeedSend()
        { 
            IEnumerable<LogInput> R1C = Pg.GetNeedSend();
            IEnumerable<LogInput> RSU = Pg.GetNeedSend(eTypeSend.SendSparUkraine);
            return $"1C=>{R1C.Count()} SparUkraine=>{RSU.Count()}";
        }

        public Status Auth(InputPhone pIPh)
        {
            FileLogger.WriteLogMessage($"Auth User=>{pIPh.ShortPhone}");
            try
            {
                var r = msSQL.Auth(pIPh);
                return new Status(r);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"Auth User=>{pIPh.ShortPhone} Error=> {ex.Message}");
                return new Status(-1, ex.Message);
            }
        }

        public Status Register(RegisterUser pUser)
        {
            var strUser = JsonSerializer.Serialize(pUser);
            try
            {
                FileLogger.WriteLogMessage($"Register Start User=>{strUser}");
                var rdd = new InputPhone() { phone = pUser.phone };
                var r = msSQL.Auth(rdd);
                if (r)
                    return new Status();
                try
                {
                    var con = new Contact(pUser);
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(con);
                    var res = new http().SendPostSiteCreate(con);
                    if (res != null && res.status != null && res.status.Equals("success") && res.contact != null)
                    {
                        pUser.IdExternal = res.contact.id;
                        pUser.BarCode = res.contact.ecard;
                        con.card_number = pUser.BarCode;
                    }
                    CreateCustomerCard(con);

                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage($"Register SendPostAsync System Error=>{e.Message} User=>{strUser}");
                }
                return new Status(msSQL.Register(pUser));
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"Register Error=>{e.Message} User=>{strUser}");
                return new Status(-1, e.Message);
            }
        }

        public async Task<AllInfoBonus> GetBonusAsync(InputPhone pPh)
        {
            AllInfoBonus oRes = new AllInfoBonus();
            oRes.cards = msSQL.GetBarCode(pPh);
            //oRes.cards.Ge
            //if (lBarCode.Count()==0)// string.IsNullOrEmpty(pBarCode))
            //pBarCode = Global.GenBarCodeFromPhone(pPh.FullPhone2);

            //oRes.cards.First
            foreach (var el in oRes.cards)
            {

                FileLogger.WriteLogMessage($"GetBonusAsync Start BarCode=>{el.card}");
                try
                {
                    string res;
                    el.pathCard = GetBarCode(el.card);
                    decimal Sum;
                    var body = soapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", el.card) });
                    var res1C = await soapTo1C.RequestAsync(Global.Server1C, body);
                    if (res1C.status)
                    {
                        res = res1C.Data.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                            el.bonus = Sum; //!!!TMP
                    }
                    body = soapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", el.card) });
                    res1C = await soapTo1C.RequestAsync(Global.Server1C, body);

                    res = res1C.Data.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                        el.rest = Sum;
                    //Global.OnClientChanged?.Invoke(parClient, parTerminalId);

                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage($"GetBonusAsync BarCode=>{el.card} Error =>{ex.Message}");
                    oRes.State = -1;
                    oRes.TextState = ex.Message;
                    return oRes;
                    // Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = parTerminalId, Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
                }
            }
            return oRes;
        }

        public Promotion GetPromotion()
        {
            FileLogger.WriteLogMessage($"GetPromotion Start");
            try
            {
                return new Promotion { products = new Product[] { NewsPaper(), YellowPrice() } };
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"GetPromotion Error=>{ex.Message}");
                return new Promotion(-1, ex.Message);
            }
        }

        private Product NewsPaper()
        {
            string pathDir = @"img\";

            var Dirs = Directory.GetDirectories(pathDir, "NP*");
            var CodeNP = Dirs.Max(a => int.Parse(new DirectoryInfo(a).Name.Substring(2)));
            string path = Path.Combine(pathDir, $"NP{CodeNP}");

            var Files = Directory.GetFiles(path, "p?.jpg");
            if (Files == null || Files.Length == 0)
                return null;
            var Res = new Product() { name = "Газета", id = -1, folder = true, description = $"№{CodeNP}", img = Files.First().Replace("\\", "/") };

            Res.folderItems = Files.Select(a => Product.GetFileName(Path.Combine(a))).ToArray();
            for (int i = 0; i < Res.folderItems.Length; i++)
            {
                Files = Directory.GetFiles(path, $"p{-Res.folderItems[i].id}_*.???");
                Res.folderItems[i].folderItems = Files.Select(a => Product.GetPicture(Path.Combine(a))).ToArray();
                //var r2 = JsonConvert.SerializeObject(el);
            };
            //var r = JsonConvert.SerializeObject(Res);
            return Res;
        }

        private Product YellowPrice()
        {
            var pathDirection = @"img\Dir";
            var pathWares = @"img\Wares";
            var Res = new Product() { name = "Жовті цінники", id = -3, folder = true, description = "", img = null };

            var Gr = msSQL.GetDirection();
            var W = msSQL.GetWares();
            Res.folderItems = Gr.Select(a => Product.GetProduct(a, pathDirection)).ToArray();

            for (int i = 0; i < Res.folderItems.Length; i++)
            {
                var el = Res.folderItems[i];
                var r = W.Where(r => r.CodeDirection == el.id).Select(e => Product.GetProduct(e, pathWares)).ToArray();
                el.folderItems = r;
            }
            return Res;
        }

        private string GetBarCode(string pBarCode)
        {
            if (pBarCode.Substring(0, 1).Equals("+"))
                pBarCode = pBarCode.Substring(1);
            string FileName = $"img/BarCode/{pBarCode.Replace("*", "_")}.png";
            if (File.Exists(FileName))
                return FileName;
            try
            {
                Bitmap Logo = new Bitmap(Image.FromFile(@"img/BarCode/Spar.png"));
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode($"{pBarCode}", QRCodeGenerator.ECCLevel.H);
                var qrCode = new QRCode(qrCodeData);
                qrCode.GetGraphic(12, Color.FromArgb(0, 123, 62), System.Drawing.Color.White, Logo, 25, 1).Save(FileName, System.Drawing.Imaging.ImageFormat.Png);

                //Merge(el, FileName);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"GetBarCode BarCode=>{pBarCode} FileName=>{FileName} Error =>{ex.Message}");
                return null;
            }
            return FileName;

        }

        void Merge(Image playbutton, string FileName)

        {
            int width = 350, height = 400;
            Image frame;
            try
            {
                frame = Image.FromFile(@"img/BarCode/Spar-logo.png");
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
                                         new Rectangle(0, 0, width, height),
                                         new Rectangle(0, 0, frame.Width, frame.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(playbutton, 10, 100);
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save(FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

        }

        class L { public IEnumerable<Locality> cities { get; set; } }
        public InfoForRegister GetInfoForRegister()
        {
            return new InfoForRegister()
            {
                locality = Global.Citys /*msSQL.GetLocality()*/,
                typeOfEmployment = new TypeOfEmployment[]
                {
                    new TypeOfEmployment { Id = 1, title = "не працюючий" },
                    new TypeOfEmployment { Id = 2, title = "працюючий" },
                    new TypeOfEmployment { Id = 3, title = "студент" },
                    new TypeOfEmployment { Id = 4, title = "пенсіонер" },

                }
            };
        }

        public string ExecuteApi(dynamic pStr, login l)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };

            string res = JsonSerializer.Serialize(pStr, options);

            Oracle oracle = new Oracle(l);
            var Res = oracle.ExecuteApi(res);
            return Res;
        }

        public login GetLoginByBarCode(string pBarCode)
        {
            return null;
        }

        public Result<WaresPrice> GetPrice(ApiPrice pAP)
        {
            var LR = Login(new login(pAP));
            if (LR.State != 0)
            {
                return new Result<WaresPrice>(LR.State, LR.TextError);
            }
            try
            {
                var r = msSQL.GetPrice(pAP);
                return new Result<WaresPrice>() { Info = r };
            }
            catch (Exception e) { return new Result<WaresPrice>(e); }
        }

        public bool DomainLogin(string pLogin, string pPassWord)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            System.DirectoryServices.AccountManagement.PrincipalContext prCont = new System.DirectoryServices.AccountManagement.PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Domain, "Vopak");
            return prCont.ValidateCredentials(pLogin, pPassWord);
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public string test()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                //Set some properties of the Excel document
                excelPackage.Workbook.Properties.Author = "VDWWD";
                excelPackage.Workbook.Properties.Title = "Title of Document";
                excelPackage.Workbook.Properties.Subject = "EPPlus demo export data";
                excelPackage.Workbook.Properties.Created = DateTime.Now;

                //Create the WorkSheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet 1");

                //Add some text to cell A1
                worksheet.Cells["A1"].Value = "My first EPPlus spreadsheet!";
                //You could also use [line, column] notation:
                worksheet.Cells[1, 2].Value = "This is cell B1!";

                //Save your file
                FileInfo fi = new FileInfo(@"d:\File.xlsx");
                excelPackage.SaveAs(fi);
            }
            return "Ok";
        }

        static int Day = 0;
        static int Count = 0;
        bool IsLimit()
        {
            if (Day != DateTime.Now.Day)
            {
                Day = DateTime.Now.Day;
                Count = 0;
            }
            return (++Count > 500);
        }

        public (string,login) Znp(dynamic pStr,login pL=null)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };

                string res = JsonSerializer.Serialize(pStr, options);
                var l = JsonSerializer.Deserialize<login>(res);

                if (!string.IsNullOrEmpty(l.BarCodeUser))
                {
                    l = GetLoginByBarCode(l.BarCodeUser);
                }
                if ((string.IsNullOrEmpty(l.Login) || string.IsNullOrEmpty(l.PassWord)) && pL!=null && !string.IsNullOrEmpty(pL.Login) && !string.IsNullOrEmpty(pL.PassWord))
                    l = pL;

                if (!string.IsNullOrEmpty(l.Login) && !string.IsNullOrEmpty(l.PassWord))
                    return (ExecuteApi(pStr, l),l);
                else
                    return ("{\"State\": -1,\"Procedure\": \"C#\\Api\",\"TextError\":\"Відсутній Логін\\Пароль\"}",l);

            }
            catch (Exception e)
            {
                return ($"{{\"State\": -1,\"Procedure\": \"C#\\Api\",\"TextError\":\"{e.Message}\"}}",null);
            }
        }

        public Status<string> FindByPhoneNumber(InputPhone pUser)
        {
            if (IsLimit())
                return new Status<string>(-1, $"Перевищено денний Ліміт=>{Count}");

            var body = soapTo1C.GenBody("FindByPhoneNumber", new Parameters[] { new Parameters("NumDocum", "j" + pUser.phone) });
            var res = soapTo1C.RequestAsync(Global.Server1C, body, 100000, "text/xml", "Администратор:0000").Result; // @"http://1csrv.vopak.local/TEST2_UTPPSU/ws/ws1.1cws"
            FileLogger.WriteLogMessage($"FindByPhoneNumber Phone=>{pUser.ShortPhone} State=> {res.State} TextState =>{res.TextState} Data=>{res.Data}");
            return res;
        }

        public Status<string> CreateCustomerCard(Contact pContact)
        {
            if (IsLimit())
                return new Status<string>(-1, $"Перевищено денний Ліміт=>{Count}");

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(pContact);
            string s = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            var body = soapTo1C.GenBody("CreateCustomerCard", new Parameters[] { new Parameters("JSONSting", s) });
            var res = soapTo1C.RequestAsync(Global.Server1C, body, 100000, "text/xml", "Администратор:0000").Result;
            FileLogger.WriteLogMessage($"CreateCustomerCard Contact=>{json} State=> {res.State} TextState =>{res.TextState} Data=>{res.Data}");
            return res;
        }

        public Status<string> SetActiveCard(InputCard pCard)
        {
            msSQL.SetActiveCard(pCard);
            return new Status<string>();
        }

        public Result<login> Login(login l)
        {
            //Result<login> res;

            if (!string.IsNullOrEmpty(l.BarCodeUser))
            {
                l = GetLoginByBarCode(l.BarCodeUser);
            }

            if (!string.IsNullOrEmpty(l.Login) && !string.IsNullOrEmpty(l.PassWord))
                return new Result<login>() { Info = l };
            else
                return new Result<login>() { State = -1, TextError = "Відсутній Логін\\Пароль" };
        }

        public void GetConfig()
        {
            DataSyncTime = Startup.Configuration.GetValue<int>("ReceiptServer:DataSyncTime");


            var Printer = new List<Printers>();
            Startup.Configuration.GetSection("PrintServer:PrinterWhite").Bind(Printer);
            foreach (var el in Printer)
                if (!PrinterWhite.ContainsKey(el.Warehouse))
                    PrinterWhite.Add(el.Warehouse, el.Printer);

            Printer.Clear();
            Startup.Configuration.GetSection("PrintServer:PrinterYellow").Bind(Printer);
            foreach (var el in Printer)
                if (!PrinterYellow.ContainsKey(el.Warehouse))
                    PrinterYellow.Add(el.Warehouse, el.Printer);
        }

        public string Print(WaresGL pWares)
        {
            try
            {
                if (pWares == null)
                    return "Bad input Data: Wares";
                Console.WriteLine(pWares.CodeWares);

                if (pWares.CodeWarehouse == 0)
                    return "Bad input Data:CodeWarehouse";

                string NamePrinterYelow = PrinterYellow[pWares.CodeWarehouse];
                string NamePrinter = PrinterWhite[pWares.CodeWarehouse];
                if (string.IsNullOrEmpty(NamePrinter))
                    return $"Відсутній принтер: NamePrinter_{pWares.CodeWarehouse}";

                //int  x = 343 / y;
                var ListWares = GL.GetCode(pWares.CodeWarehouse, pWares.CodeWares);//"000140296,000055083,000055053"
                if (ListWares.Count() > 0)
                    GL.Print(ListWares, NamePrinter, NamePrinterYelow, $"Label_{pWares.NameDCT}_{pWares.Login}", pWares.BrandName, pWares.CodeWarehouse != 89, pWares.CodeWarehouse != 22 && pWares.CodeWarehouse != 3 && pWares.CodeWarehouse != 163 && pWares.CodeWarehouse != 170);// pWares.CodeWarehouse == 9 || pWares.CodeWarehouse == 148 || pWares.CodeWarehouse == 188);  //PrintPreview();
                FileLogger.WriteLogMessage($"\n{DateTime.Now.ToString()} Warehouse=> {pWares.CodeWarehouse} Count=> {ListWares.Count()} Login=>{pWares.Login} SN=>{pWares.SerialNumber} NameDCT=>{pWares.NameDCT} Wares=>{pWares.CodeWares}");

                return $"Print=>{ListWares.Count()}";

            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"\n{DateTime.Now.ToString()}\nInputData=>{pWares.CodeWares}\n{ex.Message} \n{ex.StackTrace}");
                return "Error=>" + ex.Message;
            }
        }

        public Status SaveReceipt(Receipt pR)
        {
            int Id = Pg.SaveLogReceipt(pR);
            if (Id > 0)
            {
                Pg.SaveReceipt(pR, Id);
                SendReceipt1C(pR, Id);
                FixExciseStamp(pR);
                //Якщо кліент SPAR Україна
                if(pR.CodeClient<0)
                    _ = SendSparUkraineAsync(pR, Id);
            }
            return new Status(Id > 0 ? 0 : -1);
        }

        public void SendReceipt1C(Receipt pR, int pId, int pWait = 5000)
        {
            if (Global.IsTest) return;
            //if (pR.IdWorkplace == 23 || pR.IdWorkplace == 7) //Тест новий 5 та 11 каса
            //if (IsSend.Any(e => e == pR.IdWorkplace))
                Task.Run(async () =>
                {
                    try
                    {
                        Thread.Sleep(pWait);
                        var res = await Ds.Ds1C.SendReceiptTo1CAsync(pR, Global.Server1C, false);
                        //FileLogger.WriteLogMessage(this, "SendReceiptTo1CAsync", $" {pR.IdWorkplace} {pR.CodePeriod} {pR.CodeReceipt} res=>{res}");
                        if (res) Pg.ReceiptSetSend(pId);
                    }
                    catch (Exception e)
                    {
                        FileLogger.WriteLogMessage(this, "SendReceiptTo1CAsync", e);
                    }
                });
        }

        public Status<ExciseStamp> CheckExciseStamp(ExciseStamp pES)
        {
            try
            {
                ExciseStamp res = Pg.CheckExciseStamp(pES);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pES.ToJson()} =>{res.ToJson()}");
                return new Status<ExciseStamp>() { Data = res };
            }
            catch (Exception ex) {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name +pES?.ToJson(), ex);
                return new Status<ExciseStamp>(ex); 
            }
        }
        public Result<IEnumerable<Doc>> GetPromotion(int pCodeWarehouse)
        {
            try
            {
                var res = msSQL.GetPromotion(pCodeWarehouse);
                return new Result<IEnumerable<Doc>>() { Info = res };
            }
            catch (Exception ex) { return new Result<IEnumerable<Doc>>(ex); }
        }
        public Result<IEnumerable<DocWares>> GetPromotionData(string pNumberDoc)
        {
            try
            {
                var res = msSQL.GetPromotionData(pNumberDoc);
                return new Result<IEnumerable<DocWares>>() { Info = res };
            }
            catch (Exception ex) { return new Result<IEnumerable<DocWares>>(ex); }
        }

        public void FixExciseStamp(Receipt pR)
        {
            foreach (var el in pR.Wares.Where(x => x.GetExciseStamp?.Any() == true))
                foreach (var Stamp in el.GetExciseStamp)
                {
                    Pg.CheckExciseStamp(new ExciseStamp(el, Stamp, pR.TypeReceipt == eTypeReceipt.Refund ? eStateExciseStamp.Return : eStateExciseStamp.Used), true);
                }
            Pg.DeleteExciseStamp(pR);
        }

        public Client GetClientPhone(string pPhone)
        {
            //var Res = http.RequestFrendsAsync("http://api.druzi.cards/api/bonus/card-by-phone", HttpMethod.Post, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("phone", pPhone) });
            return new Client() { };
        }

        public Result SaveDocData( ApiSaveDoc pD)
        {
           if(pD.TypeDoc==2) //Якщо замовлення то в Oracle
            {
                Znp(pD);
            }
            else
            {
                msSQL.SaveDocData(pD);
            }
            return null;
        }

        public async Task<Status<Client>> GetDiscountAsync(FindClient pFC)
        {
            try
            {
                if (pFC.Client != null) //Якщо наша карточка
                {
                    //msSQL.GetClient(parCodeClient=>)
                    Client r = await sBL.Ds.Ds1C.GetBonusAsync(pFC.Client, pFC.CodeWarehouse);
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"({pFC.ToJson()})=>({r.ToJson()})");
                    return new Status<Client>(r);
                }
                else//Якщо друзі
                {
                    //return new Status<Client>();
                    Status<string> Res = null;
                    if (string.IsNullOrEmpty(pFC.BarCode))
                    {
                        string CardCode = null;
                        if (!string.IsNullOrEmpty(pFC.BarCode)) CardCode = pFC.BarCode;
                        else
                            if (pFC.PinCode > 0)
                        {
                            Res = await http.RequestFrendsAsync(UrlDruzi + "card-by-pin", HttpMethod.Post, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("pin", $"{pFC.PinCode:D4}") });
                        }
                        else
                        if (!string.IsNullOrEmpty(pFC.Phone))
                        {
                            string Ph = Global.GetFullPhone(pFC.Phone);
                            if (Ph != null)
                                Res = await http.RequestFrendsAsync(UrlDruzi + "card-by-phone", HttpMethod.Post, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("phone", Ph) });
                        }
                        if (Res != null && Res.status)
                        {
                            var res = JsonSerializer.Deserialize<AnsverDruzi<string>>(Res.Data);
                            if (res.status) CardCode = res.data;
                        }

                        if (!string.IsNullOrEmpty(CardCode))
                        {
                            Res = await http.RequestFrendsAsync(UrlDruzi + "balance", HttpMethod.Post, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("card_code", CardCode) });
                            if (Res != null && Res.status)
                            {
                                var Bonus = JsonSerializer.Deserialize<AnsverDruzi<AnsverBonus>>(Res.Data);
                                if (Bonus.status)
                                {
                                    Pg.InsertClientData(new ClientData { CodeClient = -Bonus.data.CardId, TypeData = eTypeDataClient.BarCode, Data = CardCode });
                                    return new Status<Client>(new Client()
                                    { CodeClient = -Bonus.data.CardId, NameClient = $"Клієнт SPAR Україна {Bonus.data.CardId}", SumBonus = Bonus.data.bonus_sum, SumMoneyBonus = "0".Equals(Bonus.data.is_treated) ? 0 : Bonus.data.SumMoneyBonus, Wallet = Bonus.data.Wallet, BirthDay = Bonus.data.birth_date });
                                }
                            }
                        }
                    }
                }
                return new Status<Client>();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + $" {pFC.ToJson()}", e);
                return new Status<Client>(e);
            }
        }

        public async Task SendSparUkraineAsync(Receipt pR, int pId)
        {            
            try
            {
                int SumBonus = 0, SumWallet = 0;
                SumBonus = -(int)(pR.Payment?.Where(el => el.TypePay == eTypePay.Bonus)?.FirstOrDefault()?.SumPay * 100??0);
                if (SumBonus == 0)
                    SumBonus = (int)Math.Round(pR.SumReceipt,0);
                SumWallet = -(int)(pR.Payment?.Where(el => el.TypePay == eTypePay.Wallet)?.FirstOrDefault()?.SumPay * 100??0);
                string BarCodeCard = Pg.GetBarCode(pR.CodeClient);
                int ObjectId = ModelMID.Global.GetWorkPlaceByIdWorkplace(pR.IdWorkplace)?.Settings?.CodeWarehouseExSystem ?? 0;//MsSQL.GetObjectId();
                var Param = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("card_code", BarCodeCard),
                new KeyValuePair<string, string>("object_id", $"{ObjectId}" ),
                new KeyValuePair<string, string>("bonus_sum", $"{SumBonus}"),
                new KeyValuePair<string, string>("safe_sum", $"{SumWallet}"),
                new KeyValuePair<string, string>("check_num", pR.NumberReceipt1C),
                new KeyValuePair<string, string>("check_sum",((int)(pR.SumReceipt*100m)).ToString())
            };

                var Res = await http.RequestFrendsAsync(UrlDruzi + "change", HttpMethod.Post, Param);
                if (Res != null && Res.status)
                {
                    FileLogger.WriteLogMessage(this, "SendSparUkraine", $"({pR.IdWorkplace},{pR.CodePeriod} ,{pR.CodeReceipt},{pR.NumberReceipt1C},{BarCodeCard},{ObjectId},{SumBonus},{SumWallet})=> ({Res.status} data=>{Res.Data})");
                    var res = JsonSerializer.Deserialize<AnsverDruzi<string>>(Res.Data);
                    if (res.status)
                    {
                        Pg.ReceiptSetSend(pId, eTypeSend.SendSparUkraine);
                    }
                }
            }catch (Exception e) 
            {
                FileLogger.WriteLogMessage(this, $"SendSparUkraine CodeClient={pR.CodeClient}, ({pR.IdWorkplace},{pR.CodePeriod} ,{pR.CodeReceipt})", e);
            }
        }

        public void ReloadReceiptDB(int pIdWorkPlace,DateTime pBegin,DateTime pEnd)
        {
            ModelMID.Global.IdWorkPlace = pIdWorkPlace;
            ModelMID.Global.PathDB = "d:/MID/db";
            //SharedLib.BL SBL = new();
            using var db = new WDB_SQLite();

            while (pBegin<= pEnd)
            {
                using var ldb = new WDB_SQLite(pBegin);

                var RS=ldb.GetReceipts(pBegin, pBegin.AddDays(1), pIdWorkPlace);
                foreach (var r in RS.Where(el=> el.StateReceipt>=eStateReceipt.Print))
                {
                    var R = ldb.ViewReceipt(r, true);
                    if (R != null)
                    {
                        Pg.SaveReceipt(R);
                    }
                }

                pBegin = pBegin.AddDays(1);
            }
        }

        public bool ReloadReceipt(IdReceipt pIdR)
        {
            var L=Pg.GetReceipt(pIdR);
            if (L != null)
                Pg.SaveReceipt(L.Receipt, L.Id);
            return true;
        }

        class AnsverDruzi<D>
        {
            public bool status { get; set; }
            public D data { get; set; }
            public int code { get; set; }
            public string error { get; set; }
        }

        class AnsverBonus
        {
            public decimal SumMoneyBonus { get { return ((decimal)bonus_sum) / 100m; } }
            public int bonus_sum { get; set; }
            public int seif_sum { get; set; }
            public decimal Wallet { get { return ((decimal)seif_sum) / 100m; } }
            public string card_id { get; set; }
            public long CardId { get { if(long.TryParse(card_id, out long res)) return res; return 0; } }
            public DateTime birth_date { get; set; }
            public string is_treated { get; set; }
        }
    }

    public class Printers
    {
        public int Warehouse { get; set; }
        public string Printer { get; set; }
    }    
}
