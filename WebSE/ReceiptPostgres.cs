using Dapper;
using Microsoft.Extensions.Configuration;
using ModelMID;
using ModelMID.DB;
using Npgsql;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Linq;
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

                    var res = con.Query<WareHouse>(@"Select ""Code"", ""Name"" ,""Adres"",""Square"",""GLN"",""TypeWarehouse"" as typewarehouse,""NameTM"" as nametm from ""Warehouse""");
                    return res;
                }
            }
            catch (Exception e) { return null; }

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
                        sql += @" WHERE ""CodeWarehouse"" = ANY (@WarehouseIds)";
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
        public IEnumerable<ReceiptWithNames> GetReceiptsByDate(List<int> workPlaceId, DateTime dateSrart, DateTime dateEnd)
        {
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            try
            {
                using (NpgsqlTransaction Transaction = con.BeginTransaction())
                {
                    var sql = @"
   SELECT ""Receipt"".""IdWorkplace"", ""CodePeriod"", ""CodeReceipt"", ""IdWorkplacePay"", ""DateReceipt"", ""TypeReceipt"", 
   ""CodePattern"", ""NumberCashier"", ""StateReceipt"", ""NumberReceipt"", ""NumberOrder"", 
   ""SumFiscal"", ""SumReceipt"", ""VatReceipt"", ""PercentDiscount"", ""SumDiscount"", ""SumRest"", 
   ""SumCash"", ""SumWallet"", ""SumCreditCard"", ""SumBonus"", ""CodeCreditCard"", ""NumberSlip"", 
   ""NumberReceiptPOS"", ""AdditionN1"", ""AdditionN2"", ""AdditionN3"", ""AdditionC1"", ""AdditionD1"", 
   ""IdWorkplaceRefund"", ""CodePeriodRefund"", ""CodeReceiptRefund"", ""DateCreate"", ""UserCreate"", 
   ""NumberReceipt1C"", ""Workplace"".""Name"" as ""WarehousName"", ""Warehouse"".""Name"" as ""WorkplaceName"",
   ""Receipt"".""CodeClient"", ""Client"".""NameClient"" as NameClient, ""Client"".""MainPhone"" 
FROM public.""Receipt""
JOIN ""Workplace"" ON ""Receipt"".""IdWorkplace"" = ""Workplace"".""IdWorkplace""
JOIN ""Client"" ON ""Receipt"".""CodeClient""= ""Client"".""CodeClient""
JOIN ""Warehouse"" ON ""Workplace"".""CodeWarehouse"" = ""Warehouse"".""Code""
WHERE ""DateReceipt""::timestamp BETWEEN @dateSrart AND @dateEnd
AND ""Receipt"".""IdWorkplace"" = ANY(@workPlaceId);";

                    var result = con.Query<ReceiptWithNames, Client, ReceiptWithNames>(sql,
                        (receipt, client) =>
                        {
                            receipt.Client = client;
                            return receipt;
                        },
                        new { workPlaceId = workPlaceId, dateSrart = dateSrart, dateEnd = dateEnd },
                        splitOn: "CodeClient");

                    return result;

                }
            }
            catch (Exception e) { return null; }
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
        public IEnumerable<ReceiptWithNames> GetReceiptsByDateFilltered(ReceiptFillter fillter)
        {
            
            using NpgsqlConnection con = GetConnect();
            if (con == null) return null;
            try
            {
                using (NpgsqlTransaction Transaction = con.BeginTransaction())
                {
                    var str = @"
      SELECT ""Receipt"".""IdWorkplace"", ""CodePeriod"", ""CodeReceipt"", ""IdWorkplacePay"", ""DateReceipt"", ""TypeReceipt"", 
               ""CodePattern"", ""NumberCashier"", ""StateReceipt"", ""NumberReceipt"", ""NumberOrder"", 
             ""SumFiscal"", ""SumReceipt"", ""VatReceipt"", ""PercentDiscount"", ""SumDiscount"", ""SumRest"", 
             ""SumCash"", ""SumWallet"", ""SumCreditCard"", ""SumBonus"", ""CodeCreditCard"", ""NumberSlip"", 
             ""NumberReceiptPOS"", ""AdditionN1"", ""AdditionN2"", ""AdditionN3"", ""AdditionC1"", ""AdditionD1"", 
             ""IdWorkplaceRefund"", ""CodePeriodRefund"", ""CodeReceiptRefund"", ""DateCreate"", ""UserCreate"", 
             ""NumberReceipt1C"",""Workplace"".""Name"" as ""WarehousName"", ""Warehouse"".""Name"" as ""WorkplaceName"",
""Receipt"".""CodeClient"", ""Client"".""NameClient""
      FROM public.""Receipt""
JOIN ""Workplace"" ON ""Receipt"".""IdWorkplace"" = ""Workplace"".""IdWorkplace""
Join ""Client"" ON ""Receipt"".""CodeClient""= ""Client"".""CodeClient""
JOIN ""Warehouse"" ON ""Workplace"".""CodeWarehouse"" = ""Warehouse"".""Code""
      WHERE ""DateReceipt""::timestamp BETWEEN @dateStart AND @dateEnd
        AND ""Receipt"".""IdWorkplace"" = ANY(@workPlaceId)";

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
                   /* var result = con.Query<ReceiptWithNames, Client, ReceiptWithNames>(sql,
                        (receipt, client) =>
                        {
                            receipt.Client = client;
                            return receipt;
                        },
                        new { workPlaceId = workPlaceId, dateSrart = dateSrart, dateEnd = dateEnd },
                        splitOn: "CodeClient");*/
                    str += ";";

                    var res = con.Query<ReceiptWithNames, Client, ReceiptWithNames>(str,
                       (receipt, client) =>
                       {
                           receipt.Client = client;
                           return receipt;
                       }, new
                    {
                        workPlaceId = fillter.WorkplacesIds,
                        dateStart = fillter.Begin,
                        dateEnd = fillter.End,
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
                        codeclient = fillter.CodeClient
                    }, splitOn: "CodeClient");
                   

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
