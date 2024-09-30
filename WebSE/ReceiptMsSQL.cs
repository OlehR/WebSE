using Dapper;
using Microsoft.Extensions.Configuration;
using ModelMID;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace WebSE
{
    public class ReceiptMsSQL
    {
        public SqlConnection connection;

        string MsSqlInit;
        public ReceiptMsSQL()
        {

            MsSqlInit = Startup.Configuration.GetValue<string>("MsSqlInit");
            connection = new SqlConnection(MsSqlInit);

        }
     
       

    }
}
