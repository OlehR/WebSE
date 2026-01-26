using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System.Collections.Generic;
using UtilNetwork;
using Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebSE
{
    public partial class BL
    {
        public UtilNetwork.Result<MidData> LoadData(ModelMID.InLoadData pLD)
        {
            try
            {
                LibApiDCT.LoadData.Init(Startup.Configuration);
                var res = LibApiDCT.LoadData.LoadMID(pLD);
                return res;
            }
            catch(Exception e)
            {
                return new UtilNetwork.Result<MidData>(e);
            }
           
        }

        public string BuldMID()
        {
            LibApiDCT.LoadData.Init(Startup.Configuration);
            var WP = msSQL.GetWorkPlaces();
            Task.Run(() => LibApiDCT.LoadData.BildMID(WP));
            Task.Run(() => CoffeeMachine.SendAsync(DateTime.Now.AddDays(-1)));
            return string.Join(",", WP); ;
        }
        public UtilNetwork.Result SetPhoneNumber(SetPhone pSPN)
        {
            var body = SoapTo1C.GenBody("SetPhoneNumber", [ new("CardId",pSPN.CodeClient.ToString()),new("NumTel", pSPN.Phone), new ("User", pSPN.UserBarCode??""),
                                                            new("ShopId", pSPN.CodeWarehouse.ToString()), new("CheckoutId", pSPN.IdWorkPlace.ToString()),new("DateOper", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")) ] );
            var res = SoapTo1C.RequestAsync(Global.Server1C, body, 100000, "text/xml", "Администратор:0000").Result;
            UtilNetwork.Result Res = new(res.State, res.Data);
            FileLogger.WriteLogMessage($"CreateCustomerCard Contact=>{pSPN.ToJson()} Res={res.ToJson()} ");
            return Res;
        }
        public UtilNetwork.Result SetWeightReceipt(IEnumerable<WeightReceipt> pWR)
        {
            try
            {
                bool r=msSQL.SetWeightReceipt(pWR);
                return new UtilNetwork.Result(r ? 0:1, r ? "Weight receipt saved successfully.": "Error save Weight receipt ");
            }
            catch (Exception e)
            {
                return new UtilNetwork.Result(e);
            }
            
        }
    }
}
