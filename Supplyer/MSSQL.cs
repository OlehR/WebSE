using Dapper;
using Supplyer.Helpers;
using Supplyer.Models.DiscountModels;
using Supplyer.Models.Enums;
using Supplyer.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Utils;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Supplyer
{
    public class MSSQL
    {
        private string _connectionString;

        public MSSQL(int timeout = 30, string connectionString = @"Server=10.1.0.22;Database=DW;Uid=dwreader;Pwd=DW_Reader;")
        {
            _connectionString = $"{connectionString};Connect Timeout={timeout};";
        }

        public Status< IEnumerable<DiscountPeriodsModel>> GetAllDiscPeriods()
        {            
            var query = @"
                SELECT
                    d._IDRRef AS doc_RRef,
                    d.[_Version] AS [version],
                    CAST(d.[_Marked] as bit) AS [is_deleted],
                    DATEADD(YEAR, -2000, d.[_Date_Time]) AS [date_time],
                    d.[_Number] AS [Number],
                    CAST(d.[_Posted] as bit) AS [is_posted],
                    CAST(d.[_Fld11661] as nvarchar(250)) AS [comment],
                    CONVERT(nchar(32), [_Fld11663RRef], 2) AS [subdivision_id],
                    CASE 
                        WHEN YEAR([_Fld11664]) < 4000 THEN CAST('2001-01-01' as date) 
                        ELSE DATEADD(YEAR, -2000, [_Fld11664]) 
                    END AS [DateStart],
                    CASE 
                        WHEN YEAR([_Fld11665]) < 4000 THEN CAST('2001-01-01' as date) 
                        ELSE DATEADD(YEAR, -2000, [_Fld11665]) 
                    END AS [DateEnd],
                    CASE 
                        WHEN roc.obj_cat_RRef = 0x80CA000C29F3389511E770430448F861 THEN 1 
                        ELSE 0 
                    END AS Is1
                FROM [utppsu].[dbo].[_Document374] d
                LEFT JOIN [utppsu].dbo._Document375_VT11673 dcd ON d._IDRRef = dcd._Fld11675RRef
                LEFT JOIN [utppsu].dbo._Document375 dc ON dc._IDRRef = dcd._Document375_IDRRef 
                    AND dc._Posted = 0x01 
                    AND DATEADD(YEAR, -2000, dc._Date_Time) < GETDATE()
                JOIN (
                    SELECT 
                        MIN(obj_cat_RRef) AS obj_cat_RRef, 
                        roc.doc_RRRef 
                    FROM dbo.v1c_reg_obj_cat roc 
                    WHERE roc.doc_type_RTRef = 0x00000176 
                        AND roc.obj_cat_RRef IN (0x80CA000C29F3389511E770430448F861, 0x80CA000C29F3389511E7704404BCB2CE) 
                    GROUP BY doc_RRRef 
                ) roc ON roc.doc_RRRef = d._IDRRef
                WHERE dc._Posted IS NULL
                    AND CASE 
                        WHEN YEAR([_Fld11664]) < 4000 THEN CAST('2001-01-01' as date) 
                        ELSE DATEADD(YEAR, -2000, [_Fld11664]) 
                    END > GETDATE()";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    return new  Status<IEnumerable<DiscountPeriodsModel>>( connection.Query<DiscountPeriodsModel>(query));
                }
                catch (Exception e)
                {
                    return new Status<IEnumerable<DiscountPeriodsModel>>(e);
                }
            }
        }

        public Status<IEnumerable<StorageAdressModel>> GetAllAdresses()
        {            
            var query = @"
            SELECT 
   distinct d._Number AS number,
  
    wh.Adres AS adress,
    STUFF((
        SELECT ', ' + wh2.Name
        FROM WAREHOUSES wh2
        WHERE wh2.Adres = wh.Adres
        FOR XML PATH(''), TYPE
    ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Name
FROM [utppsu].[dbo].[_Document374] d
LEFT JOIN [utppsu].dbo._Document375_VT11673 dcd ON d._IDRRef = dcd._Fld11675RRef
LEFT JOIN [utppsu].dbo._Document375 dc ON dc._IDRRef = dcd._Document375_IDRRef 
    AND dc._Posted = 0x01 
    AND DATEADD(YEAR, -2000, dc._Date_Time) < GETDATE()
JOIN (
    SELECT MIN(obj_cat_RRef) AS obj_cat_RRef, roc.doc_RRRef 
    FROM dbo.v1c_reg_obj_cat roc 
    WHERE roc.doc_type_RTRef = 0x00000176 
        AND roc.obj_cat_RRef IN (0x80CA000C29F3389511E770430448F861, 0x80CA000C29F3389511E7704404BCB2CE) 
    GROUP BY roc.doc_RRRef 
) roc ON roc.doc_RRRef = d._IDRRef
LEFT JOIN [utppsu].dbo._Document374_VT19053 ps ON d._IDRRef = ps._Document374_IDRRef
LEFT JOIN WAREHOUSES wh ON wh.subdivision_RRef = ps._Fld19055RRef 
    AND wh.type_warehouse = 11
WHERE dc._Posted IS NULL
    AND CASE 
        WHEN YEAR([_Fld11664]) < 4000 THEN CAST('2001-01-01' AS DATE) 
        ELSE DATEADD(YEAR, -2000, [_Fld11664]) 
    END > GETDATE()
GROUP BY 
    d._Number,
    wh.Code,
    wh.Adres;";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    return new Status<IEnumerable<StorageAdressModel>>( connection.Query<StorageAdressModel>(query));
                }
                catch (Exception e)
                {
                    return new Status<IEnumerable<StorageAdressModel>>();
                }
            }
        }
        /// <summary>
        /// Запит дістати період акції по номеру 
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public Status<DiscountPeriodsModel> GetDicountPeriodByNumber(string number)
        {
            var query = @"SELECT DATEADD(YEAR,-2000,d.[_Date_Time])    as [date_time]         
      ,d.[_Number]              as [number]       
      ,CAST(d.[_Fld11661] as nvarchar(250))    as [comment]      --Комментарий
      ,CASE WHEN YEAR([_Fld11664])<4000 THEN CAST('2001-01-01' as date) ELSE DATEADD(YEAR,-2000,[_Fld11664]) END as [datestart] --ДатаНачала
      ,CASE WHEN YEAR([_Fld11665])<4000 THEN CAST('2001-01-01' as date) ELSE DATEADD(YEAR,-2000,[_Fld11665]) END as [dateend] --ДатаОкончания     
    FROM [utppsu].[dbo].[_Document374]  d
  WHERE  d.[_Number]=@number";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var aa = connection.Query<DiscountPeriodsModel>(query, new { number }).ToList().FirstOrDefault();
                    return new Status<DiscountPeriodsModel>(aa);
                }
                catch (Exception e)
                {
                    return new Status<DiscountPeriodsModel>(e);
                }
            }
        }

        public Status<IEnumerable<StorageAdressModel>>GetAdressesByNumber(string number)
        {
            var query = @"SELECT  d._Number AS number, wh.Adres AS adress, wh.Name AS Name
FROM [utppsu].[dbo].[_Document374] d
 JOIN [utppsu].dbo._Document374_VT19053 ps ON d._IDRRef = ps._Document374_IDRRef
 JOIN WAREHOUSES wh ON wh.subdivision_RRef = ps._Fld19055RRef AND wh.type_warehouse = 11
WHERE d._Number=@number";

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    var aa= connection.Query<StorageAdressModel>(query, new { number });
                    return new Status<IEnumerable<StorageAdressModel>>( aa);
                }
                catch (Exception e)
                {
                   return new Status<IEnumerable<StorageAdressModel>> (e);
                }
            }
        }
       
    }
}
