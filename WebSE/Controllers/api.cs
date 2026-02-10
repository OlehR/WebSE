using BRB5.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ModelMID;
using ModelMID.DB;
using Newtonsoft.Json;
using SharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UtilNetwork;
using Utils;
using WebSE.Filters;

namespace WebSE.Controllers
{

    [ApiController]
    [Route("[controller]")]    
    public class api : ControllerBase
    {
        public string Version { get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); } }
        private readonly ILogger<api> _logger;
        static BL Bl;
        static bool Flag = false;
        public api(ILogger<api> logger)
        {
            _logger = logger;
            if (!Flag)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, $"Ver={Version}", eTypeLog.Expanded);
                Bl = BL.GetBL;
                
                //Bl.GetConfig();
                //FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name,"");
                Flag = true;
            }
        }

        [HttpGet]
        [Route("/GetInfo")]
        public string GetInfo()
        {
            var res = @$"Ver={Bl.Version}
GC=>{GC.GetTotalMemory(false) / (1024 * 1024)}Mb
FileLogger=>{FileLogger.GetFileName}
{Bl.GenCountNeedSend()}
WorkPlace=>{ModelMID.Global.WorkPlaceByWorkplaceId?.Count()}";
            FileLogger.WriteLogMessage(this, "/GetInfo", res);
            return res;
        }

        #region Card
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("FindByPhoneNumber/")]
        public UtilNetwork.Result<string> FindByPhoneNumber([FromBody] InputPhone pUser)
        {
            if (pUser == null || string.IsNullOrEmpty(pUser.ShortPhone))
                return new UtilNetwork.Result<string>(-1, "Невірні вхідні дані");

            return Bl.FindByPhoneNumber(pUser);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("CreateCustomerCard/")]       
        public StatusIsBonus CreateCustomerCard([FromBody] Contact pContact)
        {
            if (pContact == null)
                return new StatusIsBonus(-1, "Невірні вхідні дані");
            //if(pContact.bonus==0) pContact.bonus = 500;
            return Bl.CreateCustomerCard(pContact);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("card/update/")]
        public StatusIsBonus CardUpdate([FromBody] Contact pContact)
        {
            if (pContact == null)
                return new StatusIsBonus(-1, "Невірні вхідні дані");

            return Bl.CreateCustomerCard(pContact);
        }

        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [HttpPost]
        [Route("card/create")]
        public UtilNetwork.Result CardCreate([FromBody] Contact pContact)
        {
            if (pContact == null)
                return new UtilNetwork.Result(-1, "Невірні вхідні дані");
            return Bl.CreateCustomerCard(pContact);
        }

        [HttpPost]
        [Route("FindClient/")]
        public UtilNetwork.Result<Client> FindClient([FromBody] FindClient pFC)
        {
            Client client = null;
            if (pFC == null) return null ;            
            return new UtilNetwork.Result<Client>() { Data= Bl.GetClientPhone(pFC.BarCode) };
        }

        [HttpPost]
        [Route("GetDiscount")]
        public async Task<UtilNetwork.Result<Client>> GetDiscountAsync([FromBody] FindClient pFC)
        {
            return await Bl.GetDiscountAsync(pFC);      
        }
       
        #endregion


        /*[HttpPost]
        [Route("/OldPrint")]
        public string OldPrint([FromBody] Pr pStr)
        {
            //   if (string.IsNullOrEmpty(pStr))
            //       return null;//new Status(-1, "Невірні вхідні дані");
            string output = JsonConvert.SerializeObject(pStr);
            return http.RequestAsync("http://znp.vopak.local:8088/Print", HttpMethod.Post, output, 5000, "application/json");
        }*/
        
        [HttpPost]
        [Route("/SMS")]
        public UtilNetwork.Result<string> SMS([FromBody] VerifySMS pV)
        {
            try
            {
                //VerifySMS pV = new(); http://loyalty.zms.in.ua/api
                var r = http.RequestAsync( $"{http.Url}sms?phone={pV.Phone}&campaign={pV.Company}", HttpMethod.Get, null, 3000, "application/json;charset=UTF-8", http.GetAuthorization());

                var Ans = JsonConvert.DeserializeObject<answer>(r);

                if (Ans != null && Ans.status.ToLower().Equals("success"))
                    return new UtilNetwork.Result<string>() { Data = Ans.verify };
                return new UtilNetwork.Result<string>(-1, "SMS не відправлено");
            }
            catch (Exception e) { return new UtilNetwork.Result<string>(-1, e.Message); }
        }


        [HttpPost]
        [Route("/AddBrandToGS")]
        public string SetAddBrandToGS(AddBrandToGS pBGS)
        {
            if (pBGS == null)
                return "Відсутні вхідні дані";
            return Bl.SetAddBrandToGS(pBGS);
        }

        [HttpPost]
        [Route("/znp")]
        public string Znp([FromBody] dynamic pStr)
        {
            string Res;
            login l;
            (string, login) tt = Bl.Znp( pStr, GetHttpContex());            
            (Res, l) = tt;
            if (l != null)
                SetHttpContext(l);
            return Res;
        }

        [HttpPost]
        [Route("/Print")]
        public string Print([FromBody] WaresGL pWares) => Bl.Print(pWares);
        

        [HttpPost]
        [Route("/ReloadReceiptDB")]
        public string ReloadReceiptDB([FromBody]  ParamReloadDB pRDB)
        {
            Bl.ReloadReceiptDB(pRDB.IdWorkPlace, pRDB.Begin, pRDB.End);
            return null;
        }

        void SetHttpContext(login l)
        {
            if (!string.IsNullOrEmpty(l.Login) && !string.IsNullOrEmpty(l.PassWord) 
                && !l.Login.Equals( HttpContext.Session.GetString("Login")) && !l.PassWord.Equals(HttpContext.Session.GetString("PassWord")))
            {
                HttpContext.Session.SetString("Login", l.Login);
                HttpContext.Session.SetString("PassWord", l.PassWord);
            }
            /*else
            {
                l.Login = HttpContext.Session.GetString("Login");
                l.PassWord = HttpContext.Session.GetString("PassWord");
            }*/
        }

        /*[HttpPost]
        [Route("/UploadFile")]
        public void UploadFile(CreatePost model)
        {
            //Getting file meta data
            var fileName = Path.GetFileName(model.MyFile.FileName);
            var contentType = model.MyFile.ContentType;
            model.MyFile.CopyTo(new FileStream(fileName, FileMode.Create));
        }*/

        [HttpPost]
        [Route("/UploadFile")]
        public async Task<UtilNetwork.Result> UploadFile(IFormFile formFile) //
        {
            if (formFile?.FileName == null)
            {
                return new UtilNetwork.Result(-1, "Відсутній вхідний файл");
            }
            if (!Directory.Exists("Files/"))
                Directory.CreateDirectory("Files/");
            var path = Path.Combine("Files/", formFile.FileName);

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                    stream.Close();
                }
                return new UtilNetwork.Result();
            }
            catch (Exception e)
            {
                return new UtilNetwork.Result(e);
            }
        }

        [HttpPost]
        [Route("/ReloadReceipt")]
        public void ReloadReceipt([FromBody] IdReceipt pIdR)
        {
            Bl.ReloadReceipt(pIdR);
        }


        [HttpPost]
        [Route("/SendReceipt1C")]
        public async Task<string> SendReceipt1CAsync([FromBody] IdReceipt pIdR)
        {
            return await Bl.SendReceipt1CAsync(pIdR);
        }

        [HttpPost]
        [Route("/ReloadBadReceipt")]
        public async Task<string> ReloadBadReceipt([FromBody] int pCodePeriod)
        {
            return await Bl.ReloadBadReceipt(pCodePeriod);
        }



        [HttpPost]
        [Route("/LoadReceiptNo1C")]
        public async Task<string> LoadReceiptNo1C([FromBody] int pCodeReceipt)
        {
            return await Bl.LoadReceiptNo1C(pCodeReceipt);
        }

        [HttpPost]
        [Route("/ReloadReceiptToPG")]
        public async Task<string> ReceiptWaresPromotionNoPrice([FromBody] IdReceipt pIdR)
        {
            return await Bl.ReloadReceiptToPG(pIdR);
        }
        [HttpPost]
        [Route("/ReloadReceiptTo1CQuery")]
        public async Task<string> ReloadReceiptTo1CQuery([FromBody] string pSQL)
        {
               return await Bl.ReloadReceiptTo1CQuery(pSQL.Replace("'", "\""));
        }

        [HttpPost]
        [Route("/ReloadReceiptToPGQuery")]
        public async Task<string> ReloadReceiptToPGQuery([FromBody] string pSQL)
        {
            return await Bl.ReloadReceiptToQuery(pSQL.Replace("'", "\""));
        }

        [HttpPost]
        [Route("/Test")]
        public async Task<string> Test()
        {
            return await Bl.TestAsync();
        }

        [HttpGet]
        [Route("/UploadListex")]
        public async Task<string> UploadListex()
        {
            var aa = new Listex();
            return await aa.CSV();
        }

        [HttpPost]
        [Route("/SendBukovel")]
        public async Task<string> SendBukovel([FromBody] IdReceipt pIdR)
        {
            //await Bl.SendAllBukovelAsync();
            await Bl.SendReceiptBukovelAsync(pIdR);
            return "";
        }

        [HttpPost]
        [Route("/ReSendBukovel")]
        public async Task<string> ReSendBukovel()
        {
            //await Bl.SendAllBukovelAsync();
            string res=await Bl.ReSendBukovelAsync();
            return res;
        }

        [HttpPost]
        [Route("/Send1CClient")]
        public async Task<eReturnClient> Send1CClient([FromBody] ClientNew pC)=> await Bl.Send1CClient(pC);

        [HttpPost]
        [Route("/Send1CReceiptWaresDeleted")]
        public async Task<bool> Send1CReceiptWaresDeletedAsync([FromBody] IEnumerable<ReceiptWaresDeleted1C> pRWD) => await Bl.Send1CReceiptWaresDeletedAsync(pRWD);


        [HttpPost]
        [Route("/GetClientOrder")]
        public IEnumerable<ReceiptWares> GetClientOrder([FromBody] string pNumberOrder) => Bl.GetClientOrder(pNumberOrder);

        [HttpPost]
        [Route("/GetReceipt1C")]
        public Dictionary<string, decimal> GetReceipt1C([FromBody] IdReceipt pIdR) => Bl.GetReceipt1C(pIdR);
        [HttpGet]
        [Route("/BildGiftCard")]
        public string BildGiftCard(int pCount, int pNominal = 0, int pStart=0)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = pStart; i < pStart + pCount; i++)
                sb.Append( Model.StaticModel.CreateGiftCard(pNominal, i)+Environment.NewLine);
            return sb.ToString();
        }
    
        login GetHttpContex()
        {
            return new login() { Login = HttpContext.Session.GetString("Login"), PassWord = HttpContext.Session.GetString("PassWord") };
            var formContent = new MultipartFormDataContent();
        }


        [HttpPost]
        [Route("SU/GetRestSU")]
        public RestSU GetRestSU()=> Bl.GetRestSU();


        [HttpPost]
        [Route("SU/GetBaseSU")]
        public BaseSU GetBaseSU() => Bl.GetBaseSU();//.ToJson();
        
        
    }
    public class answer
    {
        public string status { get; set; }
        public string verify { get; set; }
    }

    public class ParamReloadDB
    {
        public int IdWorkPlace { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
    }

    public class CreatePost
    {
        public string FileCaption { set; get; }
        public string FileDescription { set; get; }
        public IFormFile MyFile { set; get; }
    }

}
