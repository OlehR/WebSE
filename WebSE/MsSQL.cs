﻿using BRB5.Model;
using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static QRCoder.PayloadGenerator.SwissQrCode;
//using System.Transactions;

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

        public  int BulkExecuteNonQuery<T>(string pQuery, IEnumerable<T> pData, bool IsRepeatNotBulk = false)
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

        public cPrice GetPrice(object pParam)
        {
            var Sql = "select dbo.GetPrice(@CodeWarehouse ,@CodeWares,null,@Article,0)";
            var json = connection.ExecuteScalar<string>(Sql, pParam);
            var price = JsonConvert.DeserializeObject<cPrice>(json);
            return price;
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
        string Sql= @"begin tran
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
            string Sql = @"INSERT INTO dbo.RaitingTemplateItem (IdTemplate, Id, Parent, IsHead, Text, RatingTemplate, OrderRS) 
          VALUES (@IdTemplate, @Id, @Parent, @IsHead, @Text, @RatingTemplate, @OrderRS)";
            return BulkExecuteNonQuery<RaitingTemplateItem> (Sql, pR);

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
            var res= connection.Query<RaitingTemplate>(Sql);
            var item = connection.Query<RaitingTemplateItem>("select IdTemplate, Id, Parent, IsHead, Text, RatingTemplate, OrderRS from  dbo.RaitingTemplateItem");
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
    }
}
