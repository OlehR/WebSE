﻿using Npgsql;
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
using System.Security.Cryptography;
using WebSE.Mobile;
using System.Drawing;

namespace WebSE
{
    public enum eTypeSend
    {
        Send1C,
        SendSparUkraine,
        SendBukovel
    }

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
                string Json = pR.ToJson(); //JsonSerializer.Serialize(pR, options);

                Id = con.ExecuteScalar<int>(@"insert into ""LogInput""(""IdWorkplace"", ""CodePeriod"", ""CodeReceipt"", ""JSON"",""IsSendSparUkraine"") values (@IdWorkplace, @CodePeriod, @CodeReceipt, @JSON,@IsSendSparUkraine) RETURNING ""Id""",
                                           new { pR.IdWorkplace, pR.CodePeriod, pR.CodeReceipt, JSON = new JsonParameter(Json), IsSendSparUkraine = pR.CodeClient < 0 ? 0 : 1 });
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

        public void SaveReceipt(Receipt pR, int pId = 0)
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
                    string SQL = @"insert into ""Receipt"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""DateReceipt"",""TypeReceipt"",""CodeClient"",""CodePattern"",""NumberCashier"",""StateReceipt"",""NumberReceipt"",""NumberOrder"",""SumFiscal"",""SumReceipt"",""VatReceipt"",""PercentDiscount"",""SumDiscount"",""SumRest"",""SumCash"",""SumWallet"",""SumCreditCard"",""SumBonus"",""CodeCreditCard"",""NumberSlip"",""NumberReceiptPOS"",""AdditionN1"",""AdditionN2"",""AdditionN3"",""AdditionC1"",""AdditionD1"",""IdWorkplaceRefund"",""CodePeriodRefund"",""CodeReceiptRefund"",""DateCreate"",""UserCreate"",""NumberReceipt1C"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @DateReceipt, @TypeReceipt, @CodeClient, @CodePattern,@NumberCashier, @StateReceipt, @NumberReceipt, @NumberOrder, @SumFiscal, @SumReceipt, @VatReceipt, @PercentDiscount, @SumDiscount, @SumRest, @SumCash, @SumWallet, @SumCreditCard, @SumBonus, @CodeCreditCard, @NumberSlip, @NumberReceiptPOS, @AdditionN1, @AdditionN2, @AdditionN3, @AdditionC1, @AdditionD1, @IdWorkplaceRefund, @CodePeriodRefund, @CodeReceiptRefund, @DateCreate, @UserCreate,@NumberReceipt1C);";

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

                    SQL = @"insert into ""ReceiptEvent"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""ProductName"",""EventType"",""EventName"",""ProductWeight"",""ProductConfirmedWeight"",""UserName"",""CreatedAt"",""ResolvedAt"",""RefundAmount"",""FiscalNumber"",""SumFiscal"",""PaymentType"",""TotalAmount"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @ProductName, @EventType, @EventName, @ProductWeight, @ProductConfirmedWeight,  @UserName, @CreatedAt, @ResolvedAt, @RefundAmount, @FiscalNumber, @SumFiscal, @PaymentType, @TotalAmount);";
                    con.Execute(SqlDelete.Replace("TABLE", "ReceiptEvent"), pR, Transaction);
                    BulkExecuteNonQuery<ReceiptEvent>(SQL, pR.ReceiptEvent, Transaction);

