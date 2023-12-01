using Npgsql;
using Dapper;
using ModelMID;
using System.Collections.Generic;
using System;
using System.Linq;
using ModelMID.DB;
using System.Text.Json.Serialization;
using System.Text.Json;
using NpgsqlTypes;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Utils;

namespace WebSE
{
    public class JsonParameter : SqlMapper.ICustomQueryParameter
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
        string PGInit;
        public Postgres()
        {
            try
            {
                PGInit = Startup.Configuration.GetValue<string>("PGInit");
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, "Postgres.Init");
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public int BulkExecuteNonQuery<T>(string pQuery, IEnumerable<T> pData, NpgsqlTransaction pT)
        {
            //using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    if (pData != null)
                        foreach (var el in pData)
                            pT.Connection.Execute(pQuery, el, pT);
                }
                catch (Exception ex)
                {
                    throw new Exception("BulkExecuteNonQuery =>" + ex.Message, ex);
                }
                return pData?.Count() ?? 0;
            }
        }

        NpgsqlConnection GetConnect()
        {
            NpgsqlConnection Connection = null;
            try
            {
                Connection = new NpgsqlConnection(connectionString: PGInit);
                Connection.Open();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return null;
            }
            return Connection;
        }

        public int SaveLogReceipt(Receipt pR)
        {
            int Id = -1;
            NpgsqlConnection con = GetConnect();
            if (con == null) return Id;

            try
            {
                JsonSerializerOptions options = new()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                string Json = JsonSerializer.Serialize(pR, options);

                Id = con.ExecuteScalar<int>(@"insert into ""LogInput""(""IdWorkplace"", ""CodePeriod"", ""CodeReceipt"", ""JSON"") values (@IdWorkplace, @CodePeriod, @CodeReceipt, @JSON) RETURNING ""Id""",
                                           new { pR.IdWorkplace, pR.CodePeriod, pR.CodeReceipt, JSON = new JsonParameter(Json) });
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            finally
            {
                con?.Close();
            }
            return Id;

        }

        public void SaveReceipt(Receipt pR, int pId)
        {
            _ = Task.Run(() =>
            {
                NpgsqlConnection con;
                NpgsqlTransaction Transaction;
                try
                {
                    con = new NpgsqlConnection(connectionString: PGInit);
                    con.Open();
                    Transaction = con.BeginTransaction();
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return;
                }

                try
                {
                    string SqlDelete = @"delete from ""TABLE"" where  ""IdWorkplace"" = @IdWorkplace and ""CodePeriod"" =@CodePeriod and  ""CodeReceipt""=@CodeReceipt";
                    string SQL = @"insert into ""Receipt"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""DateReceipt"",""TypeReceipt"",""CodeClient"",""CodePattern"",""NumberCashier"",""StateReceipt"",""NumberReceipt"",""NumberOrder"",""SumFiscal"",""SumReceipt"",""VatReceipt"",""PercentDiscount"",""SumDiscount"",""SumRest"",""SumCash"",""SumWallet"",""SumCreditCard"",""SumBonus"",""CodeCreditCard"",""NumberSlip"",""NumberReceiptPOS"",""AdditionN1"",""AdditionN2"",""AdditionN3"",""AdditionC1"",""AdditionD1"",""IdWorkplaceRefund"",""CodePeriodRefund"",""CodeReceiptRefund"",""DateCreate"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @DateReceipt, @TypeReceipt, @CodeClient, @CodePattern,@NumberCashier, @StateReceipt, @NumberReceipt, @NumberOrder, @SumFiscal, @SumReceipt, @VatReceipt, @PercentDiscount, @SumDiscount, @SumRest, @SumCash, @SumWallet, @SumCreditCard, @SumBonus, @CodeCreditCard, @NumberSlip, @NumberReceiptPOS, @AdditionN1, @AdditionN2, @AdditionN3, @AdditionC1, @AdditionD1, @IdWorkplaceRefund, @CodePeriodRefund, @CodeReceiptRefund, @DateCreate, @UserCreate);";

                    con.Execute(SqlDelete.Replace("TABLE", "Receipt"), pR, Transaction);
                    con.Execute(SQL, pR, Transaction);

                    SQL = @"insert into ""ReceiptWares"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""CodeWares"",""CodeUnit"",""Order"",""Sort"",""Quantity"",""Price"",""PriceDealer"",""Sum"",""SumDiscount"",""SumWallet"",""SumBonus"",""TypeVat"",""Priority"",""TypePrice"",""ParPrice1"",""ParPrice2"",""ParPrice3"",""BarCode2Category"",""ExciseStamp"",""QR"",""RefundedQuantity"",""FixWeight"",""FixWeightQuantity"",""Description"",""AdditionN1"",""AdditionN2"",""AdditionN3"",""AdditionC1"",""AdditionD1"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @CodeWares, @CodeUnit, @Order, @Sort, @Quantity, @Price, @PriceDealer, @Sum, @SumDiscount, @SumWallet, @SumBonus, @TypeVat, @Priority, @TypePrice, @ParPrice1, @ParPrice2, @ParPrice3, @BarCode2Category, @ExciseStamp, @QR, @RefundedQuantity, @FixWeight, @FixWeightQuantity, @Description, @AdditionN1, @AdditionN2, @AdditionN3, @AdditionC1, @AdditionD1,  @UserCreate);";
                    con.Execute(SqlDelete.Replace("TABLE", "ReceiptWares"), pR, Transaction);
                    BulkExecuteNonQuery<ReceiptWares>(SQL, pR.Wares, Transaction);

                    SQL = @"insert into ""ReceiptPayment"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""TypePay"",""CodeBank"",""SumPay"",""Rest"",""SumExt"",""NumberTerminal"",""NumberReceipt"",""CodeAuthorization"",""NumberSlip"",""NumberCard"",""PosPaid"",""PosAddAmount"",""CardHolder"",""IssuerName"",""Bank"",""TransactionId"",""TransactionStatus"",""DateCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @TypePay, @CodeBank, @SumPay, @Rest, @SumExt, @NumberTerminal, @NumberReceipt, @CodeAuthorization, @NumberSlip, @NumberCard, @PosPaid, @PosAddAmount, @CardHolder, @IssuerName, @Bank, @TransactionId, @TransactionStatus, @DateCreate);";
                    con.Execute(SqlDelete.Replace("TABLE", "ReceiptPayment"), pR, Transaction);
                    BulkExecuteNonQuery<Payment>(SQL, pR.Payment, Transaction);

                    SQL = @"insert into ""Log"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""TypePay"",""NumberOperation"",""FiscalNumber"",""TypeOperation"",""SUM"",""SumRefund"",""TypeRRO"",""JSON"",""TextReceipt"",""Error"",""CodeError"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @TypePay, @NumberOperation, @FiscalNumber, @TypeOperation, @SUM, @SumRefund, @TypeRRO, @JSON, @TextReceipt, @Error, @CodeError, @UserCreate);";
                    con.Execute(SqlDelete.Replace("TABLE", "Log"), pR, Transaction);
                    BulkExecuteNonQuery<LogRRO>(SQL, pR.LogRROs, Transaction);

                    SQL = @"insert into ""ReceiptEvent"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdGUID"",""MobileDeviceIdGUID"",""ProductName"",""EventType"",""EventName"",""ProductWeight"",""ProductConfirmedWeight"",""UserIdGUID"",""UserName"",""CreatedAt"",""ResolvedAt"",""RefundAmount"",""FiscalNumber"",""SumFiscal"",""PaymentType"",""TotalAmount"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdGUID, @MobileDeviceIdGUID, @ProductName, @EventType, @EventName, @ProductWeight, @ProductConfirmedWeight, @UserIdGUID, @UserName, @CreatedAt, @ResolvedAt, @RefundAmount, @FiscalNumber, @SumFiscal, @PaymentType, @TotalAmount);";
                    con.Execute(SqlDelete.Replace("TABLE", "ReceiptEvent"), pR, Transaction);
                    BulkExecuteNonQuery<ReceiptEvent>(SQL, pR.ReceiptEvent, Transaction);

                    SQL = @"insert into ""WaresReceiptPromotion"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"",""CodeUnit"",""Quantity"",""TypeDiscount"",""Sum"",""CodePS"",""NumberGroup"",""BarCode2Category"",""TypeWares"") 
 values (@IdWorkplace, @CodePeriod,@CodeReceipt, @CodeWares, @CodeUnit, @Quantity, @TypeDiscount, @Sum, @CodePS, @NumberGroup, @BarCode2Category, @TypeWares);";
                    con.Execute(SqlDelete.Replace("TABLE", "WaresReceiptPromotion"), pR, Transaction);
                    foreach (var el in pR.Wares)
                    {
                        BulkExecuteNonQuery<WaresReceiptPromotion>(SQL, el.ReceiptWaresPromotions, Transaction);
                    }
                    con.Execute($@"update ""LogInput"" set ""State""=1 where ""Id""={pId}", Transaction);
                    Transaction.Commit();

                }
                catch (Exception e)
                {
                    Transaction?.Rollback();
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + $"Id=>{pId} ProcessID=> {con.ProcessID}", e);
                    con?.Execute($@"update ""LogInput"" set ""State"" =-1, ""CodeError"" = -1, ""Error"" = @Error where ""Id""=@Id ", new { pId, Error = e.Message });
                }
                finally
                {
                    con?.Close();
                }
            });
        }

        public ExciseStamp CheckExciseStamp(ExciseStamp pES)
        {
            NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            IEnumerable<ExciseStamp> res;            

            try
            {
                res = con.Query<ExciseStamp>(@"select * from ""ExciseStamp"" where ""Stamp""=@Stamp", pES);
                //""IdWorkplace"" = @IdWorkplace and ""CodePeriod"" =@CodePeriod and  ""CodeReceipt""=@CodeReceipt and ""CodeWares""=@CodeWares");
                if (res == null || !res.Any())
                {
                    con.ExecuteAsync(@"insert into ""ExciseStamp"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"",""State"",""Stamp"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @State, @Stamp,  @UserCreate);", pES);
                    return null;
                }
                return res.FirstOrDefault();
            }
            catch (Exception e) { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e); }
            finally { con?.Close(); }
            return null;
        }

        public bool ReceiptIsSend1C(int pId)
        {
            NpgsqlConnection con = GetConnect();
            if (con == null)  return false; 

            try
            {
                string SQL = $@"update ""LogInput"" set ""IsSend1C""=1 where ""Id""={pId}";
                con.Execute(SQL);
                return true;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return false;
            }
            finally { con?.Close(); }
        }       
    }
}
