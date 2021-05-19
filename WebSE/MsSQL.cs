using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace WebSE
{
    public class MsSQL
    {
        public SqlConnection connection;
        public MsSQL()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "10.1.0.22";
            builder.UserID = "dwreader";
            builder.Password = "DW_Reader";
            builder.InitialCatalog = "DW";
            connection = new SqlConnection(builder.ConnectionString);

        }

        public bool Auth(string pPhone)
        {
            var sql = @"SELECT SUM(nn) AS nn FROM 
(SELECT 1 AS nn FROM dbo.client c WHERE c.MainPhone=@Phone OR c.Phone=@Phone
  UNION ALL
SELECT 1 AS nn FROM dbo.bot_client  c WHERE  c.Phone=@Phone) d";
            int r = connection.ExecuteScalar<int>(sql, new { Phone = pPhone });
            return r > 0;
        }

        public bool Register(RegisterUser pUser)
        {
            var sql = @"INSERT INTO DW.dbo.BOT_client (Phone, FirstName,LastName , email, BirthDay, Sex,  NumberOfFamilyMembers, locality, TypeOfEmployment,IdExternal) VALUES 
        (@ShortPhone, @first_name,@last_name, @email, @GetBirthday, @Sex, @family, @locality, @type_of_employment,@IdExternal)";
            int r = connection.Execute(sql, pUser);
            return r > 0;
        }

        public IEnumerable<Direction> GetDirection()
        {
            return connection.Query<Direction>("SELECT vcdgw.CODE_GROUP_WARES AS Code, vcdgw.NAME AS Name FROM dbo.V1C_dim_GROUP_WARES vcdgw WHERE vcdgw.CODE_PARENT_GROUP_WARES IS NULL AND SUBSTRING(name,3,1)='.' AND vcdgw.CODE_GROUP_WARES NOT IN (149758)");
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

        public string GetBarCode(InputPhone pPh)
        {
            var sql = "SELECT TOP 1 c.BarCode FROM dbo.client c WHERE c.MainPhone=@ShortPhone OR c.Phone=@ShortPhone";
            return connection.ExecuteScalar<string>(sql, pPh);
        }
    }

    
}
