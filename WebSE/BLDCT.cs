using BRB5;
using BRB5.Model;
using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System.Collections.Generic;
using UtilNetwork;
using Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                r.TypeDoc = GetTypeDoc();
                return new Result<AnswerLogin>() { Info=r};
            }
            catch (Exception e) { return new(e); }
             
        }
    }
}
