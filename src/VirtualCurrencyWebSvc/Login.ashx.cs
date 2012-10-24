using System;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using CommonUtils;
using log4net;
using UserManagement;
using VirtualCurrencyWebSvc.Data;
using VirtualCurrencyWebSvc.Util;

namespace VirtualCurrencyWebSvc
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Login : IHttpHandler
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            string result = null;

            try
            {
                string username;
                
                if (context.HasParam(SiteParameters.USER_NAME))
                    username = context.Param(SiteParameters.USER_NAME);
                else
                {
                    string email = context.Param(SiteParameters.EMAIL);
                    var user = UserManagementService.FindUserWithEmail(email);
                    username = user.AliasName;
                }

                string password = context.Param(SiteParameters.PASSWORD);

                var lt = SessionUtil.Login(username, password);
                result = JSONHelper.Serialize<LoginToken>(lt);
            }
            catch (UserManagementServiceException umse)
            {
                ExceptionHelper.Code exceptionCode = ExceptionHelper.Code.UnexpectedException;
                string message = umse.Message;

                switch (umse.Code)
                {
                    case UserManagementServiceException.ErrorCode.UnexpectedError:
                        break;
                    case UserManagementServiceException.ErrorCode.ObjectNotFound:
                        exceptionCode = ExceptionHelper.Code.InvalidLogin;
                        break;
                    case UserManagementServiceException.ErrorCode.InvalidOperationOnResource:
                        exceptionCode = ExceptionHelper.Code.InvalidOperation;
                        break;
                    case UserManagementServiceException.ErrorCode.AccessDenied:
                        exceptionCode = ExceptionHelper.Code.AccessDenied;
                        break;
                    case UserManagementServiceException.ErrorCode.CouldNotConnectToDatabase:
                        message = "Could not connect to the database. " + message;
                        break;
                    default:
                        message = "Unknown ErrorCode: " + umse.Code + ". Message: " + message;
                        break;
                }

                result = JSONHelper.Serialize(ExceptionHelper.Handle(umse, exceptionCode, message, log));
            }
            catch (Exception e)
            {
                result = JSONHelper.Serialize(ExceptionHelper.Handle(e, log));
            }
            finally
            {
                context.Response.ContentType = MediaTypeNames.Text.Plain;
                context.Response.Write(result);
            }
        }

        private MembershipProvider ValidateAgainstAllMembershipProviders(string username, string password)
        {
            foreach (MembershipProvider provider in Membership.Providers)
            {
                try
                {
                    if (provider.ValidateUser(username, password))
                        return provider;
                }
                catch (Exception e)
                {
                    if (log.IsInfoEnabled) log.Info("The provider '" + provider.Name + "' threw exception for username='" + username + "' and password='" + password + "'", e);
                    throw;
                }
            }

            return null;
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
