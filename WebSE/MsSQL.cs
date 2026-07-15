using BRB5.Model;
using BRB5.Model.DB;
using Dapper;
using Microsoft.Data.SqlClient;
//using Model;
using ModelMID;
using Newtonsoft.Json;
using SharedLib;
using System.Data;
using System.Runtime.CompilerServices;
using System.Transactions;
using UtilNetwork;
using Utils;
//using System.Transactions;

namespace WebSE
{
    public partial class MsSQL:LibApiDCT.SQL.MSSQL
    {        
        public MsSQL():base()
        {
            MsSqlInit = Startup.Configuration.GetValue<string>("MsSqlInit");
            connection = new SqlConnection(MsSqlInit);
        }

        public IEnumerable<Locality> GetLocality()
        {
            var sql = @"SELECT * FROM dbo.BOT_Locality";
            return Query<Locality>(sql);
        }
        public cPrice GetPrice(ApiPrice pParam)
        {
            try
            {
                var Sql = "select dbo.GetPrice(@CodeWarehouse ,@CodeWares,@BarCode,@Article,@TypePriceInfo,@StrWareHouses)";
                var json = ExecuteScalar<string>(Sql, pParam);                
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
            return ExecuteScalar<int>(Sql);
        }
        public int GetNumberDocRaiting()
        {
            string Sql = "SELECT (NEXT VALUE FOR DW.dbo.GetNumberDocRaiting )";
            return ExecuteScalar<int>(Sql);
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
            return Execute(Sql, pRt);            
        }

        public int DeleteRaitingTemplateItem(RaitingTemplate pRt)
        {
            string Sql = @"delete from dbo.RaitingTemplateItem where IdTemplate = @IdTemplate";
            return Execute(Sql, pRt);
        }

        public int InsertRaitingTemplateItem(IEnumerable<RaitingTemplateItem> pR)
        {
            string Sql = @"INSERT INTO dbo.RaitingTemplateItem (IdTemplate, Id, Parent, IsHead, Text, RatingTemplate, ValueRating, OrderRS) 
          VALUES (@IdTemplate, @Id, @Parent, @IsHead, @Text, @RatingTemplate,@ValueRating, @OrderRS)";
            using var scope = new TransactionScope();
            using var con = new SqlConnection(MsSqlInit);
            con.Open();
            int i= BulkExecuteNonQuery<RaitingTemplateItem>(Sql, pR);
            scope.Complete();
            con.Close();
            return i;
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
            return Execute(Sql, pDoc);
        }

        public IEnumerable<RaitingTemplate> GetRaitingTemplate()
        {
            string Sql = @"select IdTemplate, Text,IsActive from dbo.RaitingTemplate";
            var res = Query<RaitingTemplate>(Sql);
            var item = Query<RaitingTemplateItem>("select IdTemplate, Id, Parent, IsHead, Text, RatingTemplate, OrderRS,ValueRating from  dbo.RaitingTemplateItem");
            foreach (var r in res)
            {
                r.Item = item.Where(e => Convert.ToInt32(e.IdTemplate) == r.IdTemplate);
            }
            return res;
        }
        public IEnumerable<Doc> GetRaitingDocs()
        {
            string Sql = @"select NumberDoc, IdTemplate, CodeWarehouse, DateDoc,  Description from dbo.RaitingDoc";
            return Query<Doc>(Sql);
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
            return Query<Doc>(SQL);
        }

        public IEnumerable<DocWares> GetPromotionData(string pNumberDoc)
        {
            string SQL = $@"SELECT pg._Number AS NumberDoc, try_convert(int,nom.code) CodeWares,pgn._LineNo11667 AS ""order""
       from [utppsu].[dbo].[_Document374]  pg
       JOIN [utppsu].[dbo].[_Document374_VT11666]  pgn ON pg._IDRRef=pgn._Document374_IDRRef
       JOIN dbo.v1c_dim_nomen nom ON nom.IDRRef = pgn._Fld11668RRef
       WHERE pg._Number={pNumberDoc} AND Year(pg._Date_Time) = year(getdate())+2000";
            return Query<DocWares>(SQL);
        }
        public IEnumerable<Client> GetClient(string parBarCode = null, string parPhone = null, string parName = null, long parCodeClient = 0)
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
     (select dbo.Concatenate(ph.data+',') from ClientData ph where  p.CodeClient = ph.CodeClient and TypeData=1)   as BarCode,
     (select dbo.Concatenate(ph.data+',') from ClientData ph where  p.CodeClient = ph.CodeClient and TypeData=2) as MainPhone,
       BIRTHDAY as BirthDay, StatusCard as StatusCard 
   from t
     join dbo.client p on (t.CodeClient=p.codeclient)
   left join dbo.V1C_DIM_TYPE_DISCOUNT td on td.TYPE_DISCOUNT=p.TypeDiscount";
            return Query<Client>(SQL, new { CodeClient = parCodeClient, Phone = parPhone, BarCode = parBarCode, Name = (parName == null ? null : "%" + parName + "%") });             
        }

