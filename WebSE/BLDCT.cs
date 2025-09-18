using BRB5;
using BRB5.Model;
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
                     new TypeDoc() {Group = eGroup.Price,  CodeDoc = 13, NameDoc = "Перевірка Акцій", KindDoc = eKindDoc.PlanCheck },
                     new TypeDoc() { Group= eGroup.Price, CodeDoc = 14, NameDoc = "Знижки -%50%", KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 11, NameDoc = "Опитування", KindDoc = eKindDoc.RaitingDoc, DayBefore = 4 },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = -1, NameDoc = "Шаблони Опитування", KindDoc = eKindDoc.RaitingTempate },
                     new TypeDoc() { Group= eGroup.Raiting, CodeDoc = 12, NameDoc = "Керування Опитуваннями", KindDoc = eKindDoc.RaitingTemplateCreate },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  1, NameDoc = "Ревізія", TypeControlQuantity = eTypeControlDoc.Ask, IsSaveOnlyScan=false, KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  2, NameDoc = "Прихід", TypeControlQuantity = eTypeControlDoc.Ask, IsViewOut=true, KindDoc = eKindDoc.Normal,IsViewReason=true },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  3, NameDoc = "Переміщення Вих", TypeControlQuantity=eTypeControlDoc.NoControl, KindDoc = eKindDoc.Normal },
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  4, NameDoc = "Списання" ,  KindDoc = eKindDoc.Normal},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  5, NameDoc = "Повернення" ,  KindDoc = eKindDoc.Normal},
                     new TypeDoc() { Group= eGroup.Doc, CodeDoc =  8, NameDoc = "Переміщення Вх", TypeControlQuantity=eTypeControlDoc.Ask, IsViewOut=true, IsViewReason=true,KindDoc = eKindDoc.Normal },];
            //eGroup.FixedAssets => new TypeDoc() { Group= eGroup.FixedAssets, CodeDoc =  7, NameDoc = "Ревізія ОЗ", TypeControlQuantity=eTypeControlDoc.Ask, IsSimpleDoc=true, KindDoc = eKindDoc.Normal,CodeApi=1,IsCreateNewDoc=true },              

        }

        public Result<AnswerLogin> Login(UserBRB pU)
        {
            try
            {
                var r=msSQL.Login(pU);
                if(r == null) return new Result<AnswerLogin>(-1,"Невірний логін чи пароль") ;
                r.TypeDoc = GetTypeDoc();
                r.CustomerBarCode = msSQL.GetCustomerBarCode();
                return new Result<AnswerLogin>() { Info=r};
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

        public Result<BRB5.Model.Guid> GetGuid(int pCodeWarehouse)
        {
            //var aa = Model.InttoPref(1224);
            try
            {
                return new Result<BRB5.Model.Guid>() { Info = msSQL.GetGuid(pCodeWarehouse) };
            }
            catch (Exception e)
            {
                return new Result<BRB5.Model.Guid>(e);
            }
        }

        public Result<Docs> LoadDocs(GetDocs pGD)
        {
            if (pGD.TypeDoc == null)
                return new Result<Docs>(-1, "Bad input Data: GetDocs");
            try
            {
                if (pGD.TypeDoc == 2)
                {
                    Oracle oracle = new Oracle(new() { Login = "c", PassWord = "c" });
                    return new Result<Docs>() { Info = oracle.LoadDocs(pGD) };
                }
                else
                    return new Result<Docs>() { Info = msSQL.LoadDocs(pGD) };
            }
            catch (Exception e)
            {
                return new Result<Docs>(e);
            }
        }

        public Result SaveLogPrice(LogPriceSave pD)
        {
            try
            {
                msSQL.SaveLogPrice(pD);
                return new Result();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new Result(e);
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


    }
}
