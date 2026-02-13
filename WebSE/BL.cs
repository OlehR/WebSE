//using Newtonsoft.Json;
using BRB5.Model;
using LibApiDCT;
using LibApiDCT.SQL;
using ModelMID;
using ModelMID.DB;
using Npgsql;
using SharedLib;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Timers;
using UtilNetwork;
using Utils;

namespace WebSE
{
    public partial class BL
    {
        public static SortedList<System.Guid, UserExpiring> UserExpiring;
        string UrlDruzi = "http://api.druzi.cards/api/bonus/";
        readonly static object LockCreate = new();
        static BL sBL;
        public static BL GetBL { get { lock (LockCreate) { return sBL ?? new BL(); } } }
        //DataSync Ds;
        //SoapTo1C SoapTo1C;
        //DataSync1C Ds1C;
        WDB_MsSql WDBMsSql;
        MsSQL msSQL;
        //GenLabel GL;
        Postgres Pg;
        ConcurrentQueue<Receipt> iQ;
        int DataSyncTime = 0;
        IEnumerable<WorkPlace> Wp;

        //public SortedList<long, string> PrinterWhite = new ();
        //public SortedList<long, string> PrinterYellow = new ();
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        //IEnumerable<int> IsSend;
        //string ListIdWorkPlace;
        public BL()
        {
            FileLogger.Init(Path.Combine(Directory.GetCurrentDirectory(), "Logs"), 0, eTypeLog.Full);
            if (Global.IsTest)
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version} IsTest=>{Global.IsTest}", eTypeLog.Expanded);

            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version}", eTypeLog.Expanded);
            //!!!TMP Тренба переробити по людськи.
            ModelMID.Global.Settings = new() { CodeWaresWallet = 163516 };
            ModelMID.Global.Server1C = "http://bafsrv.vopak.local/psu_utp/ws/ws1.1cws";
            System.Timers.Timer t;
            try
            {
                GetConfig();
                //Ds = new(null);
                //SoapTo1C = new();
                //GL = new();
                string  MsSqlInit = Startup.Configuration.GetValue<string>("MsSqlInit");
                WDBMsSql = new(MsSqlInit);
                Pg = new();
                msSQL = new();
                Wp = WDBMsSql.GetDimWorkplace();
                ModelMID.Global.BildWorkplace(Wp);
                LibApiDCT.LoadData.Init(Startup.Configuration);
                iQ =new();
                //Ds1C = new(null);
                //IsSend = DW.Where(el => !el.Settings.IsSend1C).Select(el => el.IdWorkplace);
                // ListIdWorkPlace = string.Join(",", IsSend);
                // FileLogger.WriteLogMessage($"IsSend=>({ListIdWorkPlace}) DataSyncTime=>{DataSyncTime}");
                if (DataSyncTime > 0)
                {
                    t = new System.Timers.Timer(DataSyncTime);
                    t.AutoReset = true;
                    t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                    t.Start();
                    Task.Run(() => OnTimedEvent());
                }
                Task.Run(() => SaveReceiptQueuePGAsync());
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            sBL = this;
        }
        
        static bool IsLock = false;
        public System.Guid GetUserGuid(int pCodeUser) 
        {
            if (UserExpiring == null) UserExpiring = [];
            lock (UserExpiring)
            {
                var r = UserExpiring.FirstOrDefault(x => x.Value.CodeUser == pCodeUser);
                if (r.Value != null)
                {
                    r.Value.DateExpiring = DateTime.Now.AddMinutes(24*60);
                    return r.Key;
                }
                else
                {
                    System.Guid g = System.Guid.NewGuid();
                    UserExpiring.Add(g, new UserExpiring() { CodeUser = pCodeUser, DateExpiring = DateTime.Now.AddMinutes(24*60) });
                    return g;
                }
            }
        }

