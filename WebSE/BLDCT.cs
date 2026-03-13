using BRB5;
using BRB5.Model;
using Microsoft.AspNetCore.Mvc;
using ModelMID;
using SharedLib;
using System.DirectoryServices.AccountManagement;
using UtilNetwork;
using Utils;

namespace WebSE
{
    public partial class BL
    {
        public IEnumerable<TypeDoc> GetTypeDoc()
        {
            return [
                     new TypeDoc() { Group = eGroup.Price, CodeDoc = 101, NameDoc = "Цінники" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group = eGroup.Doc, CodeDoc = 102, NameDoc = "Документи" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group = eGroup.FixedAssets, CodeDoc = 103, NameDoc = "Основні засоби" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group = eGroup.Raiting, CodeDoc = 104, NameDoc = "Опитування" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group = eGroup.СollectionOfGoods, CodeDoc = 105, NameDoc = "Підбір товару" , KindDoc = eKindDoc.NotDefined},

                     new TypeDoc() { Group = eGroup.Price, CodeDoc = 0, NameDoc = "Прайсчекер" , KindDoc = eKindDoc.PriceCheck},
                     new TypeDoc() { Group = eGroup.Price, CodeDoc = 15, NameDoc = "Подвійний сканер" , KindDoc = eKindDoc.PriceCheck},
                     new TypeDoc() { Group = eGroup.Price, CodeDoc = 13, NameDoc = "Перевірка Акцій", KindDoc = eKindDoc.PlanCheck },
                     new TypeDoc() { Group = eGroup.Price, CodeDoc = 14, NameDoc = "Знижки -%50%", KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group = eGroup.Raiting, CodeDoc = 11, NameDoc = "Опитування", KindDoc = eKindDoc.RaitingDoc, DayBefore = 4 },
                     new TypeDoc() { Group = eGroup.Raiting, CodeDoc = -1, NameDoc = "Шаблони Опитування", KindDoc = eKindDoc.RaitingTempate },
                     new TypeDoc() { Group = eGroup.Raiting, CodeDoc = 12, NameDoc = "Керування Опитуваннями", KindDoc = eKindDoc.RaitingTemplateCreate },
                     new TypeDoc() { Group = eGroup.Doc, CodeDoc = 1, NameDoc = "Ревізія", TypeControlQuantity = eTypeControlDoc.Ask, IsSaveOnlyScan=false, KindDoc = eKindDoc.Normal,IsViewAct=true,IsNotViewPlanF4=true },
                     new TypeDoc() { Group = eGroup.Doc, CodeDoc = 2, NameDoc = "Прихід", TypeControlQuantity = eTypeControlDoc.Ask, IsViewOut=true, KindDoc = eKindDoc.Normal,IsViewReason=true },
                     new TypeDoc() { Group = eGroup.Doc, CodeDoc = 3, NameDoc = "Переміщення Вих", TypeControlQuantity=eTypeControlDoc.NoControl, KindDoc = eKindDoc.Normal,TypeCreateDoc=eTypeCreateDoc.Warehouse },
                     new TypeDoc() { Group = eGroup.Doc, CodeDoc = 4, NameDoc = "Списання" ,  KindDoc = eKindDoc.Normal, TypeCreateDoc=eTypeCreateDoc.Nothing},
                     new TypeDoc() { Group = eGroup.Doc, CodeDoc = 5, NameDoc = "Повернення" ,  KindDoc = eKindDoc.Normal, TypeCreateDoc=eTypeCreateDoc.Nothing},
                     new TypeDoc() { Group = eGroup.Doc, CodeDoc = 8, NameDoc = "Переміщення Вх", TypeControlQuantity=eTypeControlDoc.Ask, IsViewOut=true, IsViewReason=true,KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group = eGroup.СollectionOfGoods, CodeDoc =  21, NameDoc = "Завдання на Підбір",  KindDoc = eKindDoc.NormalNoEdit},
                     new TypeDoc() { Group = eGroup.СollectionOfGoods, CodeDoc =  22, NameDoc = "Підбір",  KindDoc = eKindDoc.Normal},
                     new TypeDoc() { Group = eGroup.СollectionOfGoods, CodeDoc =  23, NameDoc = "Поповнення",  KindDoc = eKindDoc.Normal},
            ];
            //eGroup.FixedAssets => new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc =  7, NameDoc = "Ревізія ОЗ", TypeControlQuantity=eTypeControlDoc.Ask, IsSimpleDoc=true, KindDoc = eKindDoc.Normal,CodeApi=1,IsCreateNewDoc=true },              

        }

