using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace InOut
{
    public class MsSQL
    {
        public SqlConnection connection;
        public SqlConnection ConnDW;
        public MsSQL()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "sqlsrv2.vopak.local";
            builder.UserID = "dwreader";
            builder.Password = "DW_Reader";
            builder.InitialCatalog = "for_cubes";
            connection = new SqlConnection(builder.ConnectionString);
            builder.DataSource = "sqlsrv1.vopak.local";
            builder.InitialCatalog = "DW";
            ConnDW = new SqlConnection(builder.ConnectionString);

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
    class Parse
    {
        MsSQL msSql = new MsSQL();
        public async System.Threading.Tasks.Task<string> RequestAsync(int dStart=0, int pWait = 10000)
        {
            string json = null;
            DateTime bDate,eDate = DateTime.Now.Date.AddDays(-1);
            bDate = (dStart == 0 ? eDate.AddDays(-10) : DateTime.ParseExact(dStart.ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture));

            string Url = $"http://wifi.intelpol.com.ua/api.php?date_from={bDate:yyyy-MM-dd}&date_to={eDate:yyyy-MM-dd}&token=5d5d45e5734d2b7d0e3a0f2cfb06a2d9";
            try
            {

                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(pWait);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, Url);

                // requestMessage.Content = new StringContent(parBody, Encoding.UTF8, parContex);
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();

                    Groups obj = JsonConvert.DeserializeObject<Groups>(json);
                    var dd = new { dBegin = bDate.ToString("yyyyMMdd"), dEnd = eDate.ToString("yyyyMMdd") };
                    msSql.ClearData(dd);

                    foreach (var Group in obj.groups)
                    {
                        group GroupEl = Group.Value;
                        foreach (var Stores in GroupEl.stores)
                        {
                            store StoreEl = Stores.Value;
                            foreach (var Zone in StoreEl.zones)
                            {
                                zone ZoneEl = Zone.Value;
                                foreach (var Stat in ZoneEl.stats)
                                {
                                    var Hours = Stat.Value;
                                    foreach (var el in Hours)
                                    {
                                        var TypeZone = 0;
                                        switch (Group.Key)
                                        {
                                            case "676": TypeZone = 1; break;
                                            case "677": TypeZone = 2; break;
                                            case "678": TypeZone = 11; break;
                                        }

                                        var d = new DbInOut() { day_Id = Stat.Key.Replace("-", ""), warehouse_id = "B7A3001517DE370411DF7DD82E29F000", code_zone = Zone.Key, Type_Zone = TypeZone, hour_id = el.Key, amount = el.Value };
                                        msSql.InsertData(d);
                                    }

                                }

                            }

                        }
                    }


                }
                else
                {

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }
            return json;
        }

    }


    public class DbInOut
    {
        public string day_Id { get; set; }
        public string warehouse_id { get; set; }
        public string code_zone { get; set; }
        public int Type_Zone { get; set; }
        public string hour_id { get; set; }
        public string amount { get; set; }
    }



public class Groups
    {
        public Dictionary<string, group> groups { get; set; }
    }
    public class group
    {
        public string name_group { get; set; }

        public Dictionary<string, store> stores { get; set; }
    }

    public class store
    {
        public string num_store { get; set; }
        public string name_store { get; set; }
        public Dictionary<string, zone> zones { get; set; }
    }

    public class zone
    {
        public string name_zone { get; set; }

        public Dictionary<string, Dictionary<string, string>> stats { get; set; }
    }

    
}

