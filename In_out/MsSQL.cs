using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace In_out
{
    public class MsSQL
    {
        public SqlConnection connection;
        public MsSQL()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "sqlsrv2.vopak.local";
            builder.UserID = "dwreader";
            builder.Password = "DW_Reader";
            builder.InitialCatalog = "for_cubes";
            connection = new SqlConnection(builder.ConnectionString);

        }


        public bool InsertData(DbInOut pData)
        {
            var sql = @"INSERT INTO dbo.Fact_in_out (day_Id, warehouse_id, hour_id, code_zone, Type_Zone, amount) VALUES 
                    (@day_Id, @warehouse_id, @hour_id, @code_zone, @Type_Zone, @amount)";
            int r = connection.Execute(sql, pData);
            return r > 0;            
        }

        public bool ClearData(object pData)
        {
            var sql = @"Delete from dbo.Fact_in_out where day_Id between @dBegin and @dEnd";
            int r = connection.Execute(sql, pData);
            return r > 0;
        }

    }


}
