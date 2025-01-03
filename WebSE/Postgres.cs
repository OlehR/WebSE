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
using System.Security.Cryptography;
using WebSE.Mobile;
using System.Drawing;
using Npgsql.Internal.Postgres;
using SharedLib;
using System.Xml.Linq;
using WebSE;
using System.Diagnostics;
using System.Text;

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
            _ = Task.Run(() => SaveReceiptSync( pR, pId)
            );
        }

        public string SaveReceiptSync(Receipt pR, int pId = 0)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (Global.IsNotWriteReceiptPG) return null;
            //Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();

            StringBuilder r = new StringBuilder();

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
                return null;
            }
            //stopWatch.Stop();
            //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} NpgsqlConnection{Environment.NewLine}");
            //stopWatch.Restart();
            try
            {
                string SqlDelete = @"delete from ""TABLE"" where  ""IdWorkplace"" = @IdWorkplace and ""CodePeriod"" =@CodePeriod and  ""CodeReceipt""=@CodeReceipt";
                string SQL = @"insert into ""Receipt"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""DateReceipt"",""TypeReceipt"",""CodeClient"",""CodePattern"",""NumberCashier"",""StateReceipt"",""NumberReceipt"",""NumberOrder"",""SumFiscal"",""SumReceipt"",""VatReceipt"",""PercentDiscount"",""SumDiscount"",""SumRest"",""SumCash"",""SumWallet"",""SumCreditCard"",""SumBonus"",""CodeCreditCard"",""NumberSlip"",""NumberReceiptPOS"",""AdditionN1"",""AdditionN2"",""AdditionN3"",""AdditionC1"",""AdditionD1"",""IdWorkplaceRefund"",""CodePeriodRefund"",""CodeReceiptRefund"",""DateCreate"",""UserCreate"",""NumberReceipt1C"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @DateReceipt, @TypeReceipt, @CodeClient, @CodePattern,@NumberCashier, @StateReceipt, @NumberReceipt, @NumberOrder, @SumFiscal, @SumReceipt, @VatReceipt, @PercentDiscount, @SumDiscount, @SumRest, @SumCash, @SumWallet, @SumCreditCard, @SumBonus, @CodeCreditCard, @NumberSlip, @NumberReceiptPOS, @AdditionN1, @AdditionN2, @AdditionN3, @AdditionC1, @AdditionD1, @IdWorkplaceRefund, @CodePeriodRefund, @CodeReceiptRefund, @DateCreate, @UserCreate,@NumberReceipt1C);";

                con.Execute(SqlDelete.Replace("TABLE", "Receipt"), pR, Transaction);
                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} Receipt Delete{Environment.NewLine}");
                //stopWatch.Restart();
                con.Execute(SQL, pR, Transaction);

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} Receipt{Environment.NewLine}");
                //stopWatch.Restart();

                SQL = @"insert into ""ReceiptWares"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""CodeWares"",""CodeUnit"",""Order"",""Sort"",""Quantity"",""Price"",""PriceDealer"",""Sum"",""SumDiscount"",""SumWallet"",""SumBonus"",""TypeVat"",""Priority"",""TypePrice"",""ParPrice1"",""ParPrice2"",""ParPrice3"",""BarCode2Category"",""ExciseStamp"",""QR"",""RefundedQuantity"",""FixWeight"",""FixWeightQuantity"",""Description"",""AdditionN1"",""AdditionN2"",""AdditionN3"",""AdditionC1"",""AdditionD1"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @CodeWares, @CodeUnit, @Order, @Sort, @Quantity, @Price, @PriceDealer, @Sum, @SumDiscount, @SumWallet, @SumBonus, @TypeVat, @Priority, @TypePrice, @ParPrice1, @ParPrice2, @ParPrice3, @BarCode2Category, @ExciseStamp, @QR, @RefundedQuantity, @FixWeight, @FixWeightQuantity, @Description, @AdditionN1, @AdditionN2, @AdditionN3, @AdditionC1, @AdditionD1,  @UserCreate);";
                con.Execute(SqlDelete.Replace("TABLE", "ReceiptWares"), pR, Transaction);
                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} ReceiptWares DELETE {Environment.NewLine}");
                //stopWatch.Restart();
                BulkExecuteNonQuery<ReceiptWares>(SQL, pR.Wares, Transaction);

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} ReceiptWares{Environment.NewLine}");
                //stopWatch.Restart();

                SQL = @"insert into ""ReceiptPayment"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""TypePay"",""CodeBank"",""SumPay"",""Rest"",""SumExt"",""NumberTerminal"",""NumberReceipt"",""CodeAuthorization"",""NumberSlip"",""NumberCard"",""PosPaid"",""PosAddAmount"",""CardHolder"",""IssuerName"",""Bank"",""TransactionId"",""TransactionStatus"",""DateCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @TypePay, @CodeBank, @SumPay, @Rest, @SumExt, @NumberTerminal, @NumberReceipt, @CodeAuthorization, @NumberSlip, @NumberCard, @PosPaid, @PosAddAmount, @CardHolder, @IssuerName, @Bank, @TransactionId, @TransactionStatus, @DateCreate);";
                con.Execute(SqlDelete.Replace("TABLE", "ReceiptPayment"), pR, Transaction);
                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} ReceiptPayment DELETE{Environment.NewLine}");
                //stopWatch.Restart();

                BulkExecuteNonQuery<Payment>(SQL, pR.Payment, Transaction);

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} ReceiptPayment{Environment.NewLine}");
                //stopWatch.Restart();

                SQL = @"insert into ""Log"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""IdWorkplacePay"",""TypePay"",""NumberOperation"",""FiscalNumber"",""TypeOperation"",""SUM"",""SumRefund"",""TypeRRO"",""JSON"",""TextReceipt"",""Error"",""CodeError"",""UserCreate"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @IdWorkplacePay, @TypePay, @NumberOperation, @FiscalNumber, @TypeOperation, @SUM, @SumRefund, @TypeRRO, @JSON, @TextReceipt, @Error, @CodeError, @UserCreate);";
                con.Execute(SqlDelete.Replace("TABLE", "Log"), pR, Transaction);
                BulkExecuteNonQuery<LogRRO>(SQL, pR.LogRROs, Transaction);

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} Log{Environment.NewLine}");
                //stopWatch.Restart();

                SQL = @"insert into ""ReceiptEvent"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""ProductName"",""EventType"",""EventName"",""ProductWeight"",""ProductConfirmedWeight"",""UserName"",""CreatedAt"",""ResolvedAt"",""RefundAmount"",""FiscalNumber"",""SumFiscal"",""PaymentType"",""TotalAmount"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @ProductName, @EventType, @EventName, @ProductWeight, @ProductConfirmedWeight,  @UserName, @CreatedAt, @ResolvedAt, @RefundAmount, @FiscalNumber, @SumFiscal, @PaymentType, @TotalAmount);";
                con.Execute(SqlDelete.Replace("TABLE", "ReceiptEvent"), pR, Transaction);
                BulkExecuteNonQuery<ReceiptEvent>(SQL, pR.ReceiptEvent, Transaction);

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} ReceiptEvent{Environment.NewLine}");
                //stopWatch.Restart();

                SQL = @"insert into ""WaresReceiptPromotion"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"",""CodeUnit"",""Quantity"",""TypeDiscount"",""Sum"",""CodePS"",""NumberGroup"",""BarCode2Category"",""TypeWares"") 
 values (@IdWorkplace, @CodePeriod,@CodeReceipt, @CodeWares, @CodeUnit, @Quantity, @TypeDiscount, @Sum, @CodePS, @NumberGroup, @BarCode2Category, @TypeWares);";
                con.Execute(SqlDelete.Replace("TABLE", "WaresReceiptPromotion"), pR, Transaction);
                foreach (var el in pR.Wares)
                {
                    BulkExecuteNonQuery<WaresReceiptPromotion>(SQL, el.ReceiptWaresPromotions, Transaction);
                }

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} WaresReceiptPromotion{Environment.NewLine}");
                //stopWatch.Restart();

                SQL = @"delete from public.""OneTime"" where ""IdWorkplace"" = @IdWorkplace and  ""CodePeriod"" = @CodePeriod  and  ""CodeReceipt"" = @CodeReceipt;
