using BRB5;
using BRB5.Model;
using Dapper;
using Microsoft.Extensions.Configuration;
using ModelMID;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Utils;
using WebSE.Mobile;
using static QRCoder.PayloadGenerator.SwissQrCode;
//using System.Transactions;

namespace WebSE
{
    public class MsSQL
    {
        public SqlConnection connection;

        string MsSqlInit;
        public MsSQL()
        {

            MsSqlInit = Startup.Configuration.GetValue<string>("MsSqlInit");
            connection = new SqlConnection(MsSqlInit);

        }

        public int BulkExecuteNonQuery<T>(string pQuery, IEnumerable<T> pData, bool IsRepeatNotBulk = false)
        {
            //using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var el in pData)
                        connection.Execute(pQuery, el);//, transaction);
                                                       // transaction.Commit();
                }
                catch (Exception ex)
                {
                    throw new Exception("BulkExecuteNonQuery =>" + ex.Message, ex);
                }
                return pData.Count();
            }
        }

        public bool Auth(InputPhone pPhone)
        {
            var sql = @"SELECT SUM(nn) AS nn FROM 
(SELECT 1 AS nn FROM dbo.client c WHERE c.MainPhone=@ShortPhone OR c.Phone=@ShortPhone or c.MainPhone=@phone OR c.Phone=@phone
  UNION ALL
SELECT 1 AS nn FROM dbo.bot_client  c WHERE c.Phone=@Phone OR c.Phone=@ShortPhone ) d";
            int r = connection.ExecuteScalar<int>(sql, pPhone);
            return r > 0;
        }

        public bool Register(RegisterUser pUser)
        {
            var sql = @"INSERT INTO DW.dbo.BOT_client (Phone, FirstName,LastName , email, BirthDay, Sex,  NumberOfFamilyMembers, locality, TypeOfEmployment,IdExternal,BarCode) VALUES 
        (@ShortPhone, @first_name,@last_name, @email, @GetBirthday, @Sex, @family, @locality, @type_of_employment,@IdExternal,@BarCode)";
            int r = connection.Execute(sql, pUser);
            return r > 0;
        }

        public IEnumerable<Direction> GetDirection()
        {
            var sql = @"SELECT vcdgw.CODE_GROUP_WARES AS Code, vcdgw.NAME AS Name FROM dbo.V1C_dim_GROUP_WARES vcdgw 
                            WHERE vcdgw.CODE_PARENT_GROUP_WARES IS NULL AND (SUBSTRING(name,3,1)='.' or SUBSTRING(name,1,1)='a' ) AND 
                                  vcdgw.CODE_GROUP_WARES NOT IN (149758,2524,2928,6002,44312,152983,159472,160594,163788,164335,165710)";
            return connection.Query<Direction>(sql);
        }

        public IEnumerable<Wares> GetWares()
        {
            var sql = @"  SELECT Code,Name,CodeDirection FROM 
  (SELECT dn.code as Code,dn.name_full as Name, gw.code_direction  as CodeDirection, ROW_NUMBER() OVER (PARTITION BY code_direction ORDER BY  code ) AS nn
    FROM dbo.V1C_reg_promotion_gal  pg
    JOIN dbo.V1C_reg_obj_cat oc ON  ( pg.doc_RRef=oc.doc_RRRef  AND   oc.obj_cat_RRef=0x80CA000C29F3389511E7704404BCB2CE)
    JOIN dbo.V1C_dim_nomen dn ON pg.nomen_id = dn.nomen_id
    JOIN  dbo.V1C_dim_GROUP_WARES gw ON dn._ParentIDRRef  = gw.IDRRef
    WHERE GETDATE() BETWEEN pg.date_beg AND pg.date_end
    AND pg.subdivision_RRef =  0x80DE000C29F3389511E7F79F3F9549CF) d 
   WHERE nn<20";
            return connection.Query<Wares>(sql);
        }

        public IEnumerable<Locality> GetLocality()
        {
            var sql = @"SELECT * FROM dbo.BOT_Locality";
            return connection.Query<Locality>(sql);
        }

        public IEnumerable<InfoBonus> GetBarCode(InputPhone pPh)
        {
            var sql = @"SELECT c.BarCode as card,dc.NAME_DISCOUNT_CARD as title FROM dbo.client c
  JOIN dbo.V_DISCOUNT_CARD dc ON c.TypeDiscount=dc.CODE_DISCOUNT_CARD
  LEFT JOIN dbo.BOT_Main_card bmc ON c.CodeClient = bmc.CodeClient
  WHERE c.MainPhone=@ShortPhone OR c.Phone=@ShortPhone or c.MainPhone=@phone or c.Phone=@phone
  ORDER BY ISNULL(bmc.CodeClient,0)";
            return connection.Query<InfoBonus>(sql, pPh);
        }
        public bool SetActiveCard(InputCard pCard)
        {
            var sql = @"DELETE FROM dbo.BOT_Main_card WHERE CodeClient IN (
SELECT c.CodeClient FROM dbo.client c  WHERE c.MainPhone=@ShortPhone OR c.Phone=@ShortPhone or c.MainPhone=@phone or c.Phone=@phone);
  INSERT INTO dbo.BOT_Main_card (CodeClient) SELECT c.CodeClient FROM dbo.client c WHERE  c.BarCode=@code;";
            connection.Execute(sql, pCard);
            return connection.Execute(sql, pCard) > 0;
        }

        public cPrice GetPrice(ApiPrice pParam)
        {
            try
            {
                using var con = new SqlConnection(MsSqlInit);
                var Sql = "select dbo.GetPrice(@CodeWarehouse ,@CodeWares,@BarCode,@Article,@TypePriceInfo,@StrWareHouses)";
                var json = con.ExecuteScalar<string>(Sql, pParam);
                var price = JsonConvert.DeserializeObject<cPrice>(json);
                return price;
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, $"MsSQL.GetPrice => {pParam.ToJSON()}", ex);
                throw;
            }

        }

        public int GetIdRaitingTemplate()
        {
            string Sql = "SELECT (NEXT VALUE FOR DW.dbo.GetIdRaitingTemplate )";
            return connection.ExecuteScalar<int>(Sql);
        }
        public int GetNumberDocRaiting()
        {
            string Sql = "SELECT (NEXT VALUE FOR DW.dbo.GetNumberDocRaiting )";
            return connection.ExecuteScalar<int>(Sql);
        }

        public int ReplaceRaitingTemplate(RaitingTemplate pRt)
        {
            string Sql = @"begin tran
   update dbo.RaitingTemplate  with (serializable) set Text=@Text, IsActive= @IsActive 
   where IdTemplate = @IdTemplate
   if @@rowcount = 0
   begin
     INSERT INTO dbo.RaitingTemplate ( IdTemplate, Text, IsActive) VALUES (@IdTemplate, @Text, @IsActive)
   end
commit tran";
            return connection.Execute(Sql, pRt);
        }

        public int DeleteRaitingTemplateItem(RaitingTemplate pRt)
        {
            string Sql = @"delete from dbo.RaitingTemplateItem where IdTemplate = @IdTemplate";
            return connection.Execute(Sql, pRt);
        }

        public int InsertRaitingTemplateItem(IEnumerable<RaitingTemplateItem> pR)
        {
            string Sql = @"INSERT INTO dbo.RaitingTemplateItem (IdTemplate, Id, Parent, IsHead, Text, RatingTemplate, ValueRating, OrderRS) 
          VALUES (@IdTemplate, @Id, @Parent, @IsHead, @Text, @RatingTemplate,@ValueRating, @OrderRS)";
            return BulkExecuteNonQuery<RaitingTemplateItem>(Sql, pR);

        }
        public int ReplaceRaitingDoc(Doc pDoc)
        {
            string Sql = @"begin tran
   update dbo.RaitingDoc  with (serializable) set IdTemplate =@IdTemplate, CodeWarehouse=@CodeWarehouse, DateDoc=@DateDoc,  Description=@Description 
   where NumberDoc = @NumberDoc
   if @@rowcount = 0
   begin
    INSERT INTO  DW.dbo.RaitingDoc (NumberDoc, IdTemplate, CodeWarehouse, DateDoc,  Description) VALUES
(@NumberDoc, @IdTemplate, @CodeWarehouse, @DateDoc,  @Description);
   end
commit tran";
            return connection.Execute(Sql, pDoc);
        }

        public IEnumerable<RaitingTemplate> GetRaitingTemplate()
        {
            string Sql = @"select IdTemplate, Text,IsActive from dbo.RaitingTemplate";
            var res = connection.Query<RaitingTemplate>(Sql);
            var item = connection.Query<RaitingTemplateItem>("select IdTemplate, Id, Parent, IsHead, Text, RatingTemplate, OrderRS,ValueRating from  dbo.RaitingTemplateItem");
            foreach (var r in res)
            {
                r.Item = item.Where(e => Convert.ToInt32(e.IdTemplate) == r.IdTemplate);
            }
            return res;
        }
        public IEnumerable<Doc> GetRaitingDocs()
        {
            string Sql = @" select NumberDoc, IdTemplate, CodeWarehouse, DateDoc,  Description from dbo.RaitingDoc";
            return connection.Query<Doc>(Sql);
        }

        public IEnumerable<Doc> GetPromotion(int pCodeWarehouse)
        {
            string SQL = $@"SELECT  distinct
    sd.code,  CASE WHEN roc.obj_cat_RRef = 0x80CA000C29F3389511E770430448F861 THEN 1
        WHEN  roc.obj_cat_RRef = 0x80CA000C29F3389511E7704404BCB2CE THEN 2 ELSE 0 END  AS ExtInfo
      ,pg._number AS NumberDoc
      ,cast( pg._Fld11661 as VARCHAR(100)) AS Description,
{pCodeWarehouse} as CodeWarehouse,
DATEADD(year,-2000, pg._Date_Time) AS DateDoc
           from [utppsu].[dbo].[_Document374]  pg 
           --JOIN dbo.V1C_doc_promotion_gal pg ON pg.doc_RRef =pr.doc_RRef
            JOIN (SELECT MIN(obj_cat_RRef) AS obj_cat_RRef, roc.doc_RRRef FROM dbo.v1c_reg_obj_cat roc WHERE roc.doc_type_RTRef = 0x00000176 AND roc.obj_cat_RRef IN (0x80CA000C29F3389511E770430448F861,0x80CA000C29F3389511E7704404BCB2CE) group by doc_RRRef ) roc 
                    ON roc.doc_RRRef =pg._IDRRef  
           JOIN dbo.v1c_reg_promotion_gal pr ON pg._IDRRef=pr.doc_RRef       
           JOIN dbo.v1c_dim_subdivision sd ON pr.subdivision_RRef =sd.subdivision_RRef
           JOIN dbo.WAREHOUSES wh ON sd.subdivision_RRef=wh.subdivision_RRef AND wh.Code={pCodeWarehouse}
    WHERE DATEADD(YEAR,2000,GETDATE()) BETWEEN pg._Fld11664 AND pg._Fld11665";
            return connection.Query<Doc>(SQL);
        }

        public IEnumerable<DocWares> GetPromotionData(string pNumberDoc)
        {
            string SQL = $@"SELECT pg._Number AS NumberDoc, try_convert(int,nom.code) CodeWares,pgn._LineNo11667 AS ""order""
       from [utppsu].[dbo].[_Document374]  pg
       JOIN [utppsu].[dbo].[_Document374_VT11666]  pgn ON pg._IDRRef=pgn._Document374_IDRRef
       JOIN dbo.v1c_dim_nomen nom ON nom.IDRRef = pgn._Fld11668RRef
       WHERE pg._Number={pNumberDoc} AND Year(pg._Date_Time) = year(getdate())+2000";
            return connection.Query<DocWares>(SQL);
        }
        public IEnumerable<Client> GetClient(string parBarCode = null, string parPhone = null, string parName = null, int parCodeClient = 0)
        {
            string SQL = @"with t as 
(
select p.Codeclient from ClientData p where ( p.Data = @Phone and TypeData=2)
union 
select codeclient from client p where codeclient = @CodeClient
union 
 select CodeClient from clientData p where ( p.Data = @BarCode and TypeData=1) 
)

select p.codeclient as CodeClient, p.nameclient as NameClient, 0 as TypeDiscount, td.Name as NameDiscount, p.PersentDiscount as PersentDiscount, 0 as CodeDealer, 
	   0.00 as SumMoneyBonus, 0.00 as SumBonus,1 IsUseBonusFromRest, 1 IsUseBonusToRest,1 as IsUseBonusFromRest,
     (select dbo.Concatenate(ph.data) from ClientData ph where  p.CodeClient = ph.CodeClient and TypeData=1)   as BarCode,
     (select dbo.Concatenate(ph.data) from ClientData ph where  p.CodeClient = ph.CodeClient and TypeData=2) as MainPhone,
       BIRTHDAY as BirthDay, StatusCard as StatusCard 
   from t
     join dbo.client p on (t.CodeClient=p.codeclient)
   left join dbo.V1C_DIM_TYPE_DISCOUNT td on td.TYPE_DISCOUNT=p.TypeDiscount";
            var Res = connection.Query<Client>(SQL, new { CodeClient = parCodeClient, Phone = parPhone, BarCode = parBarCode, Name = (parName == null ? null : "%" + parName + "%") });
            return Res;
        }

        public bool SaveDocData(ApiSaveDoc pD)
        {
            try
            {
                foreach (var el in pD.Wares)
                {
                    var El = new BRB5.Model.DocWares { TypeDoc = pD.TypeDoc, NumberDoc = pD.NumberDoc, OrderDoc = (int)el[0], CodeWares = (int)el[1], Quantity = el[2], CodeReason = el.Length > 3 ? (int)el[3] : 0 };
                    connection.Execute("delete from dbo.Doc_1C  where type_doc = @TypeDoc and number_doc = @NumberDoc and order_doc = @OrderDoc", El);
                    connection.Execute("insert into dbo.Doc_1C (type_doc, number_doc,order_doc,code_wares,quantity,Code_Reason) values (@TypeDoc, @NumberDoc, @OrderDoc, @CodeWares, @Quantity, @CodeReason)", El);
                }
                return true;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return false;
            }
        }

        public IEnumerable<IdReceipt> GetReceiptNo1C(int pCodePeriod)
        {
            string SQL = $@"SELECT  RR.* FROM OPENQUERY([CHECKSRV_DW] ,
 'select DISTINCT RW.""CodePeriod"", RW.""IdWorkplacePay"", RW.""CodeReceipt"", max(RW.""IdWorkplace"") as ""IdWorkplace""
	from public.""ReceiptWares""  RW where ""CodePeriod""={pCodePeriod} and ""CodeWares""<>163516  group by RW.""CodePeriod"", RW.""IdWorkplacePay"", RW.""CodeReceipt""'  
  ) AS RR 

