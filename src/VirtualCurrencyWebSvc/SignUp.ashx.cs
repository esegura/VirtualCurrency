using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Services;
using CommonUtils;
using log4net;
using UserManagement;
using VirtualCurrencyWebSvc.Data;
using VirtualCurrencyWebSvc.Util;
using System.Configuration;

namespace VirtualCurrencyWebSvc
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class SignUp : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            string result = null;

            try
            {
                string emailAsString = context.Param(SiteParameters.EMAIL);
                MailAddress email = new MailAddress(emailAsString);
                string username = context.Param(SiteParameters.USER_NAME);
                string password = context.Param(SiteParameters.PASSWORD);
                var currentSite = new StringBuilder(context.Request.Url.GetLeftPart(UriPartial.Authority));

                // fail fast if either the username or email already exist in the system.
                if (SessionUtil.IsUsernameTaken(username))
                {
                    string usernameTakenResponseMessage = "The username " + username + " already exists in this system.";
                    throw new ApplicationException(usernameTakenResponseMessage);
                }
                
                if (SessionUtil.IsEmailTaken(emailAsString))
                {
                    string emailTakenResponseMessage = "The e-mail " + emailAsString + " already exists in this system.";
                    throw new ApplicationException(emailTakenResponseMessage);
                }
                
                for (int i = 0; i < context.Request.Url.Segments.Length - 1; i++)
                    currentSite.Append(context.Request.Url.Segments[i]);

                currentSite.Append("Activate.aspx");
                Uri callbackLink = new Uri(currentSite.ToString());

                UserManagementService.SignUp(email, username, password, callbackLink);

                var response = new SiteResponse()
                {
                    response = "Please check email for activation link.",
                    status = SiteResponse.Status.Success,
                    syncKey = "aSyncKey"
                };

                result = JSONHelper.Serialize<SiteResponse>(response);
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

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
