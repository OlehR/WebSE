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
        string ConectionString = "";
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


            cmd.Parameters.Add("res", OracleDbType.Clob, res, ParameterDirection.ReturnValue);
            cmd.Parameters.Add("parData", OracleDbType.Clob, p, ParameterDirection.Input);
            cmd.Parameters.Add("is_utf8", OracleDbType.Int64, (object)1, ParameterDirection.Input);

            try
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                return $"{{ \"State\": -1, \"TextError\":\"{e.Message}\" }}";
            }

            OracleClob aa = (OracleClob)cmd.Parameters["res"].Value;
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

        public Docs LoadDocs(GetDocs pGD)
        {
            Docs res = new();
            string Sql;
            connection.Open();
            Sql = @"With T1 as (
select :CodeWarehouse as CodeWarehouse from dual
union
select wh.code_warehouse_old from dw.warehouse  wh where wh.code_warehouse=  :CodeWarehouse and wh.code_warehouse_old>0
union
select wh.CodeWarehouseLink from dw.warehouse  wh where wh.code_warehouse=  :CodeWarehouse and wh.CodeWarehouseLink>0
union
select who.Code_Warehouse_Old from dw.warehouse  wh 
join dw.warehouse  who on  wh.codewarehouselink=who.code_warehouse
where wh.code_warehouse =  :CodeWarehouse and  who.Code_Warehouse_old>0)

select to_char(os.date_warehouse,'YYYY-MM-DD') as DateDoc,2 as TypeDoc ,os.number_order_supply as NumberDoc, case when code_warehouse_through>0 then wh.name_warehouse end || ' '|| gs.name_group_supply as Description 
        ,os.number_1c as NumberDoc1C ,f.code_zip as ExtInfo,case when os.code_warehouse = 37 or os.code_warehouse_through=37 then 0 else 0 end as IsControl
        ,case when os.user_insert<>-146 then 9 else 0 end as Color
            from c.order_supply os 
              join c.group_supply gs on os.code_group_supply=gs.code_group_supply
              left join dw.firms  f on f.code_firm=os.code_company_supply
              left join dw.warehouse wh on os.code_warehouse=wh.code_warehouse
              where os.date_warehouse>sysdate -5 
              and (( os.code_warehouse in (select CodeWarehouse from T1) and nvl(os.code_warehouse_through,0)=0) or os.code_warehouse_through in (select CodeWarehouse from T1)) 
              and os.state_order in (1,2,3) and c.doc.GetSumOrderSupply(os.code_order_supply)>0 AND os.is_posted=0";

            res.Doc = connection.Query<Doc>(Sql, pGD);
            Sql = @"With T1 as (
select :CodeWarehouse as CodeWarehouse from dual
union
select wh.code_warehouse_old from dw.warehouse  wh where wh.code_warehouse=  :CodeWarehouse and wh.code_warehouse_old>0
union
select wh.CodeWarehouseLink from dw.warehouse  wh where wh.code_warehouse=  :CodeWarehouse and wh.CodeWarehouseLink>0
union
select who.Code_Warehouse_Old from dw.warehouse  wh 
join dw.warehouse  who on  wh.codewarehouselink=who.code_warehouse
where wh.code_warehouse =  :CodeWarehouse and  who.Code_Warehouse_old>0)

select 2 as TypeDoc,os.number_order_supply as NumberDoc,ROW_NUMBER ( ) OVER (  PARTITION BY wos.code_order_supply   ORDER BY  wos.code_wares ) AS OrderDoc, 
         wos.code_wares as CodeWares,
         case when os.state_order=0 then wos.begin_quantity
            when os.state_order=1 then nvl(wos.confirm_quantity,wos.begin_quantity)
            when os.state_order=2 then nvl(wos.delivered_quantity,wos.confirm_quantity)
            when os.state_order=3  then wos.delivered_quantity
            when os.state_order=4 then wos.invoice_quantity
            else null end as quantity,
              0 as QuantityMin,               
             /*1.5*case when os.state_order=0 then wos.begin_quantity
            when os.state_order=1 then nvl(wos.confirm_quantity,wos.begin_quantity)
            when os.state_order=2 then nvl(wos.delivered_quantity,wos.confirm_quantity)
            when os.state_order=3  then wos.delivered_quantity
            when os.state_order=4 then wos.invoice_quantity
            else null end*/
               1000000  as QuantityMax
      from c.order_supply os 
      join c.wares_order_supply wos on os.code_order_supply=wos.code_order_supply
      where os.date_warehouse>sysdate -5 
      and (os.code_warehouse in (select CodeWarehouse from T1) or os.code_warehouse_through in (select CodeWarehouse from T1)) 
      and os.state_order in (1,2,3) AND os.is_posted=0";

            res.Wares = connection.Query<DocWaresSample>(Sql, pGD);
            connection.Close();
            return res;
        }
        UtilNetwork.Result IsConnect()
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
                return new UtilNetwork.Result(e);
            }
            return new UtilNetwork.Result();
        }

    }
}
