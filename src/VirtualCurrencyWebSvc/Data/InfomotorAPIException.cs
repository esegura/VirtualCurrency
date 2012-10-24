using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CommonUtils;
using UserManagement;
using RoleManagement;

namespace VirtualCurrencyWebSvc.Data
{
    [global::System.Serializable]
    public class InfomotorAPIException : Exception
    {
        public enum ErrorCode
        {
            UnexpectedError,
            ObjectNotFound,
            InvalidOperationOnResource,
            AccessDenied,
            ArgumentException
        }

        public ErrorCode Code { get; set; }

        internal InfomotorAPIException(ErrorCode e) : base(e.ToString()) { this.Code = e; }
        internal InfomotorAPIException(ErrorCode e, string message) : base(message) { this.Code = e; }
        internal InfomotorAPIException(ErrorCode e, Exception inner) : base(e.ToString(), inner) { this.Code = ErrorCode.UnexpectedError; }
        internal InfomotorAPIException(ErrorCode e, string message, Exception inner) : base(message, inner) { this.Code = e; }
        protected InfomotorAPIException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        internal InfomotorAPIException(Exception ex) : base(ex.Message, ex) 
        { 
            if (ex is UserManagementServiceException)
                this.Code = this.MapCodeForUserManagementServiceException(ex as UserManagementServiceException);
            else if (ex is RoleManagementServiceException)
                this.Code = this.MapCodeForRoleManagementServiceException(ex as RoleManagementServiceException);
            else if (ex is ArgumentException)
                this.Code = ErrorCode.ArgumentException;
            else if (ex is FormatException)
                this.Code = ErrorCode.ArgumentException;
            else
            {
                this.Code = ErrorCode.UnexpectedError;
            }
        }

        private ErrorCode MapCodeForUserManagementServiceException(UserManagementServiceException umse)
        {
            switch (umse.Code)
            {
                case UserManagementServiceException.ErrorCode.UnexpectedError:
                    return ErrorCode.UnexpectedError;
                case UserManagementServiceException.ErrorCode.ObjectNotFound:
                    return ErrorCode.ObjectNotFound;
                case UserManagementServiceException.ErrorCode.InvalidOperationOnResource:
                    return ErrorCode.InvalidOperationOnResource;
                case UserManagementServiceException.ErrorCode.AccessDenied:
                    return ErrorCode.AccessDenied;
                case UserManagementServiceException.ErrorCode.CouldNotConnectToDatabase:
                    return ErrorCode.UnexpectedError;
                default:
                    return ErrorCode.UnexpectedError;
            }
        }

        private ErrorCode MapCodeForRoleManagementServiceException(RoleManagementServiceException rmse)
        {
            switch (rmse.Code)
            {
                case RoleManagementServiceException.ErrorCode.UnexpectedError:
                    return ErrorCode.UnexpectedError;
                case RoleManagementServiceException.ErrorCode.ObjectNotFound:
                    return ErrorCode.ObjectNotFound;
                case RoleManagementServiceException.ErrorCode.InvalidOperationOnResource:
                    return ErrorCode.InvalidOperationOnResource;
                case RoleManagementServiceException.ErrorCode.AccessDenied:
                    return ErrorCode.AccessDenied;
                default:
                    return ErrorCode.UnexpectedError;
            }
        }

        // Avoid polluting ExceptionHelper with application-specific code. Have each application do their own mapping, and add more codes to
        // ExceptionHelper as needed.
        public ExceptionHelper.Code MapCode()
        {
            switch (this.Code)
            {
                case ErrorCode.UnexpectedError:
                    return ExceptionHelper.Code.UnexpectedException;
                case ErrorCode.ObjectNotFound:
                    return ExceptionHelper.Code.ObjectNotFound;
                case ErrorCode.InvalidOperationOnResource:
                    return ExceptionHelper.Code.InvalidOperation;
                case ErrorCode.AccessDenied:
                    return ExceptionHelper.Code.AccessDenied;
                case ErrorCode.ArgumentException:
                    return ExceptionHelper.Code.ArgumentException;
                default:
                    return ExceptionHelper.Code.UnexpectedException;
            }
        }

    }


}
