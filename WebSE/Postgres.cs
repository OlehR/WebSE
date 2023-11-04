using Npgsql;
using Dapper;

using ModelMID;
using System.Transactions;
using System.Collections.Generic;
using System;
using System.Linq;
using ModelMID.DB;
using System.Text.Json.Serialization;
using System.Text.Json;
using Utils;
using NpgsqlTypes;
using static Dapper.SqlMapper;
using System.Data;
using System.Collections;
using BRB5.Model;

namespace WebSE
{

    public class JsonParameter : ICustomQueryParameter
    {
        private readonly string _value;

        public JsonParameter(string value)
        {
            _value = value;
        }

        public void AddParameter(IDbCommand command, string name)
        {
            var parameter = new NpgsqlParameter(name, NpgsqlDbType.Json);
            parameter.Value = _value;

            command.Parameters.Add(parameter);
        }
    }

    public class Postgres
    {
        NpgsqlConnection Con = new NpgsqlConnection(connectionString: "Server=localhost;Port=5433;User Id=postgres;Password=Nataly75;Database=DW;");
        //NpgsqlTransaction Transaction = null;
        public Postgres()
        {
            try
            {
                Con.Open();
            }
            catch (Exception ex)
            { }
        }
        public void CreateTable()
        {
            //con.CreateTable<Receipt>();
        }
        public StatusData SaveReceipt(Receipt pR)
        {
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            string Json = JsonSerializer.Serialize(pR, options);
            int Id = Con.ExecuteScalar<int>(@"insert into ""LogInput""(""IdWorkplace"", ""CodePeriod"", ""CodeReceipt"", ""JSON"") values (@IdWorkplace, @CodePeriod, @CodeReceipt, @JSON) RETURNING ""Id""",
                                        new { pR.IdWorkplace, pR.CodePeriod, pR.CodeReceipt, JSON = new JsonParameter(Json) });

            using NpgsqlTransaction Transaction = Con.BeginTransaction();
            string SqlDelete = @"delete from ""TABLE"" where  ""IdWorkplace"" = @IdWorkplace and ""CodePeriod"" =@CodePeriod and  ""CodeReceipt""=@CodeReceipt";
            string SQL = @"insert into ""Receipt"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""DateReceipt"",""TypeReceipt"",""CodeClient"",""CodePattern"",""NumberCashier"",""StateReceipt"",""NumberReceipt"",""NumberOrder"",""SumFiscal"",""SumReceipt"",""VatReceipt"",""PercentDiscount"",""SumDiscount"",""SumRest"",""SumCash"",""SumWallet"",""SumCreditCard"",""SumBonus"",""CodeCreditCard"",""NumberSlip"",""NumberReceiptPOS"",""AdditionN1"",""AdditionN2"",""AdditionN3"",""AdditionC1"",""AdditionD1"",""IdWorkplaceRefund"",""CodePeriodRefund"",""CodeReceiptRefund"",""DateCreate"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @DateReceipt, @TypeReceipt, @CodeClient, @CodePattern,@NumberCashier, @StateReceipt, @NumberReceipt, @NumberOrder, @SumFiscal, @SumReceipt, @VatReceipt, @PercentDiscount, @SumDiscount, @SumRest, @SumCash, @SumWallet, @SumCreditCard, @SumBonus, @CodeCreditCard, @NumberSlip, @NumberReceiptPOS, @AdditionN1, @AdditionN2, @AdditionN3, @AdditionC1, @AdditionD1, @IdWorkplaceRefund, @CodePeriodRefund, @CodeReceiptRefund, @DateCreate, @UserCreate);";
            try
            {
                Con.Execute(SqlDelete.Replace("TABLE", "Receipt"), pR, Transaction);
                Con.Execute(SQL, pR, Transaction);

                SQL = @"insert into ""ReceiptWares"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""CodeWares"",""CodeUnit"",""Order"",""Sort"",""Quantity"",""Price"",""PriceDealer"",""Sum"",""SumDiscount"",""SumWallet"",""SumBonus"",""TypeVat"",""Priority"",""TypePrice"",""ParPrice1"",""ParPrice2"",""ParPrice3"",""BarCode2Category"",""ExciseStamp"",""QR"",""RefundedQuantity"",""FixWeight"",""FixWeightQuantity"",""Description"",""AdditionN1"",""AdditionN2"",""AdditionN3"",""AdditionC1"",""AdditionD1"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @CodeWares, @CodeUnit, @Order, @Sort, @Quantity, @Price, @PriceDealer, @Sum, @SumDiscount, @SumWallet, @SumBonus, @TypeVat, @Priority, @TypePrice, @ParPrice1, @ParPrice2, @ParPrice3, @BarCode2Category, @ExciseStamp, @QR, @RefundedQuantity, @FixWeight, @FixWeightQuantity, @Description, @AdditionN1, @AdditionN2, @AdditionN3, @AdditionC1, @AdditionD1,  @UserCreate);";
                Con.Execute(SqlDelete.Replace("TABLE", "ReceiptWares"), pR, Transaction);
                BulkExecuteNonQuery<ReceiptWares>(SQL, pR.Wares, Transaction);

                SQL = @"insert into ""ReceiptPayment"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""TypePay"",""CodeBank"",""SumPay"",""Rest"",""SumExt"",""NumberTerminal"",""NumberReceipt"",""CodeAuthorization"",""NumberSlip"",""NumberCard"",""PosPaid"",""PosAddAmount"",""CardHolder"",""IssuerName"",""Bank"",""TransactionId"",""TransactionStatus"",""DateCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @TypePay, @CodeBank, @SumPay, @Rest, @SumExt, @NumberTerminal, @NumberReceipt, @CodeAuthorization, @NumberSlip, @NumberCard, @PosPaid, @PosAddAmount, @CardHolder, @IssuerName, @Bank, @TransactionId, @TransactionStatus, @DateCreate);";
                Con.Execute(SqlDelete.Replace("TABLE", "ReceiptPayment"), pR, Transaction);
                BulkExecuteNonQuery<Payment>(SQL, pR.Payment, Transaction);

                SQL = @"insert into ""Log"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""TypePay"",""NumberOperation"",""FiscalNumber"",""TypeOperation"",""SUM"",""SumRefund"",""TypeRRO"",""JSON"",""TextReceipt"",""Error"",""CodeError"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @TypePay, @NumberOperation, @FiscalNumber, @TypeOperation, @SUM, @SumRefund, @TypeRRO, @JSON, @TextReceipt, @Error, @CodeError, @UserCreate);";
                Con.Execute(SqlDelete.Replace("TABLE", "Log"), pR, Transaction);
                BulkExecuteNonQuery<LogRRO>(SQL, pR.LogRROs, Transaction);

                SQL = @"insert into ""ReceiptEvent"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdGUID"",""MobileDeviceIdGUID"",""ProductName"",""EventType"",""EventName"",""ProductWeight"",""ProductConfirmedWeight"",""UserIdGUID"",""UserName"",""CreatedAt"",""ResolvedAt"",""RefundAmount"",""FiscalNumber"",""SumFiscal"",""PaymentType"",""TotalAmount"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdGUID, @MobileDeviceIdGUID, @ProductName, @EventType, @EventName, @ProductWeight, @ProductConfirmedWeight, @UserIdGUID, @UserName, @CreatedAt, @ResolvedAt, @RefundAmount, @FiscalNumber, @SumFiscal, @PaymentType, @TotalAmount);";
                Con.Execute(SqlDelete.Replace("TABLE", "ReceiptEvent"), pR, Transaction);
                BulkExecuteNonQuery<ReceiptEvent>(SQL, pR.ReceiptEvent, Transaction);

                SQL = @"insert into ""WaresReceiptPromotion"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"",""CodeUnit"",""Quantity"",""TypeDiscount"",""Sum"",""CodePS"",""NumberGroup"",""BarCode2Category"",""TypeWares"") 
 values (@IdWorkplace, @CodePeriod,@CodeReceipt, @CodeWares, @CodeUnit, @Quantity, @TypeDiscount, @Sum, @CodePS, @NumberGroup, @BarCode2Category, @TypeWares);";
                Con.Execute(SqlDelete.Replace("TABLE", "WaresReceiptPromotion"), pR, Transaction);
                foreach (var el in pR.Wares)
                {
                    BulkExecuteNonQuery<WaresReceiptPromotion>(SQL, el.ReceiptWaresPromotions, Transaction);
                }

