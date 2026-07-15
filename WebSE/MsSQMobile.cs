using BRB5.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Utils;
using WebSE.Mobile;

namespace WebSE
{
    public partial class MsSQL
    {
        public IEnumerable<CardMobile> GetClientMobile(InputParCardsMobile pI)
        {
            string SQL = $@"DECLARE @Beg BIGINT; DECLARE @End BIGINT; 
SELECT @Beg=min(SendNo),@End=max(SendNo) FROM log l WHERE SendNo>0 AND l.date_time BETWEEN @from AND @to;

WITH l AS ( SELECT SendNo,max(date_time) AS date_time FROM log where SendNo BETWEEN  @Beg AND @End group by SendNo )
SELECT c.CodeClient AS reference,c.BarCode AS code,c.BarCode AS code1,
CASE WHEN len(c.BarCode)=13 THEN 'EAN13' ELSE 'Code128' END AS type_code ,
'Штриховая' AS card_kind, 'Дисконтная' card_type, 
--CASE WHEN c.StatusCard=1 THEN 'Заблокована' WHEN c.StatusCard=2 THEN 'Загублена' ELSE 'Активна' END 
c.StatusCard AS status,
c.CodeOut AS code_release,
c.NameClient AS owner_name,
CASE WHEN c.CodeOwner <100000000 THEN '0'+ FORMAT(c.CodeOwner,'D8') ELSE 'Б'+FORMAT(c.CodeOwner-100000000,'D8') end   AS person_code,
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
c.codeShop shop_id,
c.CodeTM campaign_id
,l.date_time AS send_at
FROM client c
LEFT JOIN V1C_DIM_TYPE_DISCOUNT td ON td.TYPE_DISCOUNT = TypeDiscount
LEFT JOIN V1C_DIM_Settlement s ON s.Code=c.CodeSettlement
LEFT JOIN l ON SendNo= c.MessageNo
WHERE " + (!string.IsNullOrEmpty(pI.code) || pI.reference_card > 0 ? (pI.reference_card > 0 ? "c.CodeClient=@reference_card" : "c.BarCode=@code") :
                "c.MessageNo BETWEEN @Beg AND @End" + (pI.campaign_id > 0 ? " and c.CodeTM = @campaign_id" : "")) +
  (pI.limit > 0 ? " order BY c.CodeClient OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY;" : "");
            return Query<CardMobile>(SQL, pI);
        }

