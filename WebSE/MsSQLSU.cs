using BRB5.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace WebSE
{
    public partial class MsSQL
    {
        string ListGroup = @"(0x9FBD000C29A0FC3111E5ECF85051B305,--01. Алкогольні напої
0x9FBD000C29A0FC3111E5ECF85051B455,--	02. Бакалія мокра
0x9FBD000C29A0FC3111E5ECF8564C2AA9,--	03. Бакалія суха
0x9FBD000C29A0FC3111E5ECF85C44758C,--	03-1. Снеки
0x9FBD000C29A0FC3111E5ECF880249CF2,--	04. Дитяче харчування
0x9FBD000C29A0FC3111E5ECF880249D26,--	05. Здорове харчування
0x9FBD000C29A0FC3111E5ECF8683EA255,--	06. Заморожені продукти
0x9FBD000C29A0FC3111E5ECF8683EA2CC,--	07. Засоби гігієни
0x9FBD000C29A0FC3111E5ECF85C447630,--	08. Кава
0x9FBD000C29A0FC3111E5ECF85C44768E,--	09. Ковбаси
0x9FBD000C29A0FC3111E5ECF880249B6F,--	10. Кондитерські вироби
0x9FBD000C29A0FC3111E5ECF85C447721,--	11. Молоко та молочні продукти
0x9FBD000C29A0FC3111E5ECF862418C2E,--	12. Морозиво
0x9FBD000C29A0FC3111E5ECF85051B3E3,--	13. Напої
0x9FBD000C29A0FC3111E5ECF87A29FDBD,--	14.1 Овочі
0x9FBD000C29A0FC3111E5ECF87A29FEB4,--	14.2 Фрукти
0x9FBD000C29A0FC3111E5ECF86E36F545,--	15. Парфумерно-косметичні вироби
0x9FBD000C29A0FC3111E5ECF85051B3A3,--	16. Пиво
0x9FBD000C29A0FC3111E5ECF862418C52,--	17. Риба і морепродукти
0x9FBD000C29A0FC3111E5ECF862418D75,--	18. Сири
0x86F4005056883C0611F0C5E77FC6E123,--	19-1.Живі квіти
0x9FBD000C29A0FC3111E5ECF8742F489A,--	20. Супутка- стандарт
0x86BD005056883C0611EE31C0651C7779,--	20-1. Одноразові пакети
0x9FBD000C29A0FC3111E5ECF880249B59,--	21. Сухофрукти
0x9FBD000C29A0FC3111E5ECF87A29FD57,--	22. Товари для тварин
0x9FBD000C29A0FC3111E5ECF8683EA36F,--	23. Товари побутової хімії
0x9FBD000C29A0FC3111E5ECF85C44767D,--	24. Тютюнові вироби
0x9FBD000C29A0FC3111E5ECF8683EA21D,--	25.  Хлібний відділ
0x86F5005056883C0611F0EF8EFFCF3021,--	25-1. Торти та тістечка
0x9FBD000C29A0FC3111E5ECF85C447667,--	26. Чай
0x9FBD000C29A0FC3111E5ECF8683EA1F9,--	27. Яйця
0x831B001517DE370411DFA46CBCD6B182,--	a 41. Продукцiя пекарного цеху
0x86D8005056883C0611EF7BDE4D630A40,--	a 41-2. Продукцiя пекарного цеху ПП
0x831B001517DE370411DFA46CFCC3BD57,--	a 43. М'ясо (напiвфабрикати)
0x8699005056883C0611ED44A0F07BE937,--	a 43-1. Копчення  м'ясо
0x86D3005056883C0611EF3780ED4AC230,--	""a 50-2. Зона """"Food to go"""" ПП""
0x80E1000C29F3389511E81313F8B246A9,--	""a 53. Зона """"Піцерія""""""
0x819A0050569E814D11ECB5A59E1E2202 --	a 61. Копчення Риба
)";
        public BaseSU GetBaseSU()
        { 
            BaseSU BaseSU = new();
            using var con = new SqlConnection(MsSqlInit);
            con.Open();
            var sql = $@"WITH Dir AS (SELECT DISTINCT COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef  ) AS IDRRef 
FROM dbo.V1C_dim_nomen dn
  JOIN dbo.V1C_reg_AM am ON am.nomen_RRef=dn.IDRRef AND am.Warehouse_RRef=0xACB5001517DE370411DFF301B4386160 
  LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups3 ON dn._ParentIDRRef = Groups3.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups2 ON Groups3._ParentIDRRef = Groups2.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups1 ON Groups2._ParentIDRRef = Groups1.IDRRef 
WHERE  COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef  ) IN {ListGroup}
)
SELECT --CONVERT(nchar(32),dn.IDRRef,2) AS Id, 
code AS external_id, [desc] AS name, NULL AS parent_external_id 
 FROM dbo.V1C_dim_nomen  dn 
JOIN dir ON Dir.IDRRef=dn.IDRRef
WHERE _ParentIDRRef=0 AND is_leaf=0
UNION ALL 
SELECT dn.code AS external_id, dn.[desc] AS name, dn1.code AS parent_external_id FROM dbo.V1C_dim_nomen dn1 
JOIN dbo.V1C_dim_nomen dn ON dn._ParentIDRRef = dn1.IDRRef
JOIN dir ON Dir.IDRRef=dn1.IDRRef
 WHERE dn1._ParentIDRRef=0 AND dn1.is_leaf=0";
            BaseSU.categories = con.Query<CategorieSU>(sql);
            foreach(var el in BaseSU.categories)
            {
                if(el.name.IndexOf(". ")>0)                
                    el.name = el.name.Substring(el.name.IndexOf(". ") +2);
                if (el.name.StartsWith("14.") ) 
                    el.name = el.name.Substring(4);
            }

            sql = $@"WITH P AS (SELECT Code_wares FROM   sqlsrv2.For_cubes.dbo.V_IsPicture)
,bc AS (SELECT B.nomen_IDRRef,b.bar_code,ROW_NUMBER ( )    OVER ( PARTITION BY B.nomen_IDRRef  ORDER BY DATE DESC) AS nn FROM barcode b)
SELECT dn.code sku, dn.[desc] AS name, dn.[is_weight] as is_weight_based, Groups2.code AS category_id
,dn.name_full AS description,isnull(try_convert(int,b.code_brand),0) as meker_id
,CASE WHEN p.Code_wares is not NULL THEN 'https://api.spar.uz.ua/Wares/'+dn.code+'.png' else null end AS image
,bc.bar_code AS BarCode
,dn.articul AS article
 FROM dbo.V1C_dim_nomen dn
 left JOIN BRAND b ON b._IDRRef=dn.brand_RRef
  JOIN dbo.V1C_reg_AM am ON am.nomen_RRef=dn.IDRRef AND am.Warehouse_RRef=0xACB5001517DE370411DFF301B4386160 --
  LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups3 ON dn._ParentIDRRef = Groups3.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups2 ON Groups3._ParentIDRRef = Groups2.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups1 ON Groups2._ParentIDRRef = Groups1.IDRRef 
LEFT JOIN p ON (p.code_wares= try_convert(int ,dn.code ))
LEFT JOIN bc ON dn.IDRRef=bc.nomen_IDRRef AND bc.nn=1
WHERE   Groups1.IDRRef IN {ListGroup}";
            BaseSU.products = con.Query<ProductSU>(sql);
            sql = @"select try_convert(int,b.code_brand) AS Id,b.name_brand AS name FROM  BRAND b
UNION ALL 
SELECT 0,'Невідмий бренд'";
            BaseSU.mekers = con.Query<MekersSU>(sql);
            sql = @"SELECT w.Code AS shop_id, w.Name AS name,w.Adres AS address,w.GPS AS GPS FROM WAREHOUSES w WHERE w.Code=148";
            BaseSU.Shop = con.Query<ShopSU>(sql);
            con.Close();
            return BaseSU;
        }

        class LoadSUJson
        {
            public string JSON { get; set; }
            public int CodeWarehouse { get; set; }
            public string ABCD { get; set; }
            public bool AddAM { get; set; }
        }
        class LoadSU
        {
            public WaresPrice WP { get; set; }
            public int CodeWarehouse { get; set; }
            public string ABCD { get; set; }
            public bool AddAM { get; set; }
        }
        public RestSU GetRestSU()
        {
            RestSU RestSU = new();
            var sql = $@"WITH fds as (SELECT fds.nomen_id,SUBSTRING(fds.wares_char,1,1) AS ABCD from
 [SQLSRV2].for_cubes.dbo.fact_deficit_surplus fds WHERE  day_id= CONVERT(INT,CONVERT(NCHAR , getdate(),112)) 
   AND fds.warehouse_id= 'ACB5001517DE370411DFF301B4386160' --wh.warehouse_id --Токіо -- 
   AND fds.wares_char is NOT NULL) 
select pj.CodeWarehouse, pj.JSON, fds.ABCD AS ABCD, --SUBSTRING(fds.wares_char,1,1) AS ABCD ,
 CASE WHEN  COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef) IN (0x86D8005056883C0611EF7BDE4D630A40,0x831B001517DE370411DFA46CBCD6B182,0x831B001517DE370411DFA46CFCC3BD57,0x86D3005056883C0611EF3780ED4AC230,0x80DA000C29F3389511E7E3CCDE768B24,0x80E1000C29F3389511E81313F8B246A9) THEN 1 ELSE 0 END as  AddAM
from dbo.PriceJSON pj
JOIN DW.dbo.V1C_dim_warehouse wh ON pj.CodeWarehouse=wh.code
JOIN dbo.V1C_dim_nomen dn ON pj.CodeWares=dn.code 
  JOIN dbo.V1C_reg_AM am ON am.nomen_RRef=dn.IDRRef AND am.Warehouse_RRef= wh.warehouse_RRef -- 0xACB5001517DE370411DFF301B4386160 --Токіо --
  LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups3 ON dn._ParentIDRRef = Groups3.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups2 ON Groups3._ParentIDRRef = Groups2.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups1 ON Groups2._ParentIDRRef = Groups1.IDRRef 
   LEFT JOIN fds ON dn.nomen_id = fds.nomen_id
WHERE COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef  ) IN {ListGroup}
ORDER BY 4 desc";
            var Data = Query<LoadSUJson>(sql);
            var d = Data.Select(x=> new LoadSU() { WP = JsonConvert.DeserializeObject<WaresPrice>(x.JSON), CodeWarehouse = x.CodeWarehouse, ABCD = x.ABCD, AddAM = x.AddAM });
            RestSU.residue = d?.Select(x => new ResidueSU(x.WP,x.CodeWarehouse,x.ABCD,x.AddAM)).Where(x => x.price>0 && x.stock>0).ToList();
            return RestSU;
        }
    }
}
