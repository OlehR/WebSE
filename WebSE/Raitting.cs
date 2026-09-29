using System;
using System.Collections;
using System.Collections.Generic;
using BRB5.Model;
using UtilNetwork;
using Utils;
namespace WebSE
{
    public class Raitting
    {
        MsSQL db = new MsSQL();

        public int GetIdRaitingTemplate()
        {
            return db.GetIdRaitingTemplate();
        }

        public int GetNumberDocRaiting()
        {
            return db.GetNumberDocRaiting();
        }

        public Result SaveTemplate(RaitingTemplate pRT)
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

        public Result SaveDocRating(Doc pDoc)
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
        public Result<IEnumerable<Doc>> GetRaitingDocs()
        {
            try
            {
                return new() { Data = db.GetRaitingDocs() };
            }
            catch (Exception e) { return new(e); }
        }

        public async Task<Result> SaveRaitingAsync(RaitingDocForSave pRD)
        {
            try
            {
                return db.SaveRaiting(pRD);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name + " => " + pRD.ToJSON(), ex);
                return new(ex);                
            }
        }
    }
}


