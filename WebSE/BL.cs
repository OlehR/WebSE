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
using OfficeOpenXml;

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

                    var con = new Contact(pUser);
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(con);
                    var res = new http().SendPostAsync(con);
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

        public async Task<InfoBonus> GetBonusAsync(InputPhone pPh)
        {
            string pBarCode = msSQL.GetBarCode(pPh);
            if(string.IsNullOrEmpty(pBarCode))
                 pBarCode =  Global.GenBarCodeFromPhone(pPh.FullPhone2);

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
            string pathDir = @"img\";

            var Dirs=Directory.GetDirectories(pathDir,"NP*");
            var CodeNP=Dirs.Max(a => int.Parse(new DirectoryInfo(a).Name.Substring(2)));
            string path = Path.Combine( pathDir,$"NP{CodeNP}");

            var Files = Directory.GetFiles(path, "p?.jpg");
            if (Files == null || Files.Length==0 )
                return null;
            var Res = new Product() { name = "Газета", id = -1, folder = true, description = $"№{CodeNP}", img = Files.First().Replace("\\", "/") };
            
            Res.folderItems = Files.Select(a => Product.GetFileName(Path.Combine(a))).ToArray();
            for (int i = 0; i < Res.folderItems.Length; i++)
            {
                Files = Directory.GetFiles(path, $"p{-Res.folderItems[i].id}_*.???");
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
            var Res = new Product() { name = "Жовті цінники", id = -3, folder = true, description = "", img = "Y.png" };

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
            if (pBarCode.Substring(0, 1).Equals("+"))
                pBarCode = pBarCode.Substring(1);
            string FileName = $"img/BarCode/{pBarCode}.png";
            if (File.Exists(FileName))
                return FileName;
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode($"{pBarCode}", QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                qrCode.GetGraphic(12).Save(FileName, System.Drawing.Imaging.ImageFormat.Png);
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

        public string test ()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                //Set some properties of the Excel document
                excelPackage.Workbook.Properties.Author = "VDWWD";
                excelPackage.Workbook.Properties.Title = "Title of Document";
                excelPackage.Workbook.Properties.Subject = "EPPlus demo export data";
                excelPackage.Workbook.Properties.Created = DateTime.Now;

                //Create the WorkSheet
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet 1");

                //Add some text to cell A1
                worksheet.Cells["A1"].Value = "My first EPPlus spreadsheet!";
                //You could also use [line, column] notation:
                worksheet.Cells[1, 2].Value = "This is cell B1!";

                //Save your file
                FileInfo fi = new FileInfo(@"d:\File.xlsx");
                excelPackage.SaveAs(fi);
            }
            return "Ok";
        }
    }
}
