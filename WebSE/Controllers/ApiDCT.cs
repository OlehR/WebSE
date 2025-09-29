using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
//using LibApiDCT;
using Microsoft.AspNetCore.Mvc;
using ModelMID;
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
        public Result<AnswerLogin> Login([FromBody] UserBRB pU)
        {
            return  Bl.Login(pU);
        }

        [HttpPost]
        [Route("GetPrice")]
        public Result<WaresPrice> GetPrice([FromBody] ApiPrice pAP)
        {
            return Bl.GetPrice(pAP);
        }

        [HttpPost]
        [Route("GetGuid")]
        public string /*Result<BRB5.Model.Guid> */ GetGuid([FromBody] int pCodeWarehouse)
        {
            var Res = Bl.GetGuid(pCodeWarehouse);
            string r = Res.ToJSON();
            return r;//Bl.GetGuid(pCodeWarehouse);
        }

        [HttpPost]
        [Route("LoadDoc")]
        public Result<Docs> LoadDocs([FromBody] GetDocs pGD)
        {
            return Bl.LoadDocs(pGD);
        }


        [HttpPost]
        [Route("SaveDoc")]
        public  Result SaveDoc([FromBody] SaveDoc pD)
        {
            return  Bl.SaveDocData(pD);
        }


        [HttpPost]
        [Route("SaveLogPrice")]
        public Result SaveLogPrice([FromBody] LogPriceSave pD)=> Bl.SaveLogPrice(pD);

        [HttpPost]
        [Route("GetClient")]
        public async Task<Result<IEnumerable<Client>>> GetClientAsync([FromBody] FindClient pFC)
        {
            return await Bl.GetClientAsync(pFC);
        }

        [HttpPost]
        [Route("Raitting/GetIdRaitingTemplate")]
        public Result<int> GetIdRaitingTemplate()
        {
            try
            {
                return new Result<int>() { Info = cRaitting.GetIdRaitingTemplate() };
            }
            catch (Exception e)
            {
                return new Result<int>(e);
            }
        }

        [HttpPost]
        [Route("Raitting/GetNumberDocRaiting")]
        public Result GetNumberDocRaiting()
        {
            try
            {
                return new Result() { Info = cRaitting.GetNumberDocRaiting().ToString() };
            }
            catch (Exception e)
            {
                return new Result(e);
            }
        }


        [HttpPost]
        [Route("Raitting/SaveTemplate")]
        public Result SaveTemplate([FromBody] RaitingTemplate pRT)
        {
            return cRaitting.SaveTemplate(pRT);
        }


        [HttpPost]
        [Route("Raitting/SaveDocRaiting")]
        public Result SaveDocRaiting([FromBody] Doc pDoc)
        {
            return cRaitting.SaveDocRaiting(pDoc);
        }

        [HttpPost]
        [Route("Raitting/GetRaitingTemplate")]
        public Result<IEnumerable<RaitingTemplate>> GetRaitingTemplate()
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
        public Result<IEnumerable<Doc>> GetPromotion([FromBody] int pCodeWarehouse)
        {
            return Bl.GetPromotion(pCodeWarehouse);
        }

        [HttpPost]
        [Route("CheckPromotion/GetPromotionData")]
        public Result<IEnumerable<DocWares>> GetPromotionData([FromBody] string pNumberDoc)
        {
            return Bl.GetPromotionData(pNumberDoc);
        }

        #endregion
    }
}
