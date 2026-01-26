using ModelMID;
using Newtonsoft.Json;
using Supplyer;
using UtilNetwork;
using Utils;

namespace WebSE
{
    internal class CoffeeMachine
    {
        public static async Task<UtilNetwork.Result> SendAsync(DateTime pDT, int pWait = 10000)
        {
            string json = null;
            MsSQL msSQL = new();

            string Url = $"https://dashboard.prostopay.net/api/dfsales/98e071c0-7177-4132-b249-9244464c97fb?date={pDT:yyyy-MM-dd}";
            try
            {
                HttpClient client = new() { Timeout = TimeSpan.FromMilliseconds(pWait) };

                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, Url);
                // requestMessage.Content = new StringContent(parBody, Encoding.UTF8, parContex);
                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    bool Result = true;
                    json = await response.Content.ReadAsStringAsync();
                    var Res = JsonConvert.DeserializeObject<IEnumerable<CoffeData>>(json);
                    Console.WriteLine(Res.Count());

                    Receipt1C r = new()
                    {
                        TypeReceipt = eTypeReceipt.Sale,
                        Number = $"К07{pDT:MMdd}0001",
                        CodeWarehouse = 9,
                        Wares = Res.Select(xx => xx.GetReceiptWares1C(msSQL)),
                        Date = pDT,
                        NumberCashDesk = 9,//Номер каси
                        CashOutSum = 0,
                        NumberReceipt = 1,
                        Description = $"RNN",
                        CodeBank = 3 //Приват банк                            
                    };
                    var body = SoapTo1C.GenBody("JSONCheck", [new("JSONSting", r.GetBase64())]);
                    string Server1C = "http://bafsrv.vopak.local/psu_utp/ws/ws1.1cws";
                    var res = await SoapTo1C.RequestAsync(Server1C, body, 240000, "application/json");
                    if (res?.State == 0 && res?.Data.Equals("0") == true)
                        res.Data += ' ' + r.Number;
                    return res;
                }
            }
            catch (Exception ex)
            { return new(ex); }
            return new();
        }

        class CoffeData
        {
            /*public DateTime datetime { get; set; }          
            public string VM_external_id { get; set; }
            public string VM_number { get; set; }
            public string Client_name { get; set; }
            public string Sale_place { get; set; }
            public string POS_name { get; set; }
            public string MerchantId { get; set; }*/
            public string Cell { get; set; }
            public int CellInt { get { return Cell.ToInt(); } }
            /*public string Product_external_id { get; set; }
            public string Product_name { get; set; }*/
            public string Amount { get; set; }
            //public string Status { get; set; }
            public string Quantity { get; set; }

            public DateTime d { get; set; }
            public string cName { get; set; }
            public string vmNumber { get; set; }
            public string inv_num { get; set; }
            public string vocName { get; set; }
            public string posName { get; set; }
            public string giName { get; set; }
            public string giName_id { get; set; }
            public string type_payment { get; set; }

            public ReceiptWares1C GetReceiptWares1C(MsSQL msSQL)
            {
                ModelMID.Wares W = msSQL.GetWaresPlu(CellInt);
                return new()
                { CodeWares = W.CodeWares, AbrUnit = "шт", Order = 1, Price = Amount.ToDecimal() / Quantity.ToDecimal(), Quantity = Quantity.ToInt(), Sum = Amount.ToDecimal() };
            }
        }
    }
}
