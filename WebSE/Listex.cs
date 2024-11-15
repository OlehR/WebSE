using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebSE
{
    public class ListexCSV
    {
        public string GTIN { get; set; }
        public string GoodName { get; set; }
        public string Article { get; set; }
        public string CatId { get; set; }
        public string SupplierName { get; set; }
        public string EDRPOU { get; set; }
        public override string ToString() { return $"{GTIN};{GoodName.Replace(';',',')};{Article};{CatId};{SupplierName};{EDRPOU}"; }
    }
    public class Listex
    {
        public async Task<string> CSV()
        {
            try
            {
                var aa = GetListexCSV();
                string res = $"GTIN;GoodName;Article;CatId;SupplierName;EDRPOU{Environment.NewLine}" + string.Join(Environment.NewLine, aa.Select(x => x.ToString()));
                string location = "ftp://a.listex.info:21/upload/Price.csv";
                
                WebRequest ftpRequest = WebRequest.Create(location);
                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
                ftpRequest.Credentials = new NetworkCredential("124377", "BpZJiqUcHQBQcKx");

                Stream requestStream = ftpRequest.GetRequestStream();

                Stream stream = GenerateStreamFromString(res);
                stream.CopyTo(requestStream);
                requestStream.Close();
                stream.Close();
                return aa.Count().ToString();
            }
            catch (Exception ex) { return ex.Message; }
            //}
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public IEnumerable<ListexCSV> GetListexCSV()
        {
            try
            {
                string MsSqlInit = "Data Source=sqlsrv2.vopak.local;Initial Catalog=for_cubes;User ID=dwreader;Password=DW_Reader;Connection Timeout=30";
                var Con = new SqlConnection(MsSqlInit);
                string SQL = @"DECLARE @d int = 20240101
SELECT w.barcode_last AS GTIN, w.name_full AS GoodName, w.articul AS Article, w.nomen_group_id AS CatId, cc.name_full AS SupplierName, cc.inn AS EDRPOU FROM 
(SELECT DISTINCT nomen_id FROM 
dbo.fact_deficit_surplus WHERE day_id=CONVERT(INT,CONVERT(NCHAR , getdate(),112)) AND n_min_rest>0 ) la 
LEFT JOIN 
(SELECT	[nomen_id],[contractor_id]
		FROM (
			SELECT cfrpiv.[nomen_id],cfrpiv.[contractor_id]  ,ROW_NUMBER() OVER(PARTITION BY nomen_id ORDER BY [period] DESC) as [nn]
			  FROM [dbo].fact_reg_party_in_last cfrpiv
         JOIN [dbo].dimen_contr_contract cc ON ( cfrpiv.contr_contract_id = cc.contr_contract_id  AND contr_contract_kind_id='9FB46CA2C6C32E244E408384C41B8312' )
    WHERE cfrpiv.amount<>0 AND [day_id]>= @d 
		 ) r1
		 WHERE r1.[nn] = 1) l ON la.nomen_id= l.nomen_id
    join dbo.dimen_nomen  w on la.nomen_id= w.nomen_id
    left join dbo.dimen_contractor cc  on l.contractor_id = cc.contractor_id  
    WHERE w.barcode_last IS NOT NULL";
                var r = Con.Query<ListexCSV>(SQL);
                return r;
            }
            catch (Exception e)
            {
                var s = e.Message;
            }
            return null;
        }


    }
}
