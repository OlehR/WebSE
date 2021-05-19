//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSE.Controllers;
using QRCoder;
using Utils;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace WebSE
{
    public class BL
    {
        SoapTo1C soapTo1C = new SoapTo1C();
        MsSQL msSQL = new MsSQL();

        public Status Auth(InputPhone pIPh)
        {
            FileLogger.WriteLogMessage($"Auth User=>{pIPh.ShortPhone}");
            try
            {
                var r = msSQL.Auth(pIPh.ShortPhone);
                return new Status(r);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"Auth User=>{pIPh.ShortPhone} Error=> {ex.Message}");
                return new Status(-1, ex.Message);
            }

        }


        public Status Register(RegisterUser pUser)
        {
            var strUser = JsonSerializer.Serialize(pUser);
            try
            {
                FileLogger.WriteLogMessage($"Register Start User=>{strUser}");
                var rdd = new InputPhone() { phone = pUser.phone };
                var r = msSQL.Auth(rdd.ShortPhone);
                if (r)
                    return new Status();
                try
                {
                    var res = new http().SendPostAsync(new Contact(pUser));
                    if (res != null && res.status != null && res.status.Equals("success") && res.contact != null)
                        pUser.IdExternal = res.contact.id;
                }catch(Exception e)
                {
                    FileLogger.WriteLogMessage($"Register SendPostAsync System Error=>{e.Message} User=>{strUser}");
                }

                return new Status(msSQL.Register(pUser));
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"Register Error=>{e.Message} User=>{strUser}");
                return new Status(-1, e.Message);
            }
        }

        public async Task<InfoBonus> GetBonusAsync(string pBarCode)
        {
            InfoBonus Res = new InfoBonus() { card = pBarCode };
            FileLogger.WriteLogMessage($"GetBonusAsync Start BarCode=>{pBarCode}");
            try
            {
                Res.pathCard = GetBarCode(pBarCode);
                decimal Sum;
                var body = soapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", pBarCode) });
                var res = await soapTo1C.RequestAsync(Global.Server1C, body);
                res = res.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    Res.bonus = Sum; //!!!TMP
                body = soapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", pBarCode) });
                res = await soapTo1C.RequestAsync(Global.Server1C, body);

                res = res.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                    Res.rest = Sum;
                //Global.OnClientChanged?.Invoke(parClient, parTerminalId);


            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"GetBonusAsync BarCode=>{pBarCode} Error =>{ex.Message}");
                return new InfoBonus(-1, ex.Message);
                // Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = parTerminalId, Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
            }

            return Res;
        }
        public Promotion GetPromotion()
        {
            FileLogger.WriteLogMessage($"GetPromotion Start");
            try
            {
                return new Promotion { products = new Product[] { NewsPaper(), YellowPrice() } };
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"GetPromotion Error=>{ex.Message}");
                return new Promotion(-1, ex.Message);
            }
        }

        private Product NewsPaper()
        {
            string path = @"img\NP4";
            var Res = new Product() { name = "Газета", id = -1, folder = true, description = "№4", img = Path.Combine(path, "p1.jpg") };
            var Files = Directory.GetFiles(path, "p?.jpg");
            Res.folderItems = Files.Select(a => Product.GetFileName(Path.Combine(path, a))).ToArray();
            for (int i = 0; i < Res.folderItems.Length; i++)
            {
                Files = Directory.GetFiles(path, $"p{-Res.folderItems[i].id}_*.jpg");
                Res.folderItems[i].folderItems = Files.Select(a => Product.GetPicture(Path.Combine(a))).ToArray();
                //var r2 = JsonConvert.SerializeObject(el);
            };
            //var r = JsonConvert.SerializeObject(Res);
            return Res;
        }
        private Product YellowPrice()
        {
            var pathDirection = "Dir";
            var pathWares = "Wares";
            var Res = new Product() { name = "Жовті цінники", id = -3, folder = true, description = "", img = null };

            var Gr = msSQL.GetDirection();
            var W = msSQL.GetWares();
            Res.folderItems = Gr.Select(a => Product.GetProduct(a, pathDirection)).ToArray();

            for (int i = 0; i < Res.folderItems.Length; i++)
            {
                var el = Res.folderItems[i];
                var r = W.Where(r => r.CodeDirection == el.id).Select(e => Product.GetProduct(e, pathWares)).ToArray();
                el.folderItems = r;
            }
            return Res;
        }

        private string GetBarCode(string pBarCode)
        {
            string FileName = $"img/BarCode/{pBarCode}.png";
            if (File.Exists(FileName))
                return FileName;
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode($"{pBarCode}", QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                qrCode.GetGraphic(4).Save(FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"GetBarCode BarCode=>{pBarCode} FileName=>{FileName} Error =>{ex.Message}");
                return null;
            }
            return FileName;

        }

        public InfoForRegister GetInfoForRegister()
        {
            return new InfoForRegister() { locality = msSQL.GetLocality(),
                typeOfEmployment = new TypeOfEmployment[] 
                { 
                    new TypeOfEmployment { Id = 1, title = "не працюючий" }, 
                    new TypeOfEmployment { Id = 2, title = "працюючий" },
                    new TypeOfEmployment { Id = 3, title = "студент" },
                    new TypeOfEmployment { Id = 4, title = "пенсіонер" },

                } };
        }

        public string ExecuteApi( dynamic pStr)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };

            string res = System.Text.Json.JsonSerializer.Serialize(pStr, options);

            var l = System.Text.Json.JsonSerializer.Deserialize<login>(res);
            Oracle oracle = new Oracle(l);
            var Res = oracle.ExecuteApi(res);
            return Res;

        }

        public bool DomainLogin(string pLogin, string pPassWord)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            System.DirectoryServices.AccountManagement.PrincipalContext prCont = new System.DirectoryServices.AccountManagement.PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Domain, "Vopak");
            return prCont.ValidateCredentials(pLogin, pPassWord);
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }
}