LEFT JOIN (SELECT DISTINCT CodePeriod, IdWorkplace, CodeReceipt FROM dbo.V1C_doc_receipt  r WHERE 
        r._Date_Time>= CONVERT(date, convert(char,{pCodePeriod}+20000000) ,112)  AND 
         r._Date_Time< CONVERT(date, convert(char,{pCodePeriod}+20000001) ,112) 
) R ON RR.CodePeriod=R.CodePeriod AND RR.IdWorkplacePay=R.IdWorkplace AND RR.CodeReceipt=R.CodeReceipt
WHERE R.CodePeriod IS null";

            return connection.Query<IdReceipt>(SQL);
        }

        public IEnumerable<CardMobile> GetClientMobile(InputParMobile pI)
        {
            string SQL = $@"DECLARE @Beg BIGINT; 
DECLARE @End BIGINT; 

SELECT @Beg=min(TRY_CONVERT(int,SUBSTRING(l.[desc],15,7))),@End=max(TRY_CONVERT(int,SUBSTRING(l.[desc],15,7)))  
  FROM log l WHERE SUBSTRING(l.[desc],1,14)='Start SendNo=>' AND l.date_time BETWEEN @from AND @to;

SELECT c.CodeClient AS reference,c.BarCode AS code,c.BarCode AS code1,
CASE WHEN len(c.BarCode)=13 THEN 'EAN13' ELSE 'Code128' END AS type_code ,
'Штриховая' AS card_kind, 'Дисконтная' card_type, 
--CASE WHEN c.StatusCard=1 THEN 'Заблокована' WHEN c.StatusCard=2 THEN 'Загублена' ELSE 'Активна' END 
c.StatusCard AS status,
0 AS code_release,
c.NameClient AS owner_name,
c.CodeOwner AS person_code,
(SELECT DW.dbo.Concatenate(Data+',') FROM ClientData cd  WHERE cd.TypeData=2 AND  cd.CodeClient=c.CodeClient ) AS phone,
(SELECT DW.dbo.Concatenate(Data+',') FROM ClientData cd  WHERE cd.TypeData=3 AND  cd.CodeClient=c.CodeClient ) AS email,
c.BirthDay AS birthday,
'' AS address,
c.FamilyMembers AS	family_members,
c.sex AS gender,
'2000-01-01' registration_date,
c.TypeDiscount  card_type_id,
td.Name card_type_name,
c.CodeSettlement AS card_city_id,
s.Name AS card_city_name,
0 shop_id,
c.CodeTM campaign_id
FROM client c
LEFT JOIN V1C_DIM_TYPE_DISCOUNT td ON td.TYPE_DISCOUNT = TypeDiscount
LEFT JOIN V1C_DIM_Settlement s ON s.Code=c.CodeSettlement
WHERE c.MessageNo BETWEEN @Beg AND  @End" + (pI.limit>0? " order BY c.CodeClient OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;" : "");
            return connection.Query<CardMobile>(SQL, pI);
        }

        public IEnumerable<Bonus> GetBonusMobile(DateTime pBegin, DateTime pEnd, int pLimit)
        {
            string SQL = @"SELECT a._RecordKind AS type ,DATEADD(year, -2000, a._Period) AS bonus_date, a._Fld15343 AS bonus_sum, a._LineNo AS row_num, CONVERT(int, a._RecorderTRef) AS reg,
DATEADD(year, -2000,COALESCE(d256._Date_Time,d326._Date_Time,d364._Date_Time,d376._Date_Time,d16469._Date_Time,d16639._Date_Time,d16639._Date_Time,d17299._Date_Time)) AS reg_date,
COALESCE(d256._Number,d326._Number,d364._Number,d376._Number,d16469._Number,d16639._Number,d16639._Number,d17299._Number) AS reg_number,
TRY_CONVERT(int, card._Code) AS reference_card
  FROM UTPPSU.dbo._AccumRg15340 a
  JOIN [utppsu].dbo._Reference67 card ON a._Fld15341RRef = card._IDRRef
  LEFT JOIN  UTPPSU.dbo._Document256     d256 ON _RecorderTRef=0x00000100 AND _RecorderRRef= d256._IDRRef  -- КорректировкаЗаписейРегистров
  left JOIN  UTPPSU.dbo._Document326     d326 ON _RecorderTRef=0x00000146 AND _RecorderRRef= d326._IDRRef  -- РеализацияТоваровУслуг
  left JOIN  UTPPSU.dbo._Document364     d364 ON _RecorderTRef=0x0000016C AND _RecorderRRef= d364._IDRRef  -- ЧекККМ
  LEFT JOIN  UTPPSU.dbo._Document376     d376 ON _RecorderTRef=0x00000178 AND _RecorderRRef= d376._IDRRef  -- ВыдачаПодарочногоСертификата
  LEFT JOIN  UTPPSU.dbo._Document16469 d16469 ON _RecorderTRef=0x00004055 AND _RecorderRRef= d16469._IDRRef  -- ФормированиеБонусовПокупателей
  LEFT JOIN  UTPPSU.dbo._Document16639 d16639 ON _RecorderTRef=0x000040FF AND _RecorderRRef= d16639._IDRRef  -- ДействияСИнформационнымиКартами
  LEFT JOIN  UTPPSU.dbo._Document17299 d17299 ON _RecorderTRef=0x00004393 AND _RecorderRRef= d17299._IDRRef  -- СписаниеБонусовПокупателейПредварительно
  WHERE a._Period BETWEEN @pBegin and @pEnd";
            return connection.Query<Bonus>(SQL, new { pBegin, pEnd });
        }

        public IEnumerable<Funds> GetMoneyMobile(DateTime pBegin, DateTime pEnd, int pLimit)
        {
            string SQL = @"SELECT a._RecordKind AS type ,DATEADD(year, -2000, a._Period) AS _date, a._Fld19013 AS bonus_sum, a._LineNo AS row_num, CONVERT(int, a._RecorderTRef) AS reg,
DATEADD(year, -2000,COALESCE(d256._Date_Time,d326._Date_Time,d364._Date_Time,d376._Date_Time,d16469._Date_Time,d16639._Date_Time,d16639._Date_Time,d17299._Date_Time)) AS reg_date,
COALESCE(d256._Number,d326._Number,d364._Number,d376._Number,d16469._Number,d16639._Number,d16639._Number,d17299._Number) AS reg_number,
TRY_CONVERT(int, card._Code) AS reference_card
  FROM UTPPSU.dbo._AccumRg19011 a
  JOIN [utppsu].dbo._Reference67 card ON a._Fld19012RRef = card._IDRRef
  LEFT JOIN  UTPPSU.dbo._Document256     d256 ON _RecorderTRef=0x00000100 AND _RecorderRRef= d256._IDRRef  -- КорректировкаЗаписейРегистров
  left JOIN  UTPPSU.dbo._Document326     d326 ON _RecorderTRef=0x00000146 AND _RecorderRRef= d326._IDRRef  -- РеализацияТоваровУслуг
  left JOIN  UTPPSU.dbo._Document364     d364 ON _RecorderTRef=0x0000016C AND _RecorderRRef= d364._IDRRef  -- ЧекККМ
  LEFT JOIN  UTPPSU.dbo._Document376     d376 ON _RecorderTRef=0x00000178 AND _RecorderRRef= d376._IDRRef  -- ВыдачаПодарочногоСертификата
  LEFT JOIN  UTPPSU.dbo._Document16469 d16469 ON _RecorderTRef=0x00004055 AND _RecorderRRef= d16469._IDRRef  -- ФормированиеБонусовПокупателей
  LEFT JOIN  UTPPSU.dbo._Document16639 d16639 ON _RecorderTRef=0x000040FF AND _RecorderRRef= d16639._IDRRef  -- ДействияСИнформационнымиКартами
  LEFT JOIN  UTPPSU.dbo._Document17299 d17299 ON _RecorderTRef=0x00004393 AND _RecorderRRef= d17299._IDRRef  -- СписаниеБонусовПокупателейПредварительно
  WHERE a._Period BETWEEN @pBegin and @pEnd";
            return connection.Query<Funds>(SQL, new { pBegin, pEnd });
        }

    }
}