        public bool SaveDocData(SaveDoc pD)
        {
            using var scope = new TransactionScope();
            using var con = new SqlConnection(MsSqlInit);
            con.Open();
            con.Execute($"delete from dbo.Doc_1C  where type_doc = @TypeDoc and number_doc = @NumberDoc and name_TZD='{pD.NameDCT ?? ""}'", pD.Doc);//and order_doc = @OrderDoc
            string SQL = $"insert into dbo.Doc_1C (type_doc, name_TZD, number_doc,order_doc,code_wares,quantity,Code_Reason) values (@TypeDoc,'{pD.NameDCT ?? ""}' ,@NumberDoc, @OrderDoc, @CodeWares, @InputQuantity, @CodeReason)";
            BulkExecuteNonQuery(SQL, pD.Wares,con);
            scope.Complete();
            con.Close();
            return true;
        }
        
        public void SaveLogPrice(BRB5.Model.LogPriceSave pD)
        {
            try
            {
                using var scope = new TransactionScope();
                using var con = new SqlConnection(MsSqlInit);
                con.Open();
                string SQL = $@"INSERT INTO DW.dbo.LOGPRICE
    (code_warehouse,     code_wares, is_good, BarCode, NumberOfReplenishment,  dt_insert, code_user ,   Number_Packege, SerialNumber) VALUES
    ({pD.CodeWarehouse}, @CodeWares, @Status,@BarCode,@NumberOfReplenishment, @DTInsert, {pD.CodeUser},@PackageNumber, '{pD.SerialNumber}')";
                BulkExecuteNonQuery(SQL, pD.LogPrice,con);
                scope.Complete();
                con.Close();               
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
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
            return Query<IdReceipt>(SQL);
        }
              

        public IEnumerable<ReceiptWares> GetClientOrder(string pNumberOrder)
        {
            string SQL = "SELECT oc.CodeWares,oc.CodeUnit, oc.Quantity, oc.Price, oc.Sum FROM dbo.V1C_doc_Order_Client oc WHERE oc.NumberOrder = @NumberOrder";// 'ПСЮ00006865'
            return Query<ReceiptWares>(SQL, new { NumberOrder = pNumberOrder });
        }

        public Dictionary<string, decimal> GetReceipt1C(IdReceipt pIdR)
        {
            //DateTime pDT, int pIdWorkplace
            var Res = new Dictionary<string, decimal>();
            var SQL = "SELECT number,sum FROM dbo.V1C_doc_receipt WHERE IdWorkplace=@IdWorkplace AND  _Date_Time > DATEADD(year,2000, @DTPeriod) AND _Date_Time < DATEADD(day,1,DATEADD(year,2000, @DTPeriod))";
            var res = Query<Res>(SQL, pIdR);
            foreach (var el in res)
                Res.Add(el.Number, el.Sum);
            return Res;
        }


        public IEnumerable<int> GetWorkPlaces()
        {
            string SQL = @"SELECT  min(cd.code)  FROM  DW.dbo.V1C_CashDesk cd --TOP 5
JOIN WAREHOUSES w on w.Code=cd.CodeWarehouse 
WHERE w.Code NOT IN (SELECT w.CodeWarehouse2 FROM WAREHOUSES w WHERE w.CodeWarehouse2>0 AND w.Code<>w.CodeWarehouse2)
GROUP BY CodeWarehouse ORDER by 1";
            return Query<int>(SQL);
        }

        public bool SetWeightReceipt(IEnumerable<WeightReceipt> pWR)
        {
            using var scope = new TransactionScope();
            using var con = new SqlConnection(MsSqlInit);
            con.Open();
            string SQLUpdate = @"insert into  DW.dbo.Weight_Receipt  (Type_Source,code_wares, weight,Date,ID_WORKPLACE, CODE_RECEIPT,QUANTITY) values (@TypeSource, @CodeWares,@Weight,@Date,@IdWorkplace,@CodeReceipt,@Quantity)";
            var Res = BulkExecuteNonQuery<WeightReceipt>(SQLUpdate, pWR,con) > 0;
            scope.Complete();
            con.Close();
            return Res;
        }

        public string GetPrefixDNS(long pWh)
        {
            string SQL = $@"SELECT p_DNS._Fld12274_S
 FROM  [utppsu].dbo._Reference133 AS Wh
    JOIN [utppsu].dbo._InfoRg12271 AS p_DNS ON p_DNS._Fld12273RRef = 0x86D4005056883C0611EF4F35DACA90B4 AND p_DNS._Fld12272_RRRef = Wh._IDRRef
    WHERE wh._Code={pWh}";
            return ExecuteScalar<string>(SQL);
        }

        string GetTmpWh(int pCodeWarehouse,bool pIsNomen=false)
        {
           string Res = $@"DECLARE @WarehouseRRef  BINARY(16);
SELECT @WarehouseRRef=Wh._IDRRef  FROM [utppsu].dbo._Reference133 AS Wh WHERE TRY_CONVERT(int, Wh._Code)={pCodeWarehouse};

IF OBJECT_ID('tempdb..#Wh') IS NOT NULL
 DROP TABLE #Wh;
CREATE TABLE #Wh ( WarehouseRRef BINARY(16)  PRIMARY KEY);
IF @WarehouseRRef is NOT NULL
BEGIN
INSERT INTO  #Wh (WarehouseRRef)
SELECT @WarehouseRRef AS WarehouseRRef
UNION 
SELECT p_lw._Fld12272_RRRef FROM  [utppsu].dbo._InfoRg12271  p_lw 
WHERE  p_lw._Fld12273RRef = 0x86C0005056883C0611EE6103D874A9FD AND _Fld12274_RRRef=@WarehouseRRef
UNION 
SELECT p_lw._Fld12274_RRRef FROM  [utppsu].dbo._InfoRg12271  p_lw 
WHERE  p_lw._Fld12273RRef = 0x86C0005056883C0611EE6103D874A9FD AND p_lw._Fld12272_RRRef=@WarehouseRRef;
end 
" +
(pIsNomen?
$@"IF OBJECT_ID('tempdb..#Nomen') IS NOT NULL
 DROP TABLE  #Nomen;
CREATE TABLE #Nomen ( NomenRRef BINARY(16)  PRIMARY KEY);
IF @WarehouseRRef is NOT NULL
BEGIN
INSERT INTO #Nomen (NomenRRef)
SELECT am.nomen_RRef   FROM  dbo.V1C_reg_AM am 
JOIN #Wh AS wh ON wh.WarehouseRRef=am.Warehouse_RRef
--WHERE am.Warehouse_RRef=@WarehouseRRef
UNION 
SELECT nomen_RRef FROM dbo.V1C_reg_wares_warehouse wwh 
JOIN #Wh AS wh ON wh.WarehouseRRef=wwh.Warehouse_RRef;
end
" :"");
            return Res;
        }



        public BRB5.Model.Guid GetGuid(int pCodeWarehouse, int pCodeUser = 0, GetGuid pCode = null)
        {
            bool IsCode = (pCode?.BarCode?.Any()==true || pCode?.CodeWares?.Any()==true);
            using (var scope = new TransactionScope())
            {
                using (var Con = new SqlConnection(MsSqlInit))
                {
                    string Sql;
                    BRB5.Model.Guid Res = new() { NameCompany = "ПСЮ" };
                    Con.Open();
                    if (pCodeWarehouse != 0 || IsCode)
                    {
                        if (IsCode)
                            Sql = $@"IF OBJECT_ID('tempdb..#Nomen') IS NOT NULL
 DROP TABLE  #Nomen;
CREATE TABLE #Nomen ( NomenRRef BINARY(16)  PRIMARY KEY);
INSERT INTO #Nomen ( NomenRRef) 
SELECT b.nomen_IDRRef FROM barcode b WHERE b.bar_code in ({pCode.StrBarCode})
union 
SELECT w._IDRRef FROM dbo.Wares w WHERE w.code_wares IN ({pCode.StrCodeWares})";
                        else
                            Sql = GetTmpWh(pCodeWarehouse, true);
                        Con.Execute(Sql);
                        if (!IsCode)
                        {
                            Sql = "SELECT code_unit AS CodeUnit, abr_unit AS AbrUnit, name_unit AS NameUnit FROM dbo.UNIT_DIMENSION";
                            Res.UnitDimension = Con.Query<BRB5.Model.DB.UnitDimension>(Sql);
                        }

                        Sql = @"SELECT AU.code_wares AS CodeWares, AU.code_unit AS CodeUnit, AU.coef as Coefficient FROM dbo.V_addition_unit AU
  JOIN #Nomen n on au.NomenRRef=n.NomenRRef";
                        Res.AdditionUnit = Con.Query<BRB5.Model.DB.AdditionUnit>(Sql);

                        Sql = @"SELECT  try_convert(int,w.code_wares) AS CodeWares
       ,try_convert(int,w.code_group) AS CodeGroup
       ,w.name_wares AS NameWares
       ,try_convert(int,w.articl) AS Article
       ,w.code_brand AS CodeBrand
       ,w.VAT AS vat
       ,w.VAT_OPERATION AS VatOperation
       ,w.code_unit AS CodeUnit
  FROM dbo.Wares w 
  JOIN #Nomen on w._IDRRef=NomenRRef";
                        Res.Wares = Con.Query<BRB5.Model.DB.Wares>(Sql);

                        Sql = @"SELECT code_wares AS CodeWares, code_unit AS CodeUnit, bar_code AS BarCode FROM DW.dbo.V_BARCODES B
  JOIN #Nomen on B.nomen_IDRRef=NomenRRef";
                        Res.BarCode = Con.Query<BRB5.Model.DB.BARCode>(Sql);
                        if (!IsCode)
                        {
                            Sql = "SELECT try_convert(int,gw.code_group_wares) AS CodeGroup, gw.name AS NameGroup FROM  GROUP_WARES gw";
                            Res.GroupWares = Con.Query<BRB5.Model.DB.GroupWares>(Sql);
                            /*Sql = "SELECT try_convert(int,code) AS CodeReason, [desc] AS NameReason FROM TK_OLAP.[dbo].[dim_rejection_reason]";
                        Res.Reason= connection.Query<Reason>(Sql);*/
                            Res.Reason = [new Reason() { CodeReason = 1, NameReason = "Брак" }, new Reason() { CodeReason = 4, NameReason = "Протермінований" }];
                        }
                    }
                    if (pCodeUser != 0)
                    {
                        Sql = $@"SELECT
  CASE WHEN CodeProfile in ( -2,-6) THEN '' ELSE CASE WHEN CodeWarehouse>0 THEN  ' and code_shop='''+
  (SELECT  max(wh.code_shop) FROM  dbo.WAREHOUSES wh WHERE wh.Code=e.CodeWarehouse)+''''  ELSE ' and 1=2' end  END  
FROM DW.dbo.Employee  e WHERE CodeUser ={pCodeUser};";
                        string Sql1 = Con.ExecuteScalar<string>(Sql);
                        Sql = $@"SELECT w.Code AS Code, w.Name AS Name, w.Code_TM AS CodeTM, w.GPS AS Location, w.Adres AS Address FROM  WAREHOUSES w WHERE w.type_warehouse IN (11,50,51,54,1211,7,6,3) {Sql1}";
                        Res.Warehouse = Con.Query<BRB5.Model.Warehouse>(Sql);
                    }
                    return Res;
                }
            }
        }

        public Docs LoadDocs(GetDocs pGD)
        {
           
            Docs res = new();
            if (pGD.CodeWarehouse == 0) return res;
            using (var Con = new SqlConnection(MsSqlInit))
            {
                Con.Open();
                string Sql= GetTmpWh(pGD.CodeWarehouse);
                Con.Execute(Sql);
                Sql = @"WITH Wh AS 
(SELECT TRY_CONVERT(int, wh._Code) AS code_warehouse
 FROM [utppsu].dbo._Reference133 wh
 JOIN #Wh  ON wh._IDRRef=WarehouseRRef)
SELECT di.code_warehouse AS CodeWarehouse
      ,1 AS TypeDoc
      ,di.date_time AS DateDoc
      ,di.number AS NumberDoc
      ,di.ext_info AS ExtInfo
      ,di.name_user AS NameUser
      ,null AS BarCode
      FROM dbo.V1C_doc_inventory di
      JOIN  wh ON wh.code_warehouse=di.code_warehouse
where @TypeDoc in (-1,0,1)
union all
SELECT dm.code_warehouse AS CodeWarehouse
      ,3 AS TypeDoc
      ,dm.date_time AS DateDoc
      ,dm.number AS NumberDoc
      ,dm.ext_info AS ExtInfo
      ,dm.name_user AS NameUser
      ,case WHEN left(dm.ext_info,14)='CargoBarCode = 'THEN SUBSTRING(dm.ext_info,16,13) END AS BarCode
      FROM dbo.V1C_doc_movement dm
      JOIN  wh ON wh.code_warehouse=dm.code_warehouse
where @TypeDoc in (-1,0,3)
union all
SELECT dm.code_warehouse AS CodeWarehouse
      ,8 AS TypeDoc
      ,dm.date_time AS DateDoc
      ,dm.number AS NumberDoc
      ,dm.ext_info AS ExtInfo
      ,dm.name_user AS NameUser
      ,case WHEN left(dm.ext_info,14)='CargoBarCode = 'THEN SUBSTRING(dm.ext_info,16,13) END AS BarCode
      FROM dbo.V1C_doc_movement dm
      JOIN  wh ON wh.code_warehouse=dm.code_warehouse_in
 where @TypeDoc in (-1,0,8)
union all

SELECT dw.code_warehouse AS CodeWarehouse
      ,4 AS TypeDoc
      ,dw.date_time AS DateDoc
      ,dw.number AS NumberDoc
      ,dw.ext_info AS ExtInfo
      ,dw.name_user AS NameUser
      ,null AS BarCode
      FROM dbo.v1c_doc_write_off dw
      JOIN  wh ON wh.code_warehouse=dw.code_warehouse
where @TypeDoc in (-1,0,4)
union all

SELECT drs.code_warehouse AS CodeWarehouse
      ,5 AS TypeDoc
      ,drs.date_time AS DateDoc
      ,drs.number AS NumberDoc
      ,drs.ext_info AS ExtInfo
      ,drs.name_user AS NameUser
      ,null AS BarCode
      FROM dbo.v1c_doc_return_suppl drs
      JOIN  wh ON wh.code_warehouse=drs.code_warehouse
where @TypeDoc in (-1,0,5)
union all 
SELECT dwh.code AS CodeWarehouse
      ,ocw.State+21 AS TypeDoc
      ,DATEADD(YEAR,-2000,ocw.datetime) AS DateDoc     
      , ocw.number AS NumberDoc
      ,w.NameWares AS ExtInfo
      ,/*ocw.name_user*/NULL AS NameUser
      ,null AS BarCode
      FROM dbo.V1C_OrderToCollectWares ocw
      JOIN #Wh ON ocw.WarehouseRRef = #Wh.WarehouseRRef      
      JOIN dbo.V1C_dim_warehouse dwh ON dwh.warehouse_RRef=ocw.WarehouseRRef
      LEFT JOIN dbo.v1c_dim_wares w ON ocw.Direction=w.WaresRRef
WHERE ( @TypeDoc in (-1,0) OR ( @TypeDoc BETWEEN 21 and 29 AND ocw.State=@TypeDoc-21 )) AND
 DATEADD(YEAR,-2000,ocw.datetime) >DATEADD(day,-30,GETDATE())
";
                if (pGD.TypeDoc>=0 && pGD.TypeDoc < 30)
                {
                    res.Doc = Con.Query<Doc>(Sql, pGD);

                    Sql = @"WITH Wh AS 
(SELECT TRY_CONVERT(int, wh._Code) AS code_warehouse
 FROM [utppsu].dbo._Reference133 wh
 JOIN #Wh  ON wh._IDRRef=WarehouseRRef)
SELECT --dwi.code_warehouse AS CodeWarehouse
      1 AS TypeDoc
      ,number_doc AS NumberDoc
      ,order_doc AS [Order]
      ,code_wares AS CodeWares
      ,Quantity 
      ,0 as QuantityMin, 1000000 as QuantityMax
      FROM DW.dbo.V1C_docit_inventory dwi
      JOIN  wh ON wh.code_warehouse=dwi.code_warehouse
where @TypeDoc in (-1,0,1)
UNION all
select 3 as type_doc,wi.number_doc as number_doc, wi.order_doc AS order_doc, wi.code_wares, wi.quantity as quantity, 0 as quantity_min, 1 as quantity_max 
from dbo.v1c_docit_movement  wi
          JOIN  wh ON wh.code_warehouse=wi.code_warehouse 
          where @TypeDoc in (-1,0,3)
UNION all
select 8 as type_doc,wi.number_doc as number_doc, wi.order_doc AS order_doc, wi.code_wares, wi.quantity as quantity, 0 as quantity_min, 1 as quantity_max 
from dbo.v1c_docit_movement  wi
          JOIN  wh ON wi.code_warehouse_in =wh.code_warehouse
          where @TypeDoc in (-1,0,8)
union all
SELECT ocw.State+21 AS TypeDoc, ocw.number  number_doc, 
ocww.LineN AS order_doc, w.codeWares AS code_wares, QuantityRequired as quantity, 0 as quantity_min, QuantityRequired as quantity_max 
      FROM dbo.V1C_OrderToCollectWares ocw
      JOIN DW.dbo.V1C_OrderToCollectWaresWares ocww ON ocw.IDRRef= ocww.IDRRef
      JOIN #Wh ON ocw.WarehouseRRef = #Wh.WarehouseRRef      
      JOIN dbo.V1C_dim_warehouse dwh ON dwh.warehouse_RRef=ocw.WarehouseRRef
      LEFT JOIN dbo.v1c_dim_wares w ON ocww.WaresRRef=w.WaresRRef
WHERE ( @TypeDoc in (-1,0) OR ( @TypeDoc BETWEEN 21 and 29 AND ocw.State=@TypeDoc-21 )) AND
 DATEADD(YEAR,-2000,ocw.datetime) >DATEADD(day,-30,GETDATE())";
                    res.Wares = Con.Query<DocWaresSample>(Sql, pGD);
                }
                               

                if (pGD.TypeDoc == 51)
                { 
                    Sql = @"WITH p AS (SELECT WhD.DealerRRef FROM V1C_dim_warehouse wh 
JOIN DW.dbo.V1C_dim_WarehouseDealer WhD ON wh.warehouse_RRef=whd.WarehouseRRef
WHERE wh.code=@CodeWarehouse)
SELECT 51 as TypeDoc, d.Number AS NumberDoc, d.DateTime AS DateDoc, d.Comment AS description,d.info AS ExtInfo ,
  (SELECT count(DISTINCT spw.NomenRRef ) FROM dbo.V1C_Docit_SettingPricesWares AS spw WHERE spw.IDRRef=d.IDRRef) AS CountWares
FROM DW.dbo.V1C_Doc_SettingPrices d
JOIN DW.dbo.V1C_Docit_SettingPricesDealer ds ON d.IDRRef=ds.IDRRef
JOIN p ON p.DealerRRef=ds.DealerRRef
WHERE DateTime1C>= DATEADD(year,2000,  CONVERT(date, GETDATE()))";
                    res.Doc = Con.Query<Doc>(Sql, pGD);
                    Sql = @"WITH p AS (SELECT WhD.DealerRRef FROM V1C_dim_warehouse wh 
JOIN DW.dbo.V1C_dim_WarehouseDealer WhD ON wh.warehouse_RRef=whd.WarehouseRRef
WHERE wh.code=@CodeWarehouse)
SELECT 51 as TypeDoc, d.Number AS NumberDoc,try_convert(int, w.code_wares) AS CodeWares, 1 AS Quantity
FROM DW.dbo.V1C_Doc_SettingPrices d
JOIN dbo.V1C_Docit_SettingPricesWares AS spw ON spw.IDRRef=d.IDRRef
JOIN dbo.Wares w ON spw.NomenRRef = w._IDRRef
JOIN DW.dbo.V1C_Docit_SettingPricesDealer ds ON d.IDRRef=ds.IDRRef
JOIN p ON p.DealerRRef=ds.DealerRRef
WHERE DateTime1C>= DATEADD(year,2000,  CONVERT(date, GETDATE()))";
                    res.Wares = Con.Query<DocWaresSample>(Sql, pGD);
                }
            }
            return res;
        }

        public AnswerLogin Login(UserBRB pU)
        {
            string Sql = @"SELECT Top 1 e.CodeUser, e.Login,e.PassWord,e.BarCode,1 AS Role, e.NameUser 
FROM  Employee e WHERE (upper(e.Login)=upper(@Login) and ( LEN(@BarCode)=0 or @BarCode is null )) OR (LEN(@BarCode)>0 and e.BarCode=@BarCode)";
            var Res = Query<AnswerLogin>(Sql, pU);
            return Res.FirstOrDefault();
        }

        public IEnumerable<Model.CustomerBarCode> GetCustomerBarCode()
        {
            var res = ExecuteScalar<string>(@"SELECT DW.dbo.GetCustomerBarCode()");
            var Res=System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Model.CustomerBarCode>>(res);
            return Res;
        }

        public IEnumerable<ModelMID.Client> GetClient(FindClient pFC)
        {            
            var res = Query<ModelMID.Client>(@"with t as 
(
select p.Codeclient from dbo.ClientData p where ( p.Data = @Phone and TypeData=2)
union 
select CodeClient from dbo.client p where CodeClient = @CodeClient
union 
 select CodeClient from dbo.clientData p where ( p.Data = @BarCode and TypeData=1) 
)

select p.CodeClient as CodeClient, p.nameClient as NameClient, 0 as TypeDiscount, td.NAME as NameDiscount, p.PersentDiscount as PersentDiscount, 0 as CodeDealer, 
	   0.00 as SumMoneyBonus, 0.00 as SumBonus,1 IsUseBonusFromRest, 1 IsUseBonusToRest,1 as IsUseBonusFromRest, 
     --(select group_concat(ph.data) from ClientData ph where  p.Code_Client = ph.CodeClient and TypeData=1)   as BarCode,
      -- (select group_concat(ph.data) from ClientData ph where  p.Code_Client = ph.CodeClient and TypeData=2) as MainPhone,
   
       BIRTHDAY as BirthDay, StatusCard as StatusCard,
       CASE WHEN kard_disc_type_id IN (0xBC8CFC297E763BE448E1098F069E2D9A,0xBE42F21E3C6F33804B2BF6D344591EBF) THEN 1 ELSE 0 END AS IsСertificate
   from t
     join dbo.client p on (t.CodeClient=p.CodeClient)
   left join dbo.V1C_DIM_TYPE_DISCOUNT td on td.TYPE_DISCOUNT=p.TYPEDISCOUNT;", pFC);           
            return res;
        }

        public ModelMID.Wares GetWaresPlu(int pPlu)
        {
            string Sql = $@"SELECT w.code_wares AS CodeWares, w.name_wares AS NameWares, w.code_group AS CodeGroup
        , CASE WHEN W.ARTICL= '' OR W.ARTICL IS NULL THEN '-'+W.code_wares ELSE W.ARTICL END  AS Articl
        , w.code_unit AS CodeUnit, w.VAT AS PercentVat , w.VAT_OPERATION AS TypeVat, w.code_brand AS CodeBrand
        , CASE WHEN  Type_wares= 2 AND w.Code_Direction= '000147850' THEN 4 ELSE Type_wares  END  as TypeWares
        , Weight_Brutto as WeightBrutto
  --, Weight_Fact as WeightFact_
  ,  Weight_Fact AS WeightFact
  , w.Weight_Delta as WeightDelta, w.code_UKTZED AS CodeUKTZED, w.Limit_age as LimitAge, w.PLU, w.Code_Direction as CodeDirection
  , w.code_brand as CodeTM -- бо в 1С спутано.
  FROM dbo.Wares w  WHERE w.plu = {pPlu}";
          
            var res = Query<ModelMID.Wares>(Sql);            
            return res.FirstOrDefault();
        }
    }
    class Res
    {
        public string Number { get; set; }
        public decimal Sum { get; set; }
    }
}
