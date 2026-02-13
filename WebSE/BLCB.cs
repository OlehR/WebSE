using Microsoft.AspNetCore.Mvc;
using ModelMID;
using ModelMID.DB;
using SharedLib;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json;
using UtilNetwork;
using Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebSE
{
    public partial class BL
    {
        public UtilNetwork.Result Auth(InputPhone pIPh)
        {
            FileLogger.WriteLogMessage($"Auth User=>{pIPh.ShortPhone}");
            try
            {
                var r = msSQL.Auth(pIPh);
                return new UtilNetwork.Result(r);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"Auth User=>{pIPh.ShortPhone} Error=> {ex.Message}");
                return new UtilNetwork.Result(-1, ex.Message);
            }
        }

        public UtilNetwork.Result Register(RegisterUser pUser)
        {
            var strUser = JsonSerializer.Serialize(pUser);
            try
            {
                FileLogger.WriteLogMessage($"Register Start User=>{strUser}");
                var rdd = new InputPhone() { phone = pUser.phone };
                var r = msSQL.Auth(rdd);
                if (r)
                    return new UtilNetwork.Result();
                try
                {
                    var con = new Contact(pUser);
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(con);
                    var res = new http().SendPostSiteCreate(con);
                    if (res != null && res.status != null && res.status.Equals("success") && res.contact != null)
                    {
                        pUser.IdExternal = res.contact.id;
                        pUser.BarCode = res.contact.ecard;
                        con.card_number = pUser.BarCode;
                    }
                    CreateCustomerCard(con);

                }
                catch (Exception e)
                {
                    FileLogger.WriteLogMessage($"Register SendPostAsync System Error=>{e.Message} User=>{strUser}");
                }
                return new UtilNetwork.Result(msSQL.Register(pUser));
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage($"Register Error=>{e.Message} User=>{strUser}");
                return new UtilNetwork.Result(-1, e.Message);
            }
        }

        public async Task<AllInfoBonus> GetBonusAsync(InputPhone pPh)
        {
            AllInfoBonus oRes = new AllInfoBonus();
            oRes.cards = msSQL.GetBarCode(pPh);
            //oRes.cards.Ge
            //if (lBarCode.Count()==0)// string.IsNullOrEmpty(pBarCode))
            //pBarCode = Global.GenBarCodeFromPhone(pPh.FullPhone2);

            //oRes.cards.First
            foreach (var el in oRes.cards)
            {

                FileLogger.WriteLogMessage($"GetBonusAsync Start BarCode=>{el.card}");
                try
                {
                    string res;
                    el.pathCard = GetBarCode(el.card);
                    decimal Sum;
                    var body = SoapTo1C.GenBody("GetBonusSum", new Parameters[] { new Parameters("CodeOfCard", el.card) });
                    var res1C = await SoapTo1C.RequestAsync(Global.Server1C, body,3000);
                    if (res1C.status)
                    {
                        res = res1C.Data.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                        if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                            el.bonus = Sum; //!!!TMP
                    }
                    body = SoapTo1C.GenBody("GetMoneySum", new Parameters[] { new Parameters("CodeOfCard", el.card) });
                    res1C = await SoapTo1C.RequestAsync(Global.Server1C, body,3000);

                    res = res1C.Data.Replace(".", Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    if (!string.IsNullOrEmpty(res) && decimal.TryParse(res, out Sum))
                        el.rest = Sum;
                    //Global.OnClientChanged?.Invoke(parClient, parTerminalId);

                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage($"GetBonusAsync BarCode=>{el.card} Error =>{ex.Message}");
                    oRes.State = -1;
                    oRes.TextState = ex.Message;
                    return oRes;
                    // Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = parTerminalId, Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
                }
            }
            return oRes;
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

        void Merge(Image playbutton, string FileName)
        {
            int width = 350, height = 400;
            Image frame;
            try
            {
                frame = Image.FromFile(@"img/BarCode/Spar-logo.png");
            }
            catch (Exception ex)
            {
                return;
            }

            using (frame)
            {
                using (var bitmap = new Bitmap(width, height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(frame,
                                         new Rectangle(0, 0, width, height),
                                         new Rectangle(0, 0, frame.Width, frame.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(playbutton, 10, 100);
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save(FileName, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

        }

        public InfoForRegister GetInfoForRegister()
        {
            return new InfoForRegister()
            {
                locality = Global.Citys /*msSQL.GetLocality()*/,
                typeOfEmployment = new TypeOfEmployment[]
                {
                    new TypeOfEmployment { Id = 1, title = "не працюючий" },
                    new TypeOfEmployment { Id = 2, title = "працюючий" },
                    new TypeOfEmployment { Id = 3, title = "студент" },
                    new TypeOfEmployment { Id = 4, title = "пенсіонер" },

                }
            };
        }

        public UtilNetwork.Result<string> SetActiveCard(InputCard pCard)
        {
            msSQL.SetActiveCard(pCard);
            return new UtilNetwork.Result<string>();
        }

        private Product YellowPrice()
        {
            var pathDirection = @"img\Dir";
            var pathWares = @"img\Wares";
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
            if (pBarCode.Substring(0, 1).Equals("+"))
                pBarCode = pBarCode.Substring(1);
            string FileName = $"img/BarCode/{pBarCode.Replace("*", "_")}.png";
            if (File.Exists(FileName))
                return FileName;
            try
            {
                Bitmap Logo = new Bitmap(Image.FromFile(@"img/BarCode/Spar.png"));
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode($"{pBarCode}", QRCodeGenerator.ECCLevel.H);
                var qrCode = new QRCode(qrCodeData);
                qrCode.GetGraphic(12, Color.FromArgb(0, 123, 62), System.Drawing.Color.White, Logo, 25, 1).Save(FileName, System.Drawing.Imaging.ImageFormat.Png);

                //Merge(el, FileName);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage($"GetBarCode BarCode=>{pBarCode} FileName=>{FileName} Error =>{ex.Message}");
                return null;
            }
            return FileName;

        }

        private Product NewsPaper()
        {
            string pathDir = @"img\";

            var Dirs = Directory.GetDirectories(pathDir, "NP*");
            var CodeNP = Dirs.Max(a => int.Parse(new DirectoryInfo(a).Name.Substring(2)));
            string path = Path.Combine(pathDir, $"NP{CodeNP}");

            var Files = Directory.GetFiles(path, "p?.jpg");
            if (Files == null || Files.Length == 0)
                return null;
            var Res = new Product() { name = "Газета", id = -1, folder = true, description = $"№{CodeNP}", img = Files.First().Replace("\\", "/") };

            Res.folderItems = Files.Select(a => Product.GetFileName(Path.Combine(a))).ToArray();
            for (int i = 0; i < Res.folderItems.Length; i++)
            {
                Files = Directory.GetFiles(path, $"p{-Res.folderItems[i].id}_*.???");
                Res.folderItems[i].folderItems = Files.Select(a => Product.GetPicture(Path.Combine(a))).ToArray();
                //var r2 = JsonConvert.SerializeObject(el);
            }
            ;
            //var r = JsonConvert.SerializeObject(Res);
            return Res;
        }


    }
}
