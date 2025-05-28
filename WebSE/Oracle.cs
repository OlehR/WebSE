using System;
using System.Collections.Generic;
using System.Text;
using Oracle.ManagedDataAccess.Client;
using Dapper;
//using System.Data.OracleClient;
using System.Data;
using Oracle.ManagedDataAccess.Types;
using Utils;
using BRB5.Model;
using UtilNetwork;

namespace WebSE
{
    public class Oracle
    {
        OracleConnection connection = null;
        //OracleTransaction transaction = null;
        string ConectionString="";
        public Oracle(login pLogin)
        {
            ConectionString = $"Data Source = VOPAK_NEW; User Id = {pLogin.Login}; Password={pLogin.PassWord};";
            connection = new OracleConnection(ConectionString);
            //connection.Open();
        }

        public string ExecuteApi(string p)
            {
            var cmd = new OracleCommand();
            cmd.Connection = connection;
            cmd.CommandText = "c.web.Api";
            cmd.CommandType = CommandType.StoredProcedure;
            string res = $"{{ \"State\": -1, \"TextError\":\"Rez=|>NULL\"}}";


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
                return $"{{ \"State\": -1, \"TextError\":\"{e.Message}\" }}";
            }
            
            OracleClob aa = (OracleClob) cmd.Parameters["res"].Value;
            if (aa == null || aa.Value == null)
            {
                FileLogger.WriteLogMessage($"Oracle\\ExecuteApi\\{this.ConectionString}\\{p} \\ Res=> NULL");
            }
            else
            {
                res = aa.Value.ToString();
            }
            cmd.Connection.Close();
            return res;            
        }

        Result IsConnect()
        {            
            var cmd = new OracleCommand();
            cmd.Connection = connection;
            try
            {
                cmd.Connection.Open();
                cmd.Connection.Close();
            }
            catch (Exception e)
            {
                return new Result(e);
            }
            return new Result();
        }
                
    }
}
