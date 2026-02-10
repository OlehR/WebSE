using BRB5.Model;
using Dapper;
using Newtonsoft.Json;

namespace WebSE
{
    public partial class MsSQL
    {
        public BaseSU GetBaseSU()
        {
            BaseSU BaseSU = new();
            var sql = @"WITH Dir AS (SELECT DISTINCT COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef  ) AS IDRRef 
FROM dbo.V1C_dim_nomen dn
  JOIN dbo.V1C_reg_AM am ON am.nomen_RRef=dn.IDRRef AND am.Warehouse_RRef=0x8686005056883C0611ECDC1488054374 --Ера
  LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups3 ON dn._ParentIDRRef = Groups3.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups2 ON Groups3._ParentIDRRef = Groups2.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups1 ON Groups2._ParentIDRRef = Groups1.IDRRef 
WHERE  COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef  ) NOT IN (0x9FBD000C29A0FC3111E5ECF86E36F695,0x831B001517DE370411DFA46CA9AC08B4,0x86BF005056883C0611EE5D2B14008AEC,0x831B001517DE370411DFA46D233CD10F,0x869E005056883C0611ED7605D14A1A3D
,0x80DA000C29F3389511E7E3CD1E441571,0x831B001517DE370411DFA46F147AE02D,0x81740050569E814D11EBDF2B5D2FA879,0x81960050569E814D11EC89AD29872591)
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
 WHERE dn1._ParentIDRRef=0 AND dn1.is_leaf=00";
            BaseSU.categories = connection.Query<CategorieSU>(sql);
            foreach(var el in BaseSU.categories)
            {
                if(el.name.IndexOf(". ")>0)                
                    el.name = el.name.Substring(el.name.IndexOf(". ") +2);
                if (el.name.StartsWith("14.") ) 
                    el.name = el.name.Substring(4);
            }

            sql = @"WITH P AS (SELECT Code_wares FROM   sqlsrv2.For_cubes.dbo.V_IsPicture)
SELECT dn.code sku, dn.[desc] AS name, dn.[is_weight] as is_weight_based, Groups2.code AS category_id
,dn.name_full AS description,try_convert(int,b.code_brand) as meker_id
,CASE WHEN p.Code_wares is not NULL THEN 'https://api.spar.uz.ua/Wares/'+dn.code+'.png' else null end AS image
 FROM dbo.V1C_dim_nomen dn
 JOIN BRAND b ON b._IDRRef=dn.brand_RRef
  JOIN dbo.V1C_reg_AM am ON am.nomen_RRef=dn.IDRRef AND am.Warehouse_RRef=0x8686005056883C0611ECDC1488054374 --Ера
  LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups3 ON dn._ParentIDRRef = Groups3.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups2 ON Groups3._ParentIDRRef = Groups2.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups1 ON Groups2._ParentIDRRef = Groups1.IDRRef 
LEFT JOIN p ON (p.code_wares= try_convert(int ,dn.code ))
WHERE COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef  ) NOT IN (0x9FBD000C29A0FC3111E5ECF86E36F695,0x831B001517DE370411DFA46CA9AC08B4,0x86BF005056883C0611EE5D2B14008AEC,0x831B001517DE370411DFA46D233CD10F,0x869E005056883C0611ED7605D14A1A3D
,0x80DA000C29F3389511E7E3CD1E441571,0x831B001517DE370411DFA46F147AE02D,0x81740050569E814D11EBDF2B5D2FA879,0x81960050569E814D11EC89AD29872591)";
            BaseSU.products = connection.Query<ProductSU>(sql);
            sql = @"select try_convert(int,b.code_brand) AS Id,b.name_brand AS name FROM  BRAND b";
            BaseSU.mekers = connection.Query<MekersSU>(sql);
            sql = @"SELECT w.Code AS shop_id, w.Name AS name,w.Adres AS address,w.GPS AS GPS FROM WAREHOUSES w WHERE w.Code=148";
            BaseSU.Shop = connection.Query<ShopSU>(sql);
            return BaseSU;
        }

        class LoadSUJson
        {
            public string JSON { get; set; }
            public int CodeWarehouse { get; set; }
        }
        class LoadSU
        {
            public WaresPrice WP { get; set; }
            public int CodeWarehouse { get; set; }
        }
        public RestSU GetRestSU()
        {
            RestSU RestSU = new();
            var sql = @"select pj.CodeWarehouse, pj.JSON from dbo.PriceJSON pj
JOIN dbo.V1C_dim_nomen dn ON pj.CodeWares=dn.code
 JOIN BRAND b ON b._IDRRef=dn.brand_RRef
  JOIN dbo.V1C_reg_AM am ON am.nomen_RRef=dn.IDRRef AND am.Warehouse_RRef=0x8686005056883C0611ECDC1488054374 --Ера
  LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups3 ON dn._ParentIDRRef = Groups3.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups2 ON Groups3._ParentIDRRef = Groups2.IDRRef 
   LEFT OUTER JOIN  dbo.V1C_dim_nomen AS Groups1 ON Groups2._ParentIDRRef = Groups1.IDRRef 
WHERE COALESCE(Groups1.IDRRef,Groups2.IDRRef,Groups3.IDRRef  ) NOT IN (0x9FBD000C29A0FC3111E5ECF86E36F695,0x831B001517DE370411DFA46CA9AC08B4,0x86BF005056883C0611EE5D2B14008AEC,0x831B001517DE370411DFA46D233CD10F,0x869E005056883C0611ED7605D14A1A3D
,0x80DA000C29F3389511E7E3CD1E441571,0x831B001517DE370411DFA46F147AE02D,0x81740050569E814D11EBDF2B5D2FA879,0x81960050569E814D11EC89AD29872591)";
            var Data = connection.Query<LoadSUJson>(sql);
            var d = Data.Select(x=> new LoadSU () {WP= JsonConvert.DeserializeObject<WaresPrice>(x.JSON), CodeWarehouse= x.CodeWarehouse });
            RestSU.residue = d?.Select(x => new ResidueSU(x.WP,x.CodeWarehouse));
            return RestSU;
        }
    }
}