        public UserExpiring GetUserExpiring(System.Guid pG)
        {
            if (UserExpiring?.ContainsKey(pG) == true)
                return UserExpiring[pG];
            return null;
        }

        async void OnTimedEvent(Object source = null, ElapsedEventArgs e = null)
        {
            if (IsLock)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Lock OnTimedEvent", eTypeLog.Error);
                return;
            }

            try
            {
                IsLock = true;
                Process proc = Process.GetCurrentProcess();

                IEnumerable<LogInput> R = Pg.GetNeedSend(eTypeSend.Send1C, 200);
                FileLogger.WriteLogMessage(this, "WebSE.BL.OnTimedEvent", $"PrivateMemorySize64=>{proc.PrivateMemorySize64} GC=>{GC.GetTotalMemory(false)} Receipt=>{R?.Count()}");
                if (R?.Any() == true)
                    foreach (var el in R) 
                        await SendReceipt1CAsync(el.Receipt, el.Id, 10);
                        
                R = Pg.GetNeedSend(eTypeSend.SendSparUkraine);
                if (R?.Any() == true)
                    foreach (var el in R)
                    {
                        if (el.Receipt.CodeClient < 0)
                        {
                            Thread.Sleep(10);
                            await SendSparUkraineAsync(el.Receipt, el.Id);
                        }
                        else
                            Pg.ReceiptSetSend(el.Id, eTypeSend.SendSparUkraine);
                    }
                await SendAllBukovelAsync();
                Pg.DelNotUse();

            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
            finally
            {
                IsLock = false;
            }
        }

        async Task SaveReceiptQueuePGAsync()
        {
            var con = Pg.GetConnect();
            do
            {
                Receipt R = null ;
                try
                {
                    
                    while (iQ.TryDequeue(out R))
                    {
                        Pg.SaveReceiptSync(R, R.Id, con);
                        await Task.Delay(5);
                    }
                }
                catch(Exception e) 
                {
                    try
                    {
                        con?.Close();
                        con?.Dispose();
                        con = null;
                        await Task.Delay(500);
                        if(R!=null) iQ.Enqueue(R);
                        con = Pg.GetConnect();
                    }
                    catch { }
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
                await Task.Delay(50);
            }
            while (true);
        }
        public string GenCountNeedSend()
        {
            IEnumerable<LogInput> R1C = Pg.GetNeedSend();
            IEnumerable<LogInput> RSU = Pg.GetNeedSend(eTypeSend.SendSparUkraine);
            return $"1C=>{R1C.Count()} SparUkraine=>{RSU.Count()}";
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
        public string SetAddBrandToGS(AddBrandToGS pBGS)
        {
            dynamic a = pBGS.GetAddBrandToGSAnsver();
            string Res;
            login l;
            (string, login) tt = Znp(a, pBGS.GetLogin());
            (Res, l) = tt;
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
                return new Result<WaresPrice>() { Data = r };
            }
            catch (Exception e) { return new Result<WaresPrice>(e); }
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

        public (string, login) Znp(dynamic pStr, login pL = null)
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
                if ((string.IsNullOrEmpty(l.Login) || string.IsNullOrEmpty(l.PassWord)) && pL != null && !string.IsNullOrEmpty(pL.Login) && !string.IsNullOrEmpty(pL.PassWord))
                    l = pL;

                if (!string.IsNullOrEmpty(l.Login) && !string.IsNullOrEmpty(l.PassWord))
                    return (ExecuteApi(pStr, l), l);
                else
                    return ("{\"State\": -1,\"Procedure\": \"C#\\Api\",\"TextError\":\"Відсутній Логін\\Пароль\"}", l);
            }
            catch (Exception e)
            {
                return ($"{{\"State\": -1,\"Procedure\": \"C#\\Api\",\"TextError\":\"{e.Message}\"}}", null);
            }
        }

