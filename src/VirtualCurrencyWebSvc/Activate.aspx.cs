using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using UserManagement;
using CommonUtils;
using log4net;
using System.Reflection;

namespace VirtualCurrencyWebSvc
{
    public partial class Activate1 : System.Web.UI.Page
    {
        private static readonly log4net.ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public string result;

        protected void Page_Load(object sender, EventArgs ea)
        {
            var context = Context;

            try
            {
                UserManagementService.Activate(context.Request.Params[UserManagementService.ActivationParameter]);
                result = "Your account has been activated.";
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

                result = ExceptionHelper.Handle(umse, exceptionCode, message, log).Message;
            }
            catch (Exception e)
            {
                result = ExceptionHelper.Handle(e, log).Message;
            }
        }
    }
}
