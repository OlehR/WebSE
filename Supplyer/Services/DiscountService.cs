using Supplyer.Helpers;
using Supplyer.Models.DiscountModels;
using UtilNetwork;

namespace Supplyer.DiscountService
{
    public class DiscountService
    {
       public Result<IEnumerable<StorageAdressModel>> GetAllMergedDiscounts()
        {
            MSSQL mSSQL = new MSSQL();

            return mSSQL.GetAllAdresses();
        }
        /*   public MargedDiscountModel GetMargedDiscount(string number,string code)
           {
               MSSQL mSSQL= new MSSQL();   
               Oracle oracle = new Oracle();   
               return new MargedDiscountModel(oracle.GetSpecificationByCode(code),mSSQL.GetAdressesByNumber(number),mSSQL.GetDicountPeriodByNumber(number));
           }*/
        public List<MargedDiscountModel> GetAllDiscountRequests(string userName, string passwordClaim,bool IsAllStatus=false)
        {
            List<MargedDiscountModel> margedDiscountModels = new List<MargedDiscountModel>();
            MSSQL mSSQL = new MSSQL();
            Oracle oracle = new Oracle(userName, passwordClaim);
            var requests = oracle.GetAllDiscRequests(IsAllStatus);
            foreach (var request in requests.Data)
            {
                var margedModel = new MargedDiscountModel(oracle.GetSpecificationByCode(request.CodeWares), mSSQL.GetAdressesByNumber(request.Number_).Data,
                    mSSQL.GetDicountPeriodByNumber(request.Number_).Data, request.PlannedSales, request.DiscountInitPrice,
                    request.DiscountPrice, request.CompensationAmount, request.Status, request.DiscountComment);
                if (margedModel.adressModel != null && margedModel.discountPeriods != null && margedModel.suplierPostition!=null)
                    margedDiscountModels.Add(margedModel);
            }
            return margedDiscountModels;
        }
        
    }
}