                    SQL = @"insert into ""WaresReceiptPromotion"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"",""CodeUnit"",""Quantity"",""TypeDiscount"",""Sum"",""CodePS"",""NumberGroup"",""BarCode2Category"",""TypeWares"") 
 values (@IdWorkplace, @CodePeriod,@CodeReceipt, @CodeWares, @CodeUnit, @Quantity, @TypeDiscount, @Sum, @CodePS, @NumberGroup, @BarCode2Category, @TypeWares);";
                    con.Execute(SqlDelete.Replace("TABLE", "WaresReceiptPromotion"), pR, Transaction);
                    foreach (var el in pR.Wares)
                    {
                        BulkExecuteNonQuery<WaresReceiptPromotion>(SQL, el.ReceiptWaresPromotions, Transaction);
                    }
                    if (pId != 0)
                        con.Execute($@"update ""LogInput"" set ""State""=1 where ""Id""={pId}", Transaction);
                    Transaction.Commit();

                }
                catch (Exception e)
                {
                    Transaction?.Rollback();
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + $"Id=>{pId} ProcessID=> {con.ProcessID}", e);
                    if (pId != 0)
                        con?.Execute($@"update ""LogInput"" set ""State"" =-1, ""CodeError"" = -1, ""Error"" = @Error where ""Id""=@Id ", new { Id = pId, Error = e.Message });
                }
                finally
                {
                    con?.Close();
                    con?.Dispose();
                }
            });
        }

        public ExciseStamp CheckExciseStamp(ExciseStamp pES, bool IsDelete = false)
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            IEnumerable<ExciseStamp> res = null;

            try
            {
                if (IsDelete)
                    res = con.Query<ExciseStamp>(@"select * from ""ExciseStamp"" where ""Stamp""=@Stamp", pES);
                else
                    con.Execute(@"delete from ""ExciseStamp"" where ""Stamp""=@Stamp", pES);
                // or (""IdWorkplace"" = @IdWorkplace and ""CodePeriod"" =@CodePeriod and  ""CodeReceipt""=@CodeReceipt and ""CodeWares""=@CodeWares") );
                if (res == null || !res.Any())
                {
                    con.Execute(@"insert into ""ExciseStamp"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"",""State"",""Stamp"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @State, @Stamp,  @UserCreate);", pES);
                    return null;
                }
                return res.FirstOrDefault();
            }
            catch (Exception e) { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + $"{pES.ToJson()},IsDelete=>{IsDelete}", e); }
            finally
            {
                con?.Close();
                con?.Dispose();
            }
            return null;
        }

        public bool ReceiptSetSend(int pId, eTypeSend pTypeSend = eTypeSend.Send1C)
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return false;

            try
            {
                string SQL = $@"update ""LogInput"" set ""Is{pTypeSend}""=1 where ""Id""={pId}";
                con.Execute(SQL);
                return true;
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                return false;
            }
            finally
            {
                con?.Close();
                con?.Dispose();
            }
        }

        public IEnumerable<LogInput> GetNeedSend(eTypeSend pTypeSend = eTypeSend.Send1C, int pLimit = 0) //string pListIdWorkPlace,
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = $@"select * from ""LogInput"" where {(pTypeSend == eTypeSend.SendBukovel ? @"""IdWorkplace"" in (104,105) and" : "")} ""Is{pTypeSend}""=0 and ""CodePeriod"" >= cast(to_char(current_timestamp+INTERVAL '-2 DAY', 'YYYYMMDD')as int)  and ""DateCreate"" +INTERVAL '2 Minutes'<CURRENT_TIMESTAMP {(pLimit > 0 ? $"limit " + pLimit.ToString() : "")}";//and ""IdWorkplace"" in ({pListIdWorkPlace})
                    return con.Query<LogInput>(SQL);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }

        public void DeleteExciseStamp(IdReceipt pIdR)
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    con.Execute(@"delete from ""ExciseStamp"" where ""State""=0 and ""IdWorkplace"" = @IdWorkplace and ""CodePeriod"" =@CodePeriod and  ""CodeReceipt""=@CodeReceipt", pIdR);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
                finally { con?.Close(); con?.Dispose(); }
        }

        public void InsertClientData(ClientData pCD)
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    con.Execute(@"delete from public.""ClientData"" where ""CodeClient"" = @CodeClient  or (""TypeData"" = @TypeData and ""Data""=@Data)", pCD);
                    con.Execute(@"insert into public.""ClientData"" (""TypeData"",""CodeClient"", ""Data"") values (@TypeData,@CodeClient,@Data)", pCD);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
                finally { con?.Close(); con?.Dispose(); }
        }

        public string GetBarCode(long pCodeClient)
        {
            string res = null;
            NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    res = con.ExecuteScalar<string>($@"select ""Data"" from public.""ClientData"" where ""CodeClient"" = {pCodeClient}  and ""TypeData"" = 1");
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                }
                finally { con?.Close(); con?.Dispose(); }
            return res;
        }


        public LogInput GetReceipt(IdReceipt pIdR) //string pListIdWorkPlace,
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = $@"select * from ( select * 	
,rank() OVER (PARTITION BY ""IdWorkplace"",""CodePeriod"",""CodeReceipt"" ORDER BY ""DateCreate"" DESC) as nn
from public.""LogInput""  where ""IdWorkplace""={pIdR.IdWorkplace} and ""CodePeriod""={pIdR.CodePeriod} and ""CodeReceipt""={pIdR.CodeReceipt}) where nn=1";

                    var r = con.Query<LogInput>(SQL);
                    if (r.Any() == true)
                        return r.FirstOrDefault();
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }

        public IEnumerable<LogInput> GetReceipts(IdReceipt pIdR) //string pListIdWorkPlace,
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = $@"select * from ( select * 	
,rank() OVER (PARTITION BY ""IdWorkplace"",""CodePeriod"",""CodeReceipt"" ORDER BY ""DateCreate"" DESC) as nn
from public.""LogInput""  where ""IdWorkplace""=case when {pIdR.IdWorkplace} =0 then ""IdWorkplace"" else {pIdR.IdWorkplace} end
        and ""CodePeriod""=case when {pIdR.CodePeriod}=0 then ""CodePeriod"" else {pIdR.CodePeriod} end  
        and ""CodeReceipt"" = case when {pIdR.CodeReceipt}=0 then ""CodeReceipt"" else {pIdR.CodeReceipt} end   ) where nn=1";

                    var r = con.Query<LogInput>(SQL);
                    return r;
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }

        public IEnumerable<LogInput> GetBadReceipts(int pCodePeriod) //string pListIdWorkPlace,
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = $@"	select l.""IdWorkplace"",l.""CodePeriod"",l.""CodeReceipt"",TypeReceipt ,l.Sum,r.Sum, l.SumWallet  ,l.SumDiscount,r.SumDiscount,r.""CodeReceipt"",""JSON""  from
	(select * from (
	SELECT ""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""JSON"",(""JSON""->'TypeReceipt')::TEXT::int as TypeReceipt,(""JSON""->'SumReceipt')::text::NUMERIC as Sum,
		(""JSON""->'SumDiscount')::text::NUMERIC as SumDiscount,(""JSON""->'SumWallet')::text::NUMERIC as SumWallet, ""DateCreate""
	, rank() OVER (PARTITION BY ""IdWorkplace"",""CodePeriod"",""CodeReceipt"" ORDER BY ""DateCreate"" DESC) as nn
FROM public.""LogInput"" l
where l.""CodePeriod"">={pCodePeriod} --and l.""IdWorkplace""=1 
		) d where d.nn=1) as l
		left join 	
		(
		SELECT r.""IdWorkplace"",r.""CodePeriod"",r.""CodeReceipt"", sum(rw.""Sum"") as sum, sum(rw.""SumDiscount"") as SumDiscount
FROM public.""Receipt"" r
	join public.""ReceiptWares"" rw on  r.""CodePeriod"" = rw.""CodePeriod"" and rw.""CodeReceipt"" = r.""CodeReceipt"" and rw.""IdWorkplace"" = r.""IdWorkplace""
	
	where rw.""CodeWares"" <> 163516 and r.""CodePeriod"">={pCodePeriod} --and r.""IdWorkplace""=1
	group by r.""IdWorkplace"",r.""CodePeriod"",r.""CodeReceipt""
		) as r on l.""CodePeriod""=r.""CodePeriod"" and l.""IdWorkplace""=r.""IdWorkplace"" and l.""CodeReceipt""=r.""CodeReceipt""
		where (COALESCE(round(l.Sum,2),0)<>COALESCE(r.Sum,0) or COALESCE(l.SumDiscount,0)<>COALESCE(r.SumDiscount,0))
		and TypeReceipt<>-1;";

                    var r = con.Query<LogInput>(SQL);
                    return r;
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }


        public IEnumerable<IdReceipt> GetIdReceiptsQuery(string pSQL) //string pListIdWorkPlace,
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    var r = con.Query<LogInput>(pSQL);
                    return r;
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }

        public IEnumerable<LogInput> GetReceipts(DateTime pBegin, DateTime pEnd, int pLimit) //string pListIdWorkPlace,
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = $@"select * from ""LogInput"" where ""DateCreate"" between @pBegin and @pEnd";
                    return con.Query<LogInput>(SQL, new { pBegin, pEnd });
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }

        
    }
}
