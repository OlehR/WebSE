using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
//using LibApiDCT;
using Microsoft.AspNetCore.Mvc;
using ModelMID;
using System;
using UtilNetwork;

//using ModelMID.DB;
using Utils;

namespace WebSE.Controllers
{
    [Route("DCT")]
    public class ApiDCT : Controller
    {
        readonly BL Bl;
        static Raitting cRaitting;
        public ApiDCT()
        {
            Bl = BL.GetBL;
            string cd = Directory.GetCurrentDirectory();
            cRaitting = new Raitting();
            FileLogger.Init(Path.Combine(cd, "Logs"), 0, eTypeLog.Full);
        }

        #region DCT
        [HttpPost]
        [Route("Login")]
        public UtilNetwork.Result<AnswerLogin> Login([FromBody] UserBRB pU)
        {
            return  Bl.Login(pU);
        }

        [HttpPost]
        [Route("GetPrice")]
        public UtilNetwork.Result<WaresPrice> GetPrice([FromBody] ApiPrice pAP)
        {           
            return Bl.GetPrice(pAP);
        }

        int GetCodeUser()
        {
            string strUserGuid = Request.Headers["UserGuid"];
            if (string.IsNullOrEmpty(strUserGuid)) return 0;
            try
            {
                var R = new System.Guid(strUserGuid);
                return Bl.GetUserExpiring(R)?.CodeUser ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        [HttpPost]
        [Route("GetGuid")]
        public string /*Result<BRB5.Model.Guid> */ GetGuid([FromBody] int pCodeWarehouse)
        {
            var R= GetCodeUser();
           
            var Res = Bl.GetGuid(pCodeWarehouse,R);
            string r = Res.ToJSON();
            return r;//Bl.GetGuid(pCodeWarehouse);
        }

        [HttpPost]
        [Route("LoadDoc")]
        public UtilNetwork.Result<Docs> LoadDocs([FromBody] GetDocs pGD)
        {
            return Bl.LoadDocs(pGD);
        }


        [HttpPost]
        [Route("SaveDoc")]
        public UtilNetwork.Result SaveDoc([FromBody] SaveDoc pD)
        {
            return  Bl.SaveDocData(pD);
        }


        [HttpPost]
        [Route("SaveLogPrice")]
        public UtilNetwork.Result SaveLogPrice([FromBody] LogPriceSave pD)=> Bl.SaveLogPrice(pD);

        [HttpPost]
        [Route("GetClient")]
        public async Task<UtilNetwork.Result<IEnumerable<Client>>> GetClientAsync([FromBody] FindClient pFC)
        {
            return await Bl.GetClientAsync(pFC);
        }

        [HttpPost]
        [Route("Raitting/GetIdRaitingTemplate")]
        public UtilNetwork.Result<int> GetIdRaitingTemplate()
        {
            try
            {
                return new UtilNetwork.Result<int>() { Data = cRaitting.GetIdRaitingTemplate() };
            }
            catch (Exception e)
            {
                return new UtilNetwork.Result<int>(e);
            }
        }

        [HttpPost]
        [Route("Raitting/GetNumberDocRaiting")]
        public UtilNetwork.Result GetNumberDocRaiting()
        {
            try
            {
                return new UtilNetwork.Result() { Data = cRaitting.GetNumberDocRaiting().ToString() };
            }
            catch (Exception e)
            {
                return new UtilNetwork.Result(e);
            }
        }


        [HttpPost]
        [Route("Raitting/SaveTemplate")]
        public UtilNetwork.Result SaveTemplate([FromBody] RaitingTemplate pRT)
        {
            return cRaitting.SaveTemplate(pRT);
        }


        [HttpPost]
        [Route("Raitting/SaveDocRaiting")]
        public UtilNetwork.Result SaveDocRaiting([FromBody] Doc pDoc)
        {
            return cRaitting.SaveDocRaiting(pDoc);
        }

        [HttpPost]
        [Route("Raitting/GetRaitingTemplate")]
        public UtilNetwork.Result<IEnumerable<RaitingTemplate>> GetRaitingTemplate()
        {
            return cRaitting.GetRaitingTemplate();
        }

        [HttpPost]
        [Route("Raitting/GetRaitingDocs")]
        public IEnumerable<Doc> GetRaitingDocs()
        {
            return cRaitting.GetRaitingDocs();
        }

        [HttpPost]
        [Route("CheckPromotion/Doc")]
        public UtilNetwork.Result<IEnumerable<Doc>> GetPromotion([FromBody] int pCodeWarehouse)
        {
            return Bl.GetPromotion(pCodeWarehouse);
        }

        [HttpPost]
        [Route("CheckPromotion/GetPromotionData")]
        public UtilNetwork.Result<IEnumerable<DocWares>> GetPromotionData([FromBody] string pNumberDoc)
        {
            return Bl.GetPromotionData(pNumberDoc);
        }

        #endregion
    }
}
