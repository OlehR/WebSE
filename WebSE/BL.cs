using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSE.Controllers;

namespace WebSE
{
    public class BL
    {
        SoapTo1C soapTo1C = new SoapTo1C();
        MsSQL msSQL = new MsSQL();

        public Status Auth(InputPhone pIPh)
        {
            var r = msSQL.Auth(pIPh.ShortPhone);
            return new Status(r);
        }


        public Status Register( RegisterUser pUser)
        {
            var rdd = new InputPhone() {phone=pUser.phone };
            var r = msSQL.Auth(rdd.ShortPhone);
            if (r)
                return new Status();
            return new Status(msSQL.Register(pUser));
        }

        public async Task<InfoBonus> GetBonusAsync(string pBarCode)
        {
            InfoBonus Res = new InfoBonus() { card = pBarCode };
            try
            {
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
                // Global.OnSyncInfoCollected?.Invoke(new SyncInformation { TerminalId = parTerminalId, Exception = ex, Status = eSyncStatus.NoFatalError, StatusDescription = ex.Message });
            }

            return Res;
        }
        public object GetPromotion()
        {
            var Newspaper = NewsPaper();
            var Yellow = YellowPrice();// new Product() { name = "Жовті цінники", id = -3, folder = true, description = "", img = "Y.png" };
            return new { products = new Product[] { Newspaper, Yellow } };
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
                Res.folderItems[i].folderItems = Files.Select(a => Product.GetPicture(Path.Combine(path, a))).ToArray();
                //var r2 = JsonConvert.SerializeObject(el);
            };
            var r = JsonConvert.SerializeObject(Res);
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
    }
}
