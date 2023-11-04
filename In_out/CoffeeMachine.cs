using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLib;
using ModelMID;
using Newtonsoft.Json;
using System.Net.Http;
using Utils;

namespace In_out
{
    internal class CoffeeMachine
    {
        public async Task SendAsync(DateTime pDT, int pWait = 10000)
        {
            string json = null;
            MsSQL msSQL = new MsSQL();
            //DataSync DS = new DataSync(null);
            SoapTo1C soapTo1C = new SoapTo1C();
        string Url = $"https://dashboard.prostopay.net/api/WEBPaymentsDailySales/98e071c0-7177-4132-b249-9244464c97fb?date={pDT:yyyy-MM-dd}";
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(pWait);

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, Url);
                // requestMessage.Content = new StringContent(parBody, Encoding.UTF8, parContex);
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    bool Result = true;
                    json = await response.Content.ReadAsStringAsync();
                    var Res = JsonConvert.DeserializeObject<IEnumerable<CoffeData>>(json);
                    Console.WriteLine(Res.Count());
                    int i = 0;
                    foreach (var el in Res) 
                    {
                        i++;
                        Wares W = msSQL.GetWares(el.CellInt);
                        Receipt1C r = new Receipt1C()
                        {
                            TypeReceipt = eTypeReceipt.Sale,
                            Number = $"К07{el.datetime.DayOfYear:D3}{i:D4}",
                            CodeWarehouse = 9,
                            Wares = new List<ReceiptWares1C>() { new ReceiptWares1C()
                            { CodeWares = W.CodeWares, AbrUnit = "шт", Order = 1, Price = el.Amount.ToDecimal(), Quantity = 1, Sum = el.Amount.ToDecimal() } },
                            Date = el.datetime,
                            NumberCashDesk = 9,//Номер каси
                            CashOutSum = 0,
                            NumberReceipt = (ulong)i,
                            Description = $"RNN{i}",
                            CodeBank =3 //Приват банк
                            
                        };
                        var body = soapTo1C.GenBody("JSONCheck", new Parameters[] { new Parameters("JSONSting", r.GetBase64()) });
                        string Server1C = "http://bafsrv.vopak.local/psu_utp/ws/ws1.1cws";
                        var res = Global.IsTest ? "0" : await soapTo1C.RequestAsync(Server1C, body, 240000, "application/json");
                        if (string.IsNullOrEmpty(res) || !res.Equals("0")) 
                             Result=false;
                        Console.WriteLine(r.ToJSON());
                    }
                    if(Result)
                    {

                    }
                }
            }
            catch (Exception ex)
            { var aa = ex.Message; }
        }

        class CoffeData
        {
            public DateTime datetime { get; set; }
            public string VM_external_id { get; set; }
            public string VM_number { get; set; }
            public string Client_name { get; set; }
            public string Sale_place { get; set; }
            public string POS_name { get; set; }
            public string MerchantId { get; set; }
            public string Cell { get; set; }
            public int CellInt { get { if (int.TryParse(Cell, out int Res)) return Res; else return 0; } }
            public string Product_external_id { get; set; }
            public string Product_name { get; set; }
            public string Amount { get; set; }
            public string Status { get; set; }
        }
    }
}
