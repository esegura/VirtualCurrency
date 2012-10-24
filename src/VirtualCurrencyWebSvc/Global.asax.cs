using System;
using System.Reflection;
using log4net;

namespace VirtualCurrencyWebSvc
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start(object sender, EventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            log.Info("Application starting...");
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            log.Debug(MethodBase.GetCurrentMethod().Name);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            log.Debug(MethodBase.GetCurrentMethod().Name);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            log.Debug(MethodBase.GetCurrentMethod().Name);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            log.Debug(MethodBase.GetCurrentMethod().Name);
        }

        protected void Session_End(object sender, EventArgs e)
        {
            log.Debug(MethodBase.GetCurrentMethod().Name);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            log.Info("Application stopped.");
        }
    }
}