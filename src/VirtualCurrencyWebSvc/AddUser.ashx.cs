using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using log4net;
using System.Reflection;
using VirtualCurrencyWebSvc.Data;
using CommonUtils;
using UserManagement;
using VirtualCurrencyWebSvc.Util;
using System.Net.Mime;
using CustomerManagement;
using CustomerManagement.Model;
using System.Runtime.Serialization;

namespace VirtualCurrencyWebSvc
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class AddUser : IHttpHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            string result = null;

            try
            {
                SiteResponse response;
                string username = context.Param(SiteParameters.USER_NAME);
                string email = context.Param(SiteParameters.EMAIL);
                string password = context.Param(SiteParameters.PASSWORD);
                var appKey = new AppKey(context.Param(SiteParameters.APP_KEY));

                var user = UserManagementService.CreateUser(username, email, password, appKey);

                response = new SiteResponse
                {
                    response = user,
                    status = SiteResponse.Status.Success,
                    syncKey = "are we using this?"
                };

                result = JSONHelper.Serialize<SiteResponse>(response, new []{typeof(User)});
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

        private SiteResponse ChangePassword(HttpContext context)
        {
            string username = context.Param(SiteParameters.USER_NAME);
            string oldPwd = context.Param(SiteParameters.PASSWORD);
            string newPwd = context.Param(SiteParameters.NEW_PASSWORD);
            var lt = JSONHelper.Deserialize<LoginToken>(context.Param(SiteParameters.LOGIN_TOKEN));

            if (!UserManagementService.ValidateLoginToken(lt))
                throw new ArgumentException("logintoken");

            UserManagementService.ChangeUserPassword(username, oldPwd, newPwd);

            if (log.IsInfoEnabled)
                log.Info("PasswordChange status: success");

            return new SiteResponse()
            {
                response = "Password has been changed.",
                status = SiteResponse.Status.Success,
                syncKey = "aSyncKey"
            };
        }

        private static SiteResponse _ResetPassword(HttpContext context)
        {
            string email = string.Empty;
            string username = string.Empty;

            if (context.HasParam(SiteParameters.EMAIL))
                email = context.Param(SiteParameters.EMAIL);

            if (context.HasParam(SiteParameters.USER_NAME))
                username = context.Param(SiteParameters.USER_NAME);

            SessionUtil.ResetPassword(username, email);

            if (log.IsInfoEnabled)
                log.Info("PasswordReset status: success");

            return new SiteResponse()
            {
                response = "Please check email for new password.",
                status = SiteResponse.Status.Success,
                syncKey = "aSyncKey"
            };
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
