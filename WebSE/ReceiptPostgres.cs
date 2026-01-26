using Dapper;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using Npgsql;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UtilNetwork;
using Utils;
using WebSE.Controllers.ReceiptAppControllers.ReceiptAppModels;
using WebSE.RecieptModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebSE
{
    public class ReceiptPostgres
    {
        ReceiptMsSQL receiptMsSQL;
        string PGInit;
        public ReceiptPostgres()
        {

            try
            {
                PGInit = Startup.Configuration.GetValue<string>("PGInit");

            }
            catch (Exception ex)
            {
            }
             receiptMsSQL = new ReceiptMsSQL();

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

                return null;
            }
            return Connection;
        }
        public IEnumerable<WareHouse> GetAllWareHouse()
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            try
            {
                using (NpgsqlTransaction Transaction = con.BeginTransaction())
                {

                    var res = con.Query<WareHouse>(@"Select ""Code"", ""Name"" ,""Adres"",""Square"",""GLN"",""TypeWarehouse"" as typewarehouse,""NameTM"" as nametm from ""Warehouse"" where ""TypeWarehouse""=11");
                    return res;
                }
            }
            catch (Exception e) { return null; }

        }
        public UtilNetwork.Result DeleteReceipt(Receipt receipt)
        {
            return new UtilNetwork.Result();
        }
        public IEnumerable<WorkPlace> GetWorkplaces(List<int> warehousesId)
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            try
            {
                using (NpgsqlTransaction Transaction = con.BeginTransaction())
                {
                    // Початок SQL-запиту
                    var sql = @"SELECT ""IdWorkplace"", ""Name"", ""VideoCameraIP"", ""Prefix"", ""DNSName"" AS _DNSName, 
                               ""TypeWorkplace"", ""CodeWarehouse"", ""Settings"" 
                        FROM public.""Workplace""";

                    // Якщо список warehouseId не пустий, додаємо WHERE
                    if (warehousesId != null && warehousesId.Count > 0)
                    {
                        // Додаємо умову з IN для "CodeWarehouse"
                        sql += @" WHERE ""CodeWarehouse"" = ANY (@WarehouseIds) Order by ""Name""";
                    }

                    // Використовуємо Dapper для виконання запиту з параметрами
                    var res = con.Query<WorkPlace>(sql, new { WarehouseIds = warehousesId });
                    return res;
                }
            }
            catch (Exception e)
            {
                // Логування помилки або інше оброблення
                return null;
            }
        }

        public IEnumerable<ModelMID.ReceiptWares> GetAllWaresByReciept(int workPlaceId,int recieptId,int codePeriodId)
          {
              using NpgsqlConnection con = GetConnect();
              if (con == null) return null;
              try
              {
                  using (NpgsqlTransaction Transaction = con.BeginTransaction())
                  {

                      var res = con.Query<ModelMID.ReceiptWares>(@"SELECT 
    ""Articl"",
    ""CodeUKTZED"" AS ""CodeUKTZED"", 
    ""NameWares"" AS ""nameWares"",
    ""Price"",
    ""Quantity"",
    ""SumWallet"",
    ""SumBonus"",
    ""SumDiscount"",
    ""ReceiptWares"".""CodeWares"" AS ""codewares""
FROM public.""ReceiptWares""
JOIN public.""Ware"" 
    ON CAST(""Ware"".""CodeWares"" AS integer) = ""ReceiptWares"".""CodeWares""
WHERE 
    ""CodeReceipt"" = @recieptId 
    AND ""IdWorkplace"" = @workPlaceId 
    AND ""CodePeriod"" = @codePeriodId;

  ", new { workPlaceId = workPlaceId, recieptId= recieptId, codePeriodId= codePeriodId });
                      return res;
                  }
              }
              catch (Exception e) { return null; }
          }
        public IEnumerable<ReceiptWithNames> GetReceiptsByDate(List<int> workPlaceId, DateTime dateStart, DateTime dateEnd, ReceiptFillter fillter)
        {
            //BL bl = BL.GetBL;
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            try
            {
                using (NpgsqlTransaction Transaction = con.BeginTransaction())
                {
                    var sql = @"
   SELECT ""Receipt"".""IdWorkplace"", 
          ""Receipt"".""CodePeriod"", 
          ""Receipt"".""CodeReceipt"", 
          ""Receipt"".""IdWorkplacePay"", 
          ""DateReceipt"", 
          ""TypeReceipt"", 
          ""CodePattern"", 
          ""NumberCashier"", 
          ""StateReceipt"", 
          ""Receipt"".""NumberReceipt"" as ""ReceiptNumber"", -- Унікальний псевдонім
          ""NumberOrder"", 
          ""SumFiscal"", 
          ""SumReceipt"", 
          ""VatReceipt"", 
          ""PercentDiscount"", 
          ""SumDiscount"", 
          ""SumRest"", 
          ""SumCash"", 
          ""SumWallet"", 
          ""SumCreditCard"", 
          ""SumBonus"", 
          ""CodeCreditCard"", 
          ""Receipt"".""NumberSlip"", 
          ""NumberReceiptPOS"", 
          ""AdditionN1"", 
          ""AdditionN2"", 
          ""AdditionN3"", 
          ""AdditionC1"", 
          ""AdditionD1"", 
          ""Receipt"".""IdWorkplaceRefund"", 
          ""CodePeriodRefund"", 
          ""CodeReceiptRefund"", 
          ""Receipt"".""DateCreate"", 
          ""UserCreate"", 
          ""NumberReceipt1C"", 
          ""Workplace"".""Name"" as ""WarehousName"", 
          ""Warehouse"".""Name"" as ""WorkplaceName"", 
          ""Receipt"".""CodeClient"", 
          ""Client"".""NameClient"" as ""NameClient"", 
          ""Client"".""MainPhone"", 
          ""TypePay"", 
          ""CodeBank"", 
          ""SumPay"", 
          ""Rest"", 
          ""SumExt"", 
          ""NumberTerminal"", 
          ""ReceiptPayment"".""NumberReceipt"" as ""PaymentNumber"", -- Унікальний псевдонім для Payment
          ""CodeAuthorization"", 
          ""NumberCard"", 
          ""PosPaid"", 
          ""PosAddAmount"", 
          ""CardHolder"", 
          ""IssuerName"", 
          ""Bank"", 
          ""TransactionId"", 
          ""TransactionStatus""
   FROM public.""Receipt""
   JOIN ""Workplace"" ON ""Receipt"".""IdWorkplace"" = ""Workplace"".""IdWorkplace""
   LEFT JOIN ""Client"" ON ""Receipt"".""CodeClient""= ""Client"".""CodeClient""
   JOIN ""Warehouse"" ON ""Workplace"".""CodeWarehouse"" = ""Warehouse"".""Code""
   JOIN ""ReceiptPayment"" ON ""ReceiptPayment"".""CodePeriod"" = ""Receipt"".""CodePeriod"" 
         AND ""ReceiptPayment"".""CodeReceipt"" = ""Receipt"".""CodeReceipt"" 
         AND ""ReceiptPayment"".""IdWorkplace"" = ""Receipt"".""IdWorkplace""
   WHERE ""DateReceipt""::timestamp BETWEEN @dateStart AND @dateEnd
   AND ""Receipt"".""IdWorkplace"" = ANY(@workPlaceId) and ""ReceiptPayment"".""TypePay""!=5 and""ReceiptPayment"".""TypePay""!=7 ";
                    if (fillter == null)
                    {
                        var result = con.Query<ReceiptWithNames, Client, Payment, ReceiptWithNames>(
                            sql,
                            (receipt, client, payment) =>
                            {
                                receipt.Client = client;
                                receipt._Payment = new List<Payment> { payment };
                                return receipt;
                            },
                            new { workPlaceId = workPlaceId, dateStart = dateStart, dateEnd = dateEnd },
                            splitOn: "CodeClient,TypePay");

                        return result;
                    }
                    return GetReceiptsByDateFilltered( workPlaceId, dateStart, dateEnd, fillter, sql);

                }
            }
            catch (Exception e)
            {
                // Логування помилки або інша обробка
                return null;
            }
        }


        public LogRRO GetLogByReceipt(int workPlaceId, int recieptId, int codePeriodId)
          {
              using NpgsqlConnection con = GetConnect();
              if (con == null) return null;
              try
              {
                  using (NpgsqlTransaction Transaction = con.BeginTransaction())
                  {

                      var res = con.Query<LogRRO>(@"select ""TypePay"",""NumberOperation"",""FiscalNumber"", ""TypeOperation"",""SUM"",""SumRefund"",""TypeRRO"",""JSON"", ""TextReceipt"", ""Error"",""CodeError"",""UserCreate"" from ""Log"" 
  Where ""IdWorkplace""=@workPlaceId and ""CodeReceipt""=@recieptId and ""CodePeriod""= @codePeriodId

  ", new { workPlaceId = workPlaceId, recieptId = recieptId, codePeriodId = codePeriodId });
                      return res.FirstOrDefault();
                  }
              }
              catch (Exception e) { return null; }
          }
        public IEnumerable<ReceiptEvent> GetEventByReceipt(int workPlaceId, int recieptId, int codePeriodId)
          {
              using NpgsqlConnection con = GetConnect();
              if (con == null) return null;
              try
              {
                  using (NpgsqlTransaction Transaction = con.BeginTransaction())
                  {

                      var res = con.Query<ReceiptEvent>(@"select ""ProductName"",""EventType"", ""EventName"",""ProductWeight"",""ProductConfirmedWeight"" ,
  ""UserName"",""CreatedAt"",""ResolvedAt"",""RefundAmount"",""FiscalNumber"",""SumFiscal"",""PaymentType"",""TotalAmount"" from ""ReceiptEvent""
  Where ""IdWorkplace""=@workPlaceId and ""CodeReceipt""=@recieptId and ""CodePeriod""= @codePeriodId
  ", new { workPlaceId = workPlaceId, recieptId = recieptId, codePeriodId = codePeriodId });
                      return res;
                  }
              }
              catch (Exception e) { return null; }
          }
        public IEnumerable<ReceiptWithNames> GetReceiptsByDateFilltered(List<int> workPlaceId, DateTime dateStart, DateTime dateEnd ,ReceiptFillter fillter, string sql)
        {
            
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            try
            {
                using (NpgsqlTransaction Transaction = con.BeginTransaction())
                {
                    var str = sql;

                    if (fillter.isStateReceiptneeded == true)
                    {
                        str += @" AND ""StateReceipt"" = @statereceipt";
                    }
                    if (fillter.isTypeReceiptneeded == true)
                    {
                        str += @" AND ""TypeReceipt"" = @typereceipt";
                    }
                    if (fillter.HigherAmount != 0)
                    {
                        str += @" AND ""SumFiscal"" between @loweramount and @higheramount";
                    }
                    if (fillter.LowerAmount != 0)
                    {
                        str += @" AND ""SumFiscal"" >= @loweramount";
                    }
                    if (fillter.CheckDiscount == true)
                    {
                        str += @" AND ""Receipt"".""CodeClient"" != 0";
                    }
                    if (fillter.CheckIsCard == true)
                    {
                        str += @" AND ""SumCash"" = 0 AND ""SumCreditCard"" != 0";
                    }
                    if (fillter.CheckIsCash == true)
                    {
                        str += @" AND ""SumCash"" != 0 AND ""SumCreditCard"" != 0";
                    }
                    if (fillter.NumberReceipt != "0")
                    {
                        str += @" AND ""NumberReceipt"" = @numberreceipt";
                    }
                    if (fillter.NumberOrder != "0")
                    {
                        str += @" AND ""NumberOrder"" = @numberorder";
                    }
                    if (fillter.NumberReceiptPOS != -1)
                    {
                        str += @" AND ""NumberReceiptPOS"" = @numberreceiptpos";
                    }
                    if (fillter.IdWorkplacePay != 0)
                    {
                        str += @" AND ""IdWorkplacePay"" = @idworkplacepay";
                    }
                    if (fillter.UserCreate != "0")
                    {
                        str += @" AND ""UserCreate"" = @usercreate";
                    }
                    if (fillter.NumberReceipt1C != "0")
                    {
                        str += @" AND ""NumberReceipt1C"" = @numberreceipt1c";
                    }
                    if (fillter.CodeClient != -1)
                    {
                        str += @" AND ""Receipt"".""CodeClient"" = @codeclient";
                    }
                    if (fillter.bankCheck == true)
                    {
                        str += @"And ""ReceiptPayment"".""CodeBank""= @bank";
                    }
                  
                    str += ";";
                
                    var res = con.Query<ReceiptWithNames, Client, Payment, ReceiptWithNames >(str,
                        (receipt, client, payment) =>
                        {
                            receipt.Client = client;
                            receipt._Payment = new List<Payment> { payment };
                            return receipt;
                        }, new
                    {
                        workPlaceId = workPlaceId,
                        dateStart = dateStart,
                        dateEnd = dateEnd,
                        statereceipt = fillter.StateReceipt,
                        typereceipt = fillter.TypeReceipt,
                        higheramount = fillter.HigherAmount,
                        loweramount = fillter.LowerAmount,
                        numberreceipt = fillter.NumberReceipt,
                        numberorder = fillter.NumberOrder,
                        numberreceiptpos = fillter.NumberReceiptPOS, // Ensure this is a long
                        idworkplacepay = fillter.IdWorkplacePay,
                        usercreate = long.TryParse(fillter.UserCreate, out var uc) ? uc : (long?)null, // Convert UserCreate to long
                        numberreceipt1c = fillter.NumberReceipt1C,
                        codeclient = fillter.CodeClient,
                        bank=fillter.eBank
                    }, splitOn: "CodeClient,TypePay");
                   

                    return res;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public IEnumerable<Client> GetClientsByPrefix(string prefix)
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            try
            {
                using (NpgsqlTransaction Transaction = con.BeginTransaction())
                {
                    string sql = @"SELECT ""NameClient"",""CodeClient"" FROM ""Client""
                   WHERE ""NameClient"" ILIKE @prefix";
                    return con.Query<Client>(sql, new { prefix = $"{prefix}%" });
                }
            }
            catch(Exception ex) 
            {
            }
            return null;

            
        }


    }

}