        public List<TypeDoc> PriceChecker = [new TypeDoc { Group = eGroup.Doc, CodeDoc = 51, NameDoc = "Установка цін", KindDoc = eKindDoc.Normal},
            new TypeDoc { Group = eGroup.Doc, CodeDoc = 52, NameDoc = "Друк пакетів", KindDoc = eKindDoc.Normal} ];

        public Result<AnswerLogin> Login(UserBRB pU)
        {
            try
            {
                AnswerLogin r;
                //if ("Price".Equals(pU.Login) && "Price".Equals(pU.PassWord))
                //{
                //    r = new(pU){ TypeDoc = PriceChecker};
                //}
                //else
                //{
                    r = msSQL.Login(pU);                    
                    if (r == null) return new(-1, "Невірний логін чи пароль");
                    if(string.IsNullOrEmpty(r.PassWord) && string.IsNullOrEmpty( pU.BarCode))
                    {
                        if(!ValidateWindowsCredentials("vopak", pU.Login, pU.PassWord)) return new(-1, "Невірний логін чи пароль");
                    }
                    else
                        if(string.IsNullOrEmpty(pU.BarCode) && pU.PassWord?.Equals( r.PassWord)!=true) return new(-1, "Невірний логін чи пароль");

                    r.TypeDoc = "Price".Equals(pU.Login) ?  PriceChecker:  GetTypeDoc();
                    r.CustomerBarCode = msSQL.GetCustomerBarCode();
                    r.UserGuid = GetUserGuid(r.CodeUser);
                    r.CodeUnitWeight = 7;
                    r.CodeUnitPiece = 19;
                    r.IsVisOrderF3= true;
                //}
                return new() { Data = r };
            }
            catch (Exception e) { return new(e); }
        }

        public Result SaveDocData(SaveDoc pD)
        {
            try
            {
                if (pD.Doc.TypeDoc == 2) //Якщо замовлення то в Oracle
                {
                    var r = pD.Wares.Select(el => new decimal[] { el.OrderDoc, el.CodeWares, el.InputQuantity });
                    var res = new ApiSaveDoc(153, pD.Doc.TypeDoc, pD.Doc.NumberDoc, r);
                    Znp(res, new() { Login = "c", PassWord = "c" });
                }
                else if(pD.Doc.TypeDoc>=20 && pD.Doc.TypeDoc < 30)
                {
                    if (pD.Doc != null)
                    {
                        Replenishment R = new() { CodeWarehouse = pD.Doc.CodeWarehouse, State = pD.Doc.TypeDoc-20, NumberDoc=pD.Doc.NumberDoc, DCT = pD.NameDCT, CodeUser =pD.CodeUser,
                            Wares = pD.Doc.TypeDoc == 21?[]: pD.Wares?.Where(el=> el.InputQuantity>0).Select(el=> new WaresReplenishment(el))
                        };
                        SaveReplenishment(R);
                    }
                }
                else
                {
                    msSQL.SaveDocData(pD);
                }
                return new Result();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
            }
        }