        public Result<string> FindByPhoneNumber(InputPhone pUser)
        {
            if (IsLimit())
                return new Result<string>(-1, $"Перевищено денний Ліміт=>{Count}");

            var body = SoapTo1C.GenBody("FindByPhoneNumber", new Parameters[] { new Parameters("NumDocum", "j" + pUser.phone) });
            var res = SoapTo1C.RequestAsync(Global.Server1C, body, 100000, "text/xml", "Администратор:0000").Result; // @"http://1csrv.vopak.local/TEST2_UTPPSU/ws/ws1.1cws"
            FileLogger.WriteLogMessage($"FindByPhoneNumber Phone=>{pUser.ShortPhone} State=> {res.State} TextState =>{res.TextState} Data=>{res.Data}");
            return res;
        }

        public StatusIsBonus CreateCustomerCard(Contact pContact)
        {
            if (IsLimit())
                return new StatusIsBonus(-1, $"Перевищено денний Ліміт=>{Count}");

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(pContact);
            string s = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
            var body = SoapTo1C.GenBody("CreateCustomerCard", new Parameters[] { new Parameters("JSONSting", s) });
            var res = SoapTo1C.RequestAsync(Global.Server1C, body, 100000, "text/xml", "Администратор:0000").Result;
            StatusIsBonus Res =new(res);
            FileLogger.WriteLogMessage($"CreateCustomerCard Contact=>{json} Res={Res.ToJson()} Data=>{Res.Data} is_bonus=>{Res.is_bonus}");
            return Res;
        }
       
        public Result<login> Login(login l)
        {
            //Result<login> res;

            if (!string.IsNullOrEmpty(l.BarCodeUser))
            {
                l = GetLoginByBarCode(l.BarCodeUser);
            }

            if (!string.IsNullOrEmpty(l?.Login) && !string.IsNullOrEmpty(l?.PassWord))
                return new Result<login>() { Data = l };
            else
                return new Result<login>() { State = -1, TextError = "Відсутній Логін\\Пароль" };
        }

        public void GetConfig()
        {
            DataSyncTime = Startup.Configuration.GetValue<int>("ReceiptServer:DataSyncTime");
            /*var Printer = new List<Printers>();
            Startup.Configuration.GetSection("PrintServer:PrinterWhite").Bind(Printer);
            foreach (var el in Printer)
                if (!PrinterWhite.ContainsKey(el.Warehouse))
                    PrinterWhite.Add(el.Warehouse, el.Printer);

            Printer.Clear();
            Startup.Configuration.GetSection("PrintServer:PrinterYellow").Bind(Printer);
            foreach (var el in Printer)
                if (!PrinterYellow.ContainsKey(el.Warehouse))
                    PrinterYellow.Add(el.Warehouse, el.Printer);*/
        }

