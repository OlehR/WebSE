using ModelMID.DB;
using System.Collections.Generic;
using UtilNetwork;
using Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebSE
{
    public partial class BL
    {
        public Result<MidData> LoadData(ModelMID.InLoadData pLD)
        {
            try
            {
                LibApiDCT.LoadData.Init(Startup.Configuration);
                var res = LibApiDCT.LoadData.LoadMID(pLD);
                return res;
            }
            catch(Exception e)
            {
                return new Result<MidData>(e);
            }
           
        }

        public string BildMID()
        {
            LibApiDCT.LoadData.Init(Startup.Configuration);
            var WP = msSQL.GetWorkPlaces();
            Task.Run(() => LibApiDCT.LoadData.BildMID(WP));      
            return string.Join(",", WP); ;
        }
        public Result SetPhoneNumber(ModelMID.SetPhone pSPN)
        {
            var body = soapTo1C.GenBody("SetPhoneNumber", new Parameters[] { new Parameters("CardId",pSPN.CodeClient.ToString()),new Parameters("NumTel", pSPN.Phone) });
            var res = soapTo1C.RequestAsync(Global.Server1C, body, 100000, "text/xml", "Администратор:0000").Result;
            Result Res = new(res.State,res.Data);
            FileLogger.WriteLogMessage($"CreateCustomerCard Contact=>{pSPN.ToJson()} Res={res.ToJson()} ");
            return Res;
        }
    }
}
