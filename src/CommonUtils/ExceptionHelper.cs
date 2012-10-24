using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CommonUtils
{
    public class ExceptionHelper
    {
        public enum Code
	    {
            UnexpectedException,
            InvalidArgument,
            InvalidOperation,
            InvalidLogin,
            AccessDenied,
            ObjectNotFound,
            ArgumentException,
            EmailIsTaken,
            UsernameIsTaken
	    }

        [DataContract]
        public struct ExceptionData
        {
            [DataMember] public string ExceptionCode;
            [DataMember] public string Message;
            [DataMember] public string StackTrace;

            internal ExceptionData(Exception e, string message, Code ec)
            {
                this.Message = message;
                this.StackTrace = e.StackTrace;
                this.ExceptionCode = ec.ToString();
            }
        }

        public static ExceptionData Handle(Exception e, Code? ec, string message, log4net.ILog log)
        {
            if ((log != null) && (log.IsWarnEnabled))
            {
                // If the client code has mapped the exception to a code and a message, it means some handling is in place. Use that info.
                if (ec != null)
                    log.Warn(String.Format("Code={0}; Message={1}", ec, message));
                else
                    log.Warn(e);    // if not, just log the full exception stack
            }

            return new ExceptionData(e, message ?? e.Message, ec ?? MapExceptionToExceptionCode(e));
        }

        public static ExceptionData Handle(Exception e, Code ec, string message)
        {
            return Handle(e, ec, message + " ---> " + e.Message, null);
        }

        public static ExceptionData Handle(Exception e, string message, log4net.ILog log)
        {
            return Handle(e, null, message + " ---> " + e.Message, log);
        }

        public static ExceptionData Handle(Exception e, Code ec, log4net.ILog log)
        {
            return Handle(e, ec, e.Message, log);
        }

        public static ExceptionData Handle(Exception e, Code ec)
        {
            return Handle(e, ec, e.Message, null);
        }

        public static ExceptionData Handle(Exception e, log4net.ILog log)
        {
            return Handle(e, null, e.Message, log);
        }

        public static ExceptionData Handle(Exception e)
        {
            return Handle(e, null, e.Message, null);
        }

        private static Code MapExceptionToExceptionCode(Exception e)
        {
            return (e is ArgumentException) ? Code.InvalidArgument :
                (e is ArgumentNullException) ? Code.InvalidArgument :
                (e is ArgumentOutOfRangeException) ? Code.InvalidArgument :
                (e is InvalidOperationException) ? Code.InvalidOperation :
                Code.UnexpectedException;
        }


    }
}
