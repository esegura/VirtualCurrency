using System;
using System.Net.Mime;
using System.Web;
using System.Web.Services;
using CommonUtils;
using UserManagement;
using VirtualCurrencyWebSvc.Data;
using VirtualCurrencyWebSvc.Util;

namespace VirtualCurrencyWebSvc
{
    [WebService(Namespace = "http://infomotor.com/infomotorapi")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Logout : IHttpHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            string result = string.Empty;

            try
            {
                LoginToken lt = JSONHelper.Deserialize<LoginToken>(context.Param(SiteParameters.LOGIN_TOKEN));
                SessionUtil.Logout(lt);
                result = JSONHelper.Serialize("You have been logged out");
            }
            catch (Exception e)
            {
                result = JSONHelper.Serialize(ExceptionHelper.Handle(e, "Could not find session", log));
            }
            finally
            {
                context.Response.ContentType = MediaTypeNames.Text.Plain;
                context.Response.Write(result);
            }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}
