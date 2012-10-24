using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using UserManagement;
using CommonUtils;
using System.Net.Mime;
using VirtualCurrencyWebSvc.Data;
using log4net;
using System.Reflection;
using VirtualCurrencyWebSvc.Util;

namespace VirtualCurrencyWebSvc
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Service : IHttpHandler
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            string resultStr = GetDefaultSiteResponse();

            try
            {
                var operation = context.Param<SiteOperation>(SiteParameters.OPERATION);

                switch (operation)
                {
                    case SiteOperation.CreateUser:
                        CreateUser(context);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Operation");
                }
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

                resultStr = JSONHelper.Serialize(ExceptionHelper.Handle(umse, exceptionCode, message, log));
            }
            catch (Exception e)
            {
                resultStr = JSONHelper.Serialize(ExceptionHelper.Handle(e, log));
            }
            finally
            {
                context.Response.ContentType = MediaTypeNames.Text.Plain;
                context.Response.Write(resultStr);
            }
        }

        private string GetDefaultSiteResponse()
        {
            return JSONHelper.Serialize(new SiteResponse
            {
                response = "Success",
                status = SiteResponse.Status.Success,
                syncKey = "Are we using this?"
            });
        }

        private void CreateUser(HttpContext context)
        {
            string username = context.Param(SiteParameters.USER_NAME);
            string email = context.Param(SiteParameters.USER_EMAIL);
            string password = context.Param(SiteParameters.PASSWORD);
            string appKeyStr = context.Param(SiteParameters.APP_KEY);
            var appKey = new AppKey(appKeyStr);

            UserManagementService.CreateUser(username, email, password, appKey);
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
