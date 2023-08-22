using System;
using BRB5.Model;
namespace WebSE
{
    public class Raitting
    {
        MsSQL db = new MsSQL();

        public  int GetIdRaitingTemplate() 
        {
            return db.GetIdRaitingTemplate();
        }

        public  int GetNumberDocRaiting() 
        { 
            return db.GetNumberDocRaiting(); 
        }

        public  Result SaveTemplate(RaitingTemplate pRT) 
        {
            try
            {
                db.ReplaceRaitingTemplate(pRT);
                db.DeleteRaitingTemplateItem(pRT);
                db.InsertRaitingTemplateItem(pRT.Item);

                return new Result();
            }
            catch (Exception e) { return new Result(e); }
        
        }

        public  Result SaveDocRaiting() { throw new NotImplementedException(); }

        public  IEquatable<RaitingTemplate> GetRaitingTemplate() { throw new NotImplementedException(); }

    }
}
