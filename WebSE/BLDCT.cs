using BRB5;
using BRB5.Model;
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
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 101, NameDoc = "Цінники" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc = 102, NameDoc = "Документи" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc = 103, NameDoc = "Основні засоби" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 104, NameDoc = "Опитування" , KindDoc = eKindDoc.NotDefined},
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 0, NameDoc = "Прайсчекер" , KindDoc = eKindDoc.PriceCheck},
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 15, NameDoc = "Подвійний сканер" , KindDoc = eKindDoc.PriceCheck},
                     new TypeDoc() { Group = eGroup.Price,  CodeDoc = 13, NameDoc = "Перевірка Акцій", KindDoc = eKindDoc.PlanCheck },
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 14, NameDoc = "Знижки -%50%", KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 11, NameDoc = "Опитування", KindDoc = eKindDoc.RaitingDoc, DayBefore = 4 },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = -1, NameDoc = "Шаблони Опитування", KindDoc = eKindDoc.RaitingTempate },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 12, NameDoc = "Керування Опитуваннями", KindDoc = eKindDoc.RaitingTemplateCreate },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  1, NameDoc = "Ревізія", TypeControlQuantity = eTypeControlDoc.Ask, IsSaveOnlyScan=false, KindDoc = eKindDoc.Normal,IsViewAct=true },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  2, NameDoc = "Прихід", TypeControlQuantity = eTypeControlDoc.Ask, IsViewOut=true, KindDoc = eKindDoc.Normal,IsViewReason=true },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  3, NameDoc = "Переміщення Вих", TypeControlQuantity=eTypeControlDoc.NoControl, KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  4, NameDoc = "Списання" ,  KindDoc = eKindDoc.Normal},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  5, NameDoc = "Повернення" ,  KindDoc = eKindDoc.Normal},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  8, NameDoc = "Переміщення Вх", TypeControlQuantity=eTypeControlDoc.Ask, IsViewOut=true, IsViewReason=true,KindDoc = eKindDoc.Normal },];
            //eGroup.FixedAssets => new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc =  7, NameDoc = "Ревізія ОЗ", TypeControlQuantity=eTypeControlDoc.Ask, IsSimpleDoc=true, KindDoc = eKindDoc.Normal,CodeApi=1,IsCreateNewDoc=true },              

        }

        public List<TypeDoc> PriceChecker = [new TypeDoc { Group = eGroup.Doc, CodeDoc = 51, NameDoc = "Установка цін", KindDoc = eKindDoc.Normal},
            new TypeDoc { Group = eGroup.Doc, CodeDoc = 52, NameDoc = "Друк пакетів", KindDoc = eKindDoc.Normal} ];

        public UtilNetwork.Result<AnswerLogin> Login(UserBRB pU)
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

        public UtilNetwork.Result SaveDocData(SaveDoc pD)
        {
            try
            {
                if (pD.Doc.TypeDoc == 2) //Якщо замовлення то в Oracle
                {
                    var r = pD.Wares.Select(el => new decimal[] { el.OrderDoc, el.CodeWares, el.InputQuantity });
                    var res = new ApiSaveDoc(153, pD.Doc.TypeDoc, pD.Doc.NumberDoc, r);
                    Znp(res, new() { Login = "c", PassWord = "c" });
                }
                else
                {
                    msSQL.SaveDocData(pD);
                }
                return new UtilNetwork.Result();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new UtilNetwork.Result(e);
            }
        }

        public UtilNetwork.Result<BRB5.Model.Guid> GetGuid(int pCodeWarehouse, int pCodeUser)
        {           
            try
            {
                return new UtilNetwork.Result<BRB5.Model.Guid>() { Data = msSQL.GetGuid(pCodeWarehouse, pCodeUser) };
            }
            catch (Exception e)
            {
                return new UtilNetwork.Result<BRB5.Model.Guid>(e);
            }
        }

        public UtilNetwork.Result<Docs> LoadDocs(GetDocs pGD)
        {
            if (pGD.TypeDoc == null)
                return new UtilNetwork.Result<Docs>(-1, "Bad input Data: GetDocs");
            try
            {
                if (pGD.TypeDoc == 2)
                {
                    Oracle oracle = new Oracle(new() { Login = "c", PassWord = "c" });
                    return new UtilNetwork.Result<Docs>() { Data = oracle.LoadDocs(pGD) };
                }
                else
                    return new UtilNetwork.Result<Docs>() { Data = msSQL.LoadDocs(pGD) };
            }
            catch (Exception e)
            {
                return new UtilNetwork.Result<Docs>(e);
            }
        }

        public UtilNetwork.Result SaveLogPrice(LogPriceSave pD)
        {
            try
            {
                msSQL.SaveLogPrice(pD);
                return new UtilNetwork.Result();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new UtilNetwork.Result(e);
            }
        }

        public UtilNetwork.Result<IEnumerable<Doc>> GetPromotion(int pCodeWarehouse)
        {
            try
            {
                var res = msSQL.GetPromotion(pCodeWarehouse);
                return new UtilNetwork.Result<IEnumerable<Doc>>() { Data = res };
            }
            catch (Exception ex) { return new UtilNetwork.Result<IEnumerable<Doc>>(ex); }
        }

        public UtilNetwork.Result<IEnumerable<DocWares>> GetPromotionData(string pNumberDoc)
        {
            try
            {
                var res = msSQL.GetPromotionData(pNumberDoc);
                return new UtilNetwork.Result<IEnumerable<DocWares>>() { Data = res };
            }
            catch (Exception ex) { return new UtilNetwork.Result<IEnumerable<DocWares>>(ex); }
        }

        public async Task<UtilNetwork.Result<IEnumerable<Client>>> GetClientAsync(FindClient pFC)
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
                return new UtilNetwork.Result<IEnumerable<Client>>(ex);
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