        public string Print(WaresGL pWares)
        {
            string Res = "";
            try
            {
                if (pWares == null)
                    return "Bad input Data: Wares";
                Console.WriteLine(pWares.CodeWares);

                if (pWares.CodeWarehouse == 0)
                    return "Bad input Data:CodeWarehouse";

                string PrefixDNS =msSQL.GetPrefixDNS(pWares.CodeWarehouse);
                string NamePrinter =  PrefixDNS + Startup.Configuration.GetValue<string>("PrintServer:PrinterWhiteSuffix"); //"BTP-R580II(U)";//!!!TMP
                string NamePrinterYelow = PrefixDNS + Startup.Configuration.GetValue<string>("PrintServer:PrinterYellowSuffix"); //"BTP-R580II(U)"; //!!!TMP 

                if (string.IsNullOrEmpty(NamePrinter))
                    return $"Відсутній принтер: NamePrinter_{pWares.CodeWarehouse}";

                GenLabel GL =LibApiDCT.Global.Company==eCompany.Olive ? new GenLabelOlive() : new GenLabelPSU();
                IEnumerable<cPrice> ListWares=pWares.Wares?.Select(x => GL.GetPrice(pWares.CodeWarehouse, x));

                //var ListWares = GL.GetCode(pWares.CodeWarehouse, pWares.CodeWares);//"000140296,000055083,000055053"
                if (ListWares?.Count() > 0)
                    Res = GL.Print(ListWares, NamePrinter, !(pWares.CodeWarehouse == 89 || pWares.CodeWarehouse == 9), NamePrinterYelow, false, $"Label_{pWares.NameDCT}_{pWares.Login}");
          
                FileLogger.WriteLogMessage(this, "Print", $"InputData=>{pWares.ToJson()} Print=>{ListWares.Count()}");
                return $"Print=>{ListWares.Count()} {Res}";
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"BL.Print InputData=>{pWares.ToJson()}", ex);
                return "Error=>" + ex.Message;
            }
        }

        public Result SaveReceipt(Receipt pR)
        {
            long Id = Pg.SaveLogReceipt(pR);
            if (Id > 0)
            {
                pR.Id = Id;
                iQ.Enqueue(pR);

                Task.Run(async () =>
                {
                    try
                    {
                        //Pg.SaveReceipt(pR, Id);
                        if (!Global.IsNotSendReceipt1C) await SendReceipt1CAsync(pR, Id);

                        FixExciseStamp(pR);
                        //Якщо кліент SPAR Україна
                        if (pR.CodeClient < 0)
                            await SendSparUkraineAsync(pR, Id);
                        if (IsBukovel(pR.IdWorkplace))
                            await SendBukovelAsync(pR, Id);
                    }
                    catch (Exception e)
                    {
                        FileLogger.WriteLogMessage(this, $"SaveReceipt Id=>{Id} {pR.IdWorkplace} {pR.CodePeriod} {pR.CodeReceipt}", e);
                    }
                });
            }
            return new Result(Id > 0 ? 0 : -1);
        }

        public async Task<bool> SendReceipt1CAsync(Receipt pR, long pId, int pWait = 50)
        {
            bool res = false;
            try
            {
                Thread.Sleep(pWait);
                res = await DataSync1C.SendReceiptTo1CAsync(pR, null);
                //FileLogger.WriteLogMessage(this, "SendReceiptTo1CAsync", $" {pR.IdWorkplace} {pR.CodePeriod} {pR.CodeReceipt} res=>{res}");
                if (res && pId > 0) Pg.ReceiptSetSend(pId);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, "SendReceiptTo1CAsync", e);
            }
            return res;
        }

        public Result<ExciseStamp> CheckExciseStamp(ExciseStamp pES)
        {
            try
            {
                ExciseStamp res = Pg.CheckExciseStamp(pES);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pES.ToJson()} =>{res.ToJson()}");
                return new Result<ExciseStamp>() { Data = res };
            }
            catch (Exception ex) {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + pES?.ToJson(), ex);
                return new Result<ExciseStamp>(ex);
            }
        }

        public Result<OneTime> CheckOneTime(OneTime pES)
        {
            try
            {
                OneTime res = Pg.CheckOneTime(pES);
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"{pES.ToJson()} =>{res.ToJson()}");
                return new Result<OneTime>() { Data = res };
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + pES?.ToJson(), ex);
                return new Result<OneTime>(ex);
            }
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
        
        public async Task<Result<Client>> GetDiscountAsync(FindClient pFC)
        {  
            try
            {
                if (Global.IsNotGetBonus1C) return new Result<Client>(new Client( pFC.Client));
                if (pFC.Client != null) //Якщо наша карточка
                {
                    //msSQL.GetClient(parCodeClient=>)
                    Client r = await DataSync1C.GetBonusAsync(new(pFC.Client), pFC.CodeWarehouse);
                    r.OneTimePromotion = Pg.GetOneTimePromotion(r.CodeClient);

                    //r.ReceiptGift = Pg.ReceiptGift(r.CodeClient);

                    r.IsCheckOnline = true;
                    FileLogger.WriteLogMessage( $"WebSE.BL.GetDiscountAsync ({pFC.ToJson()})=>({r.ToJson()})");
                    return new Result<Client>(r);
                }
                else//Якщо друзі
                {
                    //return new Status<Client>();
                    Result<string> Res = null;

                    string CardCode = null;
                    if (!string.IsNullOrEmpty(pFC.BarCode)) CardCode = pFC.BarCode;
                    else
                    {
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
                                var r = new Client() { CodeClient = -Bonus.data.CardId, NameClient = $"Клієнт SPAR Україна {Bonus.data.CardId}", SumBonus = Bonus.data.bonus_sum, SumMoneyBonus = "0".Equals(Bonus.data.is_treated) ? 0 : Bonus.data.SumMoneyBonus, Wallet = Bonus.data.Wallet, BirthDay = Bonus.data.birth_date };
                                FileLogger.WriteLogMessage($"WebSE.BL.GetDiscountAsync SPAR UA ({pFC.ToJson()})=>({r.ToJson()})");
                                return new Result<Client>(r);                            
                            }
                        }
                    }
                }
                return new Result<Client>();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + $" {pFC.ToJson()}", e);
                return new Result<Client>(e);
            }
        }

        public async Task SendSparUkraineAsync(Receipt pR, long pId)
        {
            try
            {
                int SumBonus = 0, SumWallet = 0;
                SumBonus = -(int)(pR.Payment?.Where(el => el.TypePay == eTypePay.Bonus)?.FirstOrDefault()?.SumPay * 100 ?? 0);
                if (SumBonus == 0)
                    SumBonus = (int)Math.Round(pR.SumReceipt, 0);
                SumWallet = -(int)(pR.Payment?.Where(el => el.TypePay == eTypePay.Wallet)?.FirstOrDefault()?.SumPay * 100 ?? 0);
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
            } catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, $"SendSparUkraine CodeClient={pR.CodeClient}, ({pR.IdWorkplace},{pR.CodePeriod} ,{pR.CodeReceipt})", e);
            }
        }

        public void ReloadReceiptDB(int pIdWorkPlace, DateTime pBegin, DateTime pEnd)
        {
            ModelMID.Global.IdWorkPlace = pIdWorkPlace;
            ModelMID.Global.PathDB = "d:/MID/db";
            //SharedLib.BL SBL = new();
            using var db = new WDB_SQLite();

            while (pBegin <= pEnd)
            {
                using var ldb = new WDB_SQLite(pBegin);

                var RS = ldb.GetReceipts(pBegin, pBegin.AddDays(1), pIdWorkPlace);
                foreach (var r in RS.Where(el => el.StateReceipt >= eStateReceipt.Print))
                {
                    var R = ldb.ViewReceipt(r, true);
                    if (R != null)
                    {
                        Pg.SaveReceiptSync(R);
                    }
                }

                pBegin = pBegin.AddDays(1);
            }
        }

        public bool ReloadReceipt(IdReceipt pIdR)
        {
            var L = Pg.GetReceipt(pIdR);
            if (L != null)
                Pg.SaveReceiptSync(L.Receipt, L.Id);
            return true;
        }

        public async Task<string> SendReceipt1CAsync(IdReceipt pIdR)
        {
            bool Res = false;
            int i = 0;
            StringBuilder Sb = new($"{DateTime.Now} Start{Environment.NewLine}");
            var L = Pg.GetReceipts(pIdR);
            Sb.Append($"{DateTime.Now} Load=>{L.Count()}");

            if (L != null)
                foreach (var el in L)
                    try
                    {
                        i++;
                        Res = await SendReceipt1CAsync(el.Receipt, 0, 20);
                        Sb.Append($"{DateTime.Now} Receipt=>{el.NumberReceipt1C}");
                    }
                    catch (Exception e) { return $" {el.CodeReceipt} {e.Message}"; }
            return $"{Sb}{Environment.NewLine}{Res}";
        }


        public async Task<string> ReloadBadReceipt(int pCodePeriod)
        {
            string Res = null;
            int i = 0;
            var L = Pg.GetBadReceipts(pCodePeriod);
            if (L != null)
                foreach (var el in L)
                    try
                    {
                        i++;
                        Pg.SaveReceiptSync(el.Receipt,el.Id);
                        await Task.Delay(5);
                    }
                    catch (Exception e) { return $" {el.CodeReceipt} {e.Message}"; }
            return $"Чеків=>{i} {Res}";

        }

        public async Task<string> LoadReceiptNo1C(int pCodePeriod)
        {
            try
            {
                var r = msSQL.GetReceiptNo1C(pCodePeriod);
                foreach (var el in r)
                    await SendReceipt1CAsync(el);
                return r.Count().ToString();
            } catch(Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> ReloadReceiptToPG(IdReceipt pIdR)
        {
            StringBuilder r = new();
            int i = 0;
            var L = Pg.GetReceipts(pIdR);
            if (L != null)
                foreach (var el in L)
                    try
                    {
                        i++;
                        r.Append(Pg.SaveReceiptSync(el.Receipt));
                    }
                    catch (Exception e) { return $" {el.CodeReceipt} {e.Message}"; }
            return $"Чеків=>{i}{Environment.NewLine}{r}";
        }

        public async Task<string> ReloadReceiptTo1CQuery(string pSql)         
        {
            StringBuilder r = new();        
            int i = 0;
            try
            {
                foreach (var el in Pg.GetReceiptsQuery(pSql))
                {
                    r.Append(await SendReceipt1CAsync(el.Receipt, el.Id, 10) + $" {el.NumberReceipt1C}{Environment.NewLine}");
                    i++;
                }              
            }
            catch (Exception e)
            {
                return e.Message;
            }  
            return $"Чеків=>{i}{Environment.NewLine}{r.ToString()}";
        }

        public async Task<string> ReloadReceiptToQuery(string pSql)
        {
            StringBuilder r = new();
            NpgsqlConnection con=null;
            int i = 0;
            try
            {
                con = Pg.GetConnect();
                foreach (var el in Pg.GetReceiptsQuery(pSql))
                {
                    i++;
                    r.Append(Pg.SaveReceiptSync(el.Receipt, el.Id,con));
                    Thread.Sleep(5);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {       
                    con?.Close();
                    con?.Dispose();
            }

            return $"Чеків=>{i}{Environment.NewLine}{r.ToString()}";
        }

        public async Task<string> TestAsync()
        {
            int i = 0;
            foreach (var el in Wp)
            {
                var r = Pg.GetReceipts(new IdReceipt { CodePeriod = 20241112, IdWorkplace = el.IdWorkplace });
                foreach (var e in r)
                {
                    if (e.Receipt.IdWorkplacePays.Count() == 1 && e.Receipt.IdWorkplace != e.Receipt.IdWorkplacePays[0])
                    { await SendReceipt1CAsync(e); i++; }
                }
            }
            return i.ToString();
        }
        public async Task<eReturnClient> Send1CClient(ClientNew pC) => await DataSync1C.Send1CClientAsync(pC);

        public async Task<bool> Send1CReceiptWaresDeletedAsync(IEnumerable<ReceiptWaresDeleted1C> pRWD) => await DataSync1C.Send1CReceiptWaresDeletedAsync(pRWD);

        public IEnumerable<ReceiptWares> GetClientOrder(string pNumberOrder)=> msSQL.GetClientOrder(pNumberOrder);

        public Dictionary<string, decimal> GetReceipt1C(IdReceipt pIdR) => msSQL.GetReceipt1C(pIdR);


        public RestSU GetRestSU() => msSQL.GetRestSU();

        public BaseSU GetBaseSU() => msSQL.GetBaseSU();
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
}