                Transaction.Commit();

                Con.Execute($@"update ""LogInput"" set ""State""=1 where ""Id""={Id}");
            }
            catch (Exception e)
            {
                Transaction.Rollback();
                Con.Execute($@"update ""LogInput"" set ""State"" =1, ""CodeError"" = -1, ""Error"" = @Error where ""Id""=@Id ", new { Id = Id, Error = e.Message });
            }
            return null;
        }


        public int BulkExecuteNonQuery<T>(string pQuery, IEnumerable<T> pData, NpgsqlTransaction pT)
        {
            //using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    if (pData != null)
                        foreach (var el in pData)
                            Con.Execute(pQuery, el, pT);
                }
                catch (Exception ex)
                {
                    throw new Exception("BulkExecuteNonQuery =>" + ex.Message, ex);
                }
                return pData?.Count() ?? 0;
            }
        }

        public ExciseStamp CheckExciseStamp(ExciseStamp pES)
        {
            var res = Con.Query<ExciseStamp>(@"select * from ExciseStamp where ""Stamp""=@Stamp");
            //""IdWorkplace"" = @IdWorkplace and ""CodePeriod"" =@CodePeriod and  ""CodeReceipt""=@CodeReceipt and ""CodeWares""=@CodeWares");
            if (res == null || !res.Any())
            {
                Con.Execute(@"insert into ""ExciseStamp"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"",""State"",""Stamp"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @State, @Stamp,  @UserCreate);");
                return null;
            }
            return res.FirstOrDefault();

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
           LEFT JOIN (SELECT MIN(obj_cat_RRef) AS obj_cat_RRef, roc.doc_RRRef FROM dbo.v1c_reg_obj_cat roc WHERE roc.doc_type_RTRef = 0x00000176 AND roc.obj_cat_RRef IN (0x80CA000C29F3389511E770430448F861,0x80CA000C29F3389511E7704404BCB2CE) group by doc_RRRef ) roc 
                    ON roc.doc_RRRef =pg._IDRRef  
           JOIN dbo.v1c_reg_promotion_gal pr ON pg._IDRRef=pr.doc_RRef       
           JOIN dbo.v1c_dim_subdivision sd ON pr.subdivision_RRef =sd.subdivision_RRef
           JOIN dbo.V_WAREHOUSE wh ON sd.subdivision_RRef=wh.subdivision_RRef AND wh.Code={pCodeWarehouse}
    WHERE DATEADD(YEAR,2000,GETDATE()) BETWEEN pg._Fld11664 AND pg._Fld11665";
            return Con.Query<Doc>(SQL);
        }

        public IEnumerable<DocWares> GetPromotionData(string pNumberDoc)
        {
            string SQL = $@"SELECT pg._Number AS NumberDoc, try_convert(int,nom.code) CodeWares,pgn._LineNo11667 AS ""order""
       from [utppsu].[dbo].[_Document374]  pg
       JOIN [utppsu].[dbo].[_Document374_VT11666]  pgn ON pg._IDRRef=pgn._Document374_IDRRef
       JOIN dbo.v1c_dim_nomen nom ON nom.IDRRef = pgn._Fld11668RRef
       WHERE pg._Number={pNumberDoc} AND Year(pg._Date_Time)=4023";
            return Con.Query<DocWares>(SQL);
        }
    }
}