        public IEnumerable<Bonus> GetBonusMobile(DateTime pBegin, DateTime pEnd, Int64 pReferenceCard = 0, int pLimit = 0, int pOffset = 0)
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
  WHERE a._Period BETWEEN @pBegin and @pEnd and TRY_CONVERT(int, card._Code) = case when @pReferenceCard>0 then @pReferenceCard else TRY_CONVERT(int, card._Code) end" +
(pLimit > 0 ? $" order BY a._Period OFFSET {pOffset} ROWS FETCH NEXT {pLimit} ROWS ONLY;" : "");
            return Query<Bonus>(SQL, new { pBegin, pEnd, pReferenceCard });
        }

        public IEnumerable<Funds> GetMoneyMobile(DateTime pBegin, DateTime pEnd, Int64 pReferenceCard = 0, int pLimit = 0, int pOffset = 0)
        {
            string SQL = @"SELECT a._RecordKind AS type ,DATEADD(year, -2000, a._Period) AS funds_date, a._Fld19013 AS funds_sum, a._LineNo AS row_num, CONVERT(int, a._RecorderTRef) AS reg,
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
  WHERE a._Period BETWEEN @pBegin and @pEnd and TRY_CONVERT(int, card._Code) = case when @pReferenceCard>0 then @pReferenceCard else TRY_CONVERT(int, card._Code) end" +
(pLimit > 0 ? $" order BY a._Period OFFSET {pOffset} ROWS FETCH NEXT {pLimit} ROWS ONLY;" : "");
            return Query<Funds>(SQL, new { pBegin, pEnd, pReferenceCard });
        }

        public ResultFixGuideMobile GetFixGuideMobile()
        {
            try
            {
                using var con = new SqlConnection(MsSqlInit);
                con.Open();
                ResultFixGuideMobile res = new ResultFixGuideMobile();
                //Тип номенклатури (товар, тара)            
                res.TypeWares = con.Query<GuideMobile>("SELECT TRY_CONVERT(int, _Code) AS code,_Description AS name  FROM  [utppsu].dbo._Reference40");
                res.Unit = con.Query<GuideMobile>("SELECT ud.code_unit AS code,ud.name_unit AS name  FROM UNIT_DIMENSION ud");
                res.TM = con.Query<GuideMobile>("SELECT tm.CodeTM as code, tm.NameTM AS name FROM  TRADE_MARKS tm");
                res.Brand = con.Query<GuideMobile>("SELECT b.code_brand AS code, b.name_brand as name FROM  BRAND b");
                res.TypePrice = con.Query<GuideMobile>("SELECT vcdtp.code, vcdtp.[desc] as name FROM V1C_dim_type_price vcdtp");
                res.TypeBarCode = con.Query<GuideMobile>("SELECT vctbc.Code, vctbc.name FROM V1C_TypeBarCode vctbc");
                res.Warehouse = con.Query<GuideMobile>("SELECT  wh.Code AS Code, wh.Name AS name, try_convert(int,wh.Code_Shop) AS parent FROM dbo.WAREHOUSES wh WHERE wh.type_warehouse=11");
                res.Campaign = con.Query<GuideMobile>("SELECT code,name FROM V1C_DIM_TM_SHOP");
                res.CashDesk = con.Query<GuideMobile>("SELECT cd.code AS code,cd.[desc] AS name,cd.CodeWarehouse AS parent  FROM DW.dbo.V1C_CashDesk cd");
                con.Close();
                return res;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new ResultFixGuideMobile(e.Message);
            }
        }

        public ResultGuideMobile GetGuideMobile(InputParWaresMobile pIP)
        {
            try
            {
                ResultGuideMobile res = new ResultGuideMobile();
                using var con = new SqlConnection(MsSqlInit);
                con.Open();
                string BeginEnd = pIP.code>0?"": @"DECLARE @Beg BIGINT; 
DECLARE @End BIGINT;
SELECT @Beg=min(TRY_CONVERT(int,SUBSTRING(l.[desc],15,7))),@End=max(TRY_CONVERT(int,SUBSTRING(l.[desc],15,7)))  
  FROM log l WHERE SUBSTRING(l.[desc],1,14)='Start SendNo=>' AND l.date_time BETWEEN @from AND @to;";
                string FilterLimit = pIP.code > 0 ? "Code_wares=@code;" : $"MessageNo BETWEEN @Beg AND @End" + (pIP.limit > 0 ? " order BY w.code_wares OFFSET @offset ROWS FETCH NEXT @limit ROWS ONLY; " : "");
                string SQL = BeginEnd + $@"
SELECT w.code_wares AS reference,w.articl AS  vendor_code,w.name_wares AS name, --w.name_wares AS title, w.name_wares AS print_title,
w.code_group AS parent_code,
CASE WHEN w.type_wares=0 THEN 0 ELSE 0 end AS   is_excise,
case WHEN w.code_unit=7 THEN 1 ELSE 0 END AS is_weight,
w.VAT AS tax,
w.Code_TypeOfWaresh as type_code, --Код виду номенклатури
w.code_unit AS unit_code,
w.code_brand AS brand_code,
w.code_tm AS trademark_code, 
w.is_old AS is_old 
--w. Name_TypeOfWares
FROM Wares w
WHERE w." + FilterLimit;
                res.products = con.Query<WaresMobile>(SQL, pIP);

                SQL = BeginEnd + $@"
SELECT b.code_wares AS code_products, b.TypeBarCode AS type_code, b.bar_code AS code, w.is_old 
FROM barcode b 
JOIN Wares w ON b.code_wares = w.code_wares
WHERE b." + FilterLimit;
                res.BarCode = con.Query<BarCodeMobile>(SQL, pIP);

                SQL = BeginEnd + $@"
SELECT p.code_wares AS code_products, p.CODE_DEALER  AS price_type_code, p.price AS price,p.date_change AS price_date, w.is_old 
FROM dbo.price p 
JOIN Wares w ON w.code_wares = p.code_wares 
WHERE p." + FilterLimit;
                res.Price = con.Query<PriceMobile>(SQL, pIP);
                con.Close();
                return res;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + pIP.ToJson(), e);
                return new ResultGuideMobile(e.Message);
            }
        }

        public ResultPromotionMobile<ProductsPromotionMobile> GetPromotionMobile()
        {
            try
            {
                ResultPromotionMobile<ProductsPromotionMobile> res = new();
                using var con = new SqlConnection(MsSqlInit);
                con.Open();
                //Тип номенклатури (товар, тара)            
                string SQL = $@"SELECT DISTINCT CONVERT(INT, YEAR(dpg.date_time)*100000+dpg.number) AS number,dpg.date_beg ,dpg.date_end,dpg.comment  
        FROM dbo.V1C_doc_promotion_gal  dpg
        WHERE  dpg. date_end>GETDATE()";
                res.Promotions = con.Query<PromotionMobile<ProductsPromotionMobile>>(SQL);
                SQL = @"SELECT DISTINCT CONVERT(INT, YEAR(dpg.date_time)*100000+dpg.number) AS number, CONVERT(INT, dn.code) AS products, CONVERT(INT, tp.code) AS type_price
    , isnull(pp.Priority+1, 0) AS priority, pg.MaxQuantity as max_priority, dpgn.Price
  FROM dbo.V1C_reg_promotion_gal pg
  JOIN dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef
  JOIN dbo.V1C_docit_promotion_gal_nom dpgn ON dpgn.doc_RRef = pg.doc_RRef  AND  pg.nomen_RRef=dpgn.nomen_RRef AND dpgn.[line_no]=pg.[line_no]
  JOIN dbo.V1C_dim_nomen dn ON pg.nomen_RRef= dn.IDRRef
  JOIN dbo.V1C_dim_type_price tp ON pg.price_type_RRef= tp.type_price_RRef
  --JOIN dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef= pg.subdivision_RRef
  LEFT JOIN dbo.V1C_DIM_Priority_Promotion PP ON tp.Priority_Promotion_RRef= pp.Priority_Promotion_RRef
  where pg.date_end>GETDATE()";
                var pp = con.Query<ProductsPromotionMobile>(SQL);

                SQL = @"SELECT DISTINCT CONVERT(INT, YEAR(dpg.date_time)*100000+dpg.number) AS number,TRY_CONVERT(int, wh.code) warehouse
  FROM dbo.V1C_reg_promotion_gal pg   
  JOIN dbo.V1C_doc_promotion_gal dpg ON pg.doc_RRef = dpg.doc_RRef  
  JOIN dbo.V1C_dim_warehouse wh ON wh.subdivision_RRef= pg.subdivision_RRef
  where pg.date_end>GETDATE()";
                var wh = con.Query<PromotionWarehouseMobile>(SQL);
                foreach (var el in res.Promotions)
                {
                    el.products = pp.Where(x => x.number == el.number);
                    el.warehouses = wh.Where(x => x.number == el.number).Select(a => a.warehouse);
                }

                return res;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new ResultPromotionMobile<ProductsPromotionMobile>(e.Message);
            }

        }

        public ResultPromotionMobile<ProductsKitMobile> GetPromotionKitMobile()
        {
            try
            {
                ResultPromotionMobile<ProductsKitMobile> res = new();
                using var con = new SqlConnection(MsSqlInit);
                con.Open();
                //Тип номенклатури (товар, тара)            
                string SQL = $@"SELECT  CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number)  AS number, CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number)  AS  reference,dp.d_begin as date_beg ,dp.d_end as date_end, dp.comment, dp.number_ex_value 
        FROM dbo.V1C_doc_promotion  dp
        WHERE  dp. d_end>GETDATE() AND dp.kind_promotion=0x94BE56137942F05C49313B91A28B535D";
                res.Promotions = con.Query<PromotionMobile<ProductsKitMobile>>(SQL);
                SQL = @"SELECT DISTINCT CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS number, CONVERT(INT, dn.code) AS reference,pk.price
  FROM dbo.V1C_doc_promotion dp 
  JOIN dbo.V1C_doc_promotion_kit pk ON dp._IDRRef=pk.doc_promotion_RRef
  JOIN dbo.V1C_dim_nomen dn ON dn.IDRRef= pk.nomen_RRef  
  WHERE  dp. d_end>GETDATE() AND dp.kind_promotion=0x94BE56137942F05C49313B91A28B535D";
                var pp = con.Query<ProductsKitMobile>(SQL);

                SQL = @"SELECT  CONVERT(INT, YEAR(dp.year_doc)*10000+dp.number) AS number,TRY_CONVERT(int, wh.code) warehouse
  FROM dbo.V1C_doc_promotion dp 
  JOIN dbo.V1C_doc_promotion_warehouse dw ON dp._IDRRef=dw.doc_promotion_RRef
  JOIN dbo.V1C_dim_warehouse wh ON wh.warehouse_RRef=dw.warehouse_RRef
  WHERE  dp. d_end>GETDATE() AND dp.kind_promotion=0x94BE56137942F05C49313B91A28B535D";
                var wh = con.Query<PromotionWarehouseMobile>(SQL);
                foreach (var el in res.Promotions)
                {
                    el.products = pp.Where(x => x.number == el.number);
                    el.warehouses = wh.Where(x => x.number == el.number).Select(a => a.warehouse);
                }
                return res;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new ResultPromotionMobile<ProductsKitMobile>(e.Message);
            }
        }

        public  ResultCategoriesMobile GetCategories()
        {
            try
            {
                using var con = new SqlConnection(MsSqlInit);
                con.Open();
                ResultCategoriesMobile res = new();
                string Sql = @"SELECT TRY_CONVERT(int, gr._Code) AS code,gr._Description AS name  , isnull(TRY_CONVERT(int, grup._Code),0) AS parent
   FROM  [utppsu].dbo._Reference99 Gr
   LEFT JOIN [utppsu].dbo._Reference99  grup ON Gr._ParentIDRRef=grup._IDRRef
   WHERE gr._Folder=0
   --AND grup._Code is null";
                //Тип номенклатури (товар, тара)            
                res.categories = con.Query<GuideMobile>(Sql);                
                con.Close();
                return res;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return new (e.Message);
            }

        }
    }
}