        public Result<BRB5.Model.Guid> GetGuid(int pCodeWarehouse, int pCodeUser)
        {           
            try
            {
                return new() { Data = msSQL.GetGuid(pCodeWarehouse, pCodeUser) };
            }
            catch (Exception e)
            {
                return new(e);
            }
        }
        public Result<Doc> CreateNewDoc(CreateDocData pD)
        {
            try
            {
                string Data = System.Text.Json.JsonSerializer.Serialize(pD);
                var body = SoapTo1C.GenBody("EmptyDoc", [new Parameters("JsonStr", Data.GetBase64())]);
                var res = SoapTo1C.RequestAsync("http://bafsrv/psu_utp/ws/ws2.1cws", body, 10000, "text/xml", "Администратор:0000").Result;
                if (res.Success)
                {
                    Doc D = new() { TypeDoc = pD.TypeDoc, CodeWarehouse = pD.CodeWarehouse, NumberDoc = res.Data, DateDoc=DateTime.Now, Description = pD.Description };
                    return new() { Data = D };
                }
                else
                    return new(res);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + "=>" + pD.ToJson(), e);
                return new(e);
            }
        }
        public Result<Docs> LoadDocs(GetDocs pGD)
        {
            if (pGD == null)
                return new(-1, "Bad input Data: GetDocs");
            try
            {
                if (pGD.TypeDoc == 2)
                {
                    Oracle oracle = new(new() { Login = "c", PassWord = "c" });
                    return new() { Data = oracle.LoadDocs(pGD) };
                }
                else
                    return new() { Data = msSQL.LoadDocs(pGD) };
            }
            catch (Exception e)
            {
                return new(e);
            }
        }
        class Replenishment
        {
            public Replenishment() { }
            public Replenishment(LogPriceSave pLPS)
            {
                CodeWarehouse = pLPS.CodeWarehouse;
                State = 0;
                DCT = pLPS.SerialNumber;
                CodeUser = pLPS.CodeUser;
                Wares = pLPS.LogPrice.Where(el => el.NumberOfReplenishment > 0).Select(el => new WaresReplenishment(el));
            }
            public int CodeWarehouse { get; set; }
            public int State { get; set; }
            public int CodeUser { get; set; }
            public string DCT { get; set; } = "";
            public string  NumberDoc { get; set; } = "";
            public IEnumerable<WaresReplenishment> Wares { get; set; }
        }
        class WaresReplenishment
        {
            public WaresReplenishment(LogPrice pLP)
            {
                CodeWares = pLP.CodeWares;
                Quantity = (decimal)pLP.NumberOfReplenishment;
                ProductArea = pLP.ProductArea ?? "NotDefine";
            }
            public WaresReplenishment(DocWares pW)
            {
                CodeWares = pW.CodeWares;
                Quantity = pW.InputQuantity;
            }
            public long CodeWares { get; set; }
            public decimal Quantity { get; set; }
            public string ProductArea { get; set; }
        }
        public Result SaveLogPrice(LogPriceSave pD)
        {
            try
            {
                msSQL.SaveLogPrice(pD);
                var r = new Replenishment(pD);
                if (r.Wares.Any())
                    SaveReplenishment(r);
                return new();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name +"=>" +pD.ToJson(), e);
                return new(e);
            }
        }

        bool SaveReplenishment(Replenishment pR)
        {
            string Data = System.Text.Json.JsonSerializer.Serialize(pR);
            //System.Text.Json.JsonSerializer.Serialize(s), options);
            var body = SoapTo1C.GenBody("PostCollectingOrder", [new Parameters("InputString", Data)]);
            var res = SoapTo1C.RequestAsync("http://bafsrv/psu_utp/ws/ws2.1cws", body, 10000, "text/xml", "Администратор:0000").Result; // @"http://1csrv.vopak.local/TEST2_UTPPSU/ws/ws1.1cws"

            FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Data={Data} Result=>{res.ToJson()}");
            return true;
        }

        public Result<IEnumerable<Doc>> GetPromotion(int pCodeWarehouse)
        {
            try
            {
                var res = msSQL.GetPromotion(pCodeWarehouse);
                return new() { Data = res };
            }
            catch (Exception ex) { return new(ex); }
        }

        public Result<IEnumerable<DocWares>> GetPromotionData(string pNumberDoc)
        {
            try
            {
                var res = msSQL.GetPromotionData(pNumberDoc);
                return new() { Data = res };
            }
            catch (Exception ex) { return new(ex); }
        }

        public async Task<Result<IEnumerable<Client>>> GetClientAsync(FindClient pFC)
        {
            try
            {
                var r = msSQL.GetClient(pFC);
                if (r?.Count() > 0)
                {
                    pFC.Client = r.FirstOrDefault();
                    _ = await DataSync1C.GetBonusAsync(new(pFC.Client), pFC.CodeWarehouse);
                }
                return new() { Data = r };
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }

        public static bool ValidateWindowsCredentials(string domainName, string username, string password)
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
}
