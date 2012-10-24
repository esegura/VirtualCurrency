using System;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using System.Web.Services;
using CommonUtils;
using CustomerManagement;
using CustomerManagement.Model;
using log4net;
using Payments;
using UserManagement;
using VirtualCurrencyWebSvc.Data;
using VirtualCurrencyWebSvc.Util;
using System.Collections.Generic;

namespace VirtualCurrencyWebSvc
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class GetAllProducts : IHttpHandler
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            string result = null;

            try
            {
                string loginToken = context.Param(SiteParameters.LOGIN_TOKEN);
                var lt = JSONHelper.Deserialize<LoginToken>(loginToken);
                VerifySession(lt);
                List<Item> allProducts = CustomerManagementService.GetAllProducts();
                result = JSONHelper.Serialize<List<Item>>(allProducts);
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

        private void VerifySession(LoginToken lt)
        {
            bool isValid = UserManagementService.ValidateLoginToken(lt);

            if (!isValid)
                throw new ApplicationException("Invalid session");
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