INSERT INTO public.""OneTime""(""IdWorkplace"", ""CodePeriod"", ""CodeReceipt"", ""CodePS"", ""TypeData"", ""CodeData"", ""State"")
	select rw.""IdWorkplace"", rw.""CodePeriod"", rw.""CodeReceipt"",rw.""ParPrice1"" as ""CodePS"", 6 as ""TypeData"",r.""CodeClient"" as ""CodeData"", 1 as ""State""
from public.""ReceiptWares"" rw 
		join public.""Receipt"" r on r.""CodePeriod"" = rw.""CodePeriod"" and rw.""CodeReceipt"" = r.""CodeReceipt"" and rw.""IdWorkplace"" = r.""IdWorkplace""
	where ""ParPrice2""<0 and r.""CodeClient"">0 and 
		rw.""IdWorkplace""=@IdWorkplace and  rw.""CodePeriod"" =@CodePeriod  and  rw.""CodeReceipt""=@CodeReceipt;";
                con.Execute(SQL, pR);

                var OneTime = pR.OneTime.Where(el => el.CodePS != 0);
                if (OneTime?.Any() == true)
                    foreach (var el in OneTime)
                        con.Execute(@"insert into public.""OneTime"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodePS"",""State"",""TypeData"",""CodeData"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodePS, @State, @TypeData, @CodeData) 
--ON CONFLICT (""CodePS"",""TypeData"",""CodeData"") DO NOTHING;"
                , el);

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} OneTime{Environment.NewLine}");
                //stopWatch.Restart();

                if (pR.ReceiptWaresPromotionNoPrice?.Any() == true)
                    foreach (var el in pR.ReceiptWaresPromotionNoPrice)
                    {
                        con.Execute(@"insert into public.""ReceiptWaresPromotionNoPrice"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodeWares"" ,""CodePS"",""TypeDiscount"",""Data"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodeWares, @CodePS, @TypeDiscount, @Data) "
                        , el);
                    }

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} ReceiptWaresPromotionNoPrice{Environment.NewLine}");
                //stopWatch.Restart();

                Transaction.Commit();
                if (pId != 0)
                    con.Execute($@"update ""LogInput"" set ""State""=1 where ""Id""={pId}");

                //stopWatch.Stop();
                //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} LogInput{Environment.NewLine}");
                //stopWatch.Restart();
            }
            catch (Exception e)
            {
                Transaction?.Rollback();
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + $"Id =>{pId} ProcessID=> {con.ProcessID}", e);
                if (pId != 0)
                    con?.Execute($@"update ""LogInput"" set ""State"" =-1, ""CodeError"" = -1, ""Error"" = @Error where ""Id""=@Id ", new { Id = pId, Error = e.Message });
            }
            finally
            {
                con?.Close();
                con?.Dispose();
            }
            //stopWatch.Stop();
            //r.Append($"{stopWatch.Elapsed.TotalMilliseconds} Close{Environment.NewLine}");
            //stopWatch.Restart();

            sw.Stop();
            r.Append($"{sw.Elapsed.TotalMilliseconds} {pR.NumberReceipt1C} Total{Environment.NewLine}");
            return r?.ToString();
        }
       

        public ExciseStamp CheckExciseStamp(ExciseStamp pES, bool IsDelete = false)
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            IEnumerable<ExciseStamp> res = null;

            try
            {
                if (!IsDelete)
                    res = con.Query<ExciseStamp>(@"select * from ""ExciseStamp"" where ""Stamp""=@Stamp and  ""State"">=0", pES);
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

        public OneTime CheckOneTime(OneTime pES, bool IsDelete = false)
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            IEnumerable<OneTime> res = null;

            try
            {
                if (!IsDelete)
                    res = con.Query<OneTime>(@"select * from public.""OneTime"" where ""CodePS""= @CodePS and ""TypeData""=@TypeData  and ""CodeData""=@CodeData ", pES);
                else
                    con.Execute(@"delete from public.""OneTime"" where ""CodePS""= @CodePS and ""TypeData""=@TypeData  and ""CodeData""=@CodeData", pES);

                if (res == null || !res.Any())
                {
                    con.Execute(@"insert into public.""OneTime"" (""IdWorkplace"",""CodePeriod"",""CodeReceipt"",""CodePS"",""State"",""TypeData"",""CodeData"") 
 values (@IdWorkplace, @CodePeriod, @CodeReceipt, @CodePS, @State, @TypeData, @CodeData);", pES);
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
                    var r = con.Query<IdReceipt>(pSQL);
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
        public IEnumerable<LogInput> GetReceiptsQuery(string pSQL) //string pListIdWorkPlace,
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

        public IEnumerable<LogInput> GetReceipts(InputParReceiptMobile pIP)
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = @"select li.* from  ""LogInput"" li
"+(pIP.reference_card > 0 || !pIP.is_all_receipt ? @"join public.""Receipt"" r on r.""CodePeriod"" = li.""CodePeriod"" and li.""CodeReceipt"" = r.""CodeReceipt"" and li.""IdWorkplace"" = r.""IdWorkplace"" ":"" )+
 (pIP.is_all_receipt ? "" : @" and r.""CodeClient""<>0") +
 (pIP.reference_card > 0 ? @" and r.""CodeClient"" = @reference_card" : "") +@"
" + (pIP.store_code?.Count() > 0 ? $@" join public.""Workplace"" Wpl on li.""IdWorkplace"" = Wpl.""IdWorkplace""  and Wpl.""CodeWarehouse"" in ({string.Join(",", pIP.store_code.Select(x => x.ToString()))})" :"") +@" 
    where LI.""DateCreate"" between @FromTZ and @ToTZ 
" + (pIP.limit > 0 ? @" order BY li.""DateCreate"" LIMIT @limit OFFSET @offset;" : "");
                    return con.Query<LogInput>(SQL, pIP);
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }

       public IEnumerable<Int64> GetOneTimePromotion(long pCodeClient)
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = @"select 0 as ""CodePS"" union all select ""CodePS"" from public.""OneTime"" ot where ot.""TypeData""=6 and ot.""CodeData""=@pCodeClient";
                    return con.Query<Int64>(SQL, new { pCodeClient });
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return null;
                }
                finally { con?.Close(); con?.Dispose(); }
            return null;
        }

        public bool DelNotUse()
        {
            using NpgsqlConnection con = GetConnect();
            if (con != null)
                try
                {
                    string SQL = @"UPDATE public.""ExciseStamp"" ES
SET ""State""=-1 
FROM (SELECT ES.*
	FROM public.""ExciseStamp"" ES 
	left join public.""LogInput"" LI on ES.""IdWorkplace""=LI.""IdWorkplace"" and ES.""CodePeriod""= LI.""CodePeriod""  and ES.""CodeReceipt""=LI.""CodeReceipt""
where ES.""CodePeriod"" between to_char(now()-interval '10 days','YYYYMMDD')::int  and  to_char(now()-interval '2 days','YYYYMMDD')::int and ES.""State""=0
and LI.""CodeReceipt"" is null ) AS LI
WHERE ES.""IdWorkplace""=LI.""IdWorkplace"" and ES.""CodePeriod""= LI.""CodePeriod""  and ES.""CodeReceipt""=LI.""CodeReceipt""";
                     return con.Execute(SQL)>0;
                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
                    return false;
                }
                finally { con?.Close(); con?.Dispose(); }
            return false;
        }


    }
}
