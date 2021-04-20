using System;
using System.Collections.Generic;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Dapper;
using System.Data.OracleClient;
using System.Data;
using Oracle.ManagedDataAccess.Types;

namespace WebSE
{
    public class Oracle
    {
        OracleConnection connection = null;
        //OracleTransaction transaction = null;
        public Oracle(login pLogin)
        {
            string varConectionString = $"Data Source = VOPAK_NEW; User Id = {pLogin.Login}; Password={pLogin.PassWord};";
            connection = new OracleConnection(varConectionString);
            //connection.Open();
        }


        public string ExecuteApi(string p)
            {
            var cmd = new OracleCommand();
            cmd.Connection = connection;
            cmd.CommandText = "c.web.Api";
            cmd.CommandType = CommandType.StoredProcedure;
            string res = new string("");
            
            
            cmd.Parameters.Add( "res",  OracleDbType.Clob, res,ParameterDirection.ReturnValue);
            cmd.Parameters.Add("parData", OracleDbType.Clob, p, ParameterDirection.Input);
            cmd.Parameters.Add("is_utf8", OracleDbType.Int64, (object)1, ParameterDirection.Input);

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                return "{ \"State\": -1, \"TextError\":\""+ e.Message+" \" }";
            }
            
            OracleClob aa = (OracleClob) cmd.Parameters["res"].Value;
            res=aa.Value.ToString();            
            cmd.Connection.Close();
            return res;            
        }
                
    }
}
