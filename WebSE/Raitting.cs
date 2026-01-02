using System;
using System.Collections;
using System.Collections.Generic;
using BRB5.Model;
using UtilNetwork;
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

        public  Result SaveDocRaiting(Doc pDoc) 
        {
            try
            {
                db.ReplaceRaitingDoc(pDoc);
                return new Result();
            }
            catch (Exception e) { return new Result(e); }

        }

        public Result<IEnumerable<RaitingTemplate>> GetRaitingTemplate() 
        {
            try
            {
                return new() { Data = db.GetRaitingTemplate() };
            }
            catch (Exception e) { return new(e); }
        }
        public IEnumerable<Doc> GetRaitingDocs()
        {
            return db.GetRaitingDocs();
        }

    }
}
