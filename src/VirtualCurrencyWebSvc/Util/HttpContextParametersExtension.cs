using System;
using System.Web;

namespace VirtualCurrencyWebSvc.Util
{
    internal static class HttpContextParametersExtension
    {
        internal static string Param(this HttpContext c, Data.SiteParameters sp)
        {
            string paramValue = c.Request[sp.ToString()];

            if (string.IsNullOrEmpty((paramValue ?? "").Trim()))
                throw new ArgumentException("Missing expected parameter: " + sp.ToString());

            return paramValue;
        }

        internal static T Param<T>(this HttpContext c, Data.SiteParameters sp) where T : struct
        {
            string paramValueStr = c.Request[sp.ToString()];

            if (string.IsNullOrEmpty((paramValueStr ?? "").Trim()))
                throw new ArgumentException("Missing expected parameter: " + sp.ToString());

            if (!Enum.IsDefined(typeof(T), paramValueStr))
                throw new ArgumentOutOfRangeException("Invalid parameter value: " + paramValueStr);

            return (T)Enum.Parse(typeof(T), paramValueStr);
        }

        internal static bool HasParam(this HttpContext c, Data.SiteParameters sp)
        {
            string paramValue = c.Request[sp.ToString()];

            if (string.IsNullOrEmpty(paramValue))
                return false;

            if (string.Empty == paramValue.Trim())
                return false;

            return true;
        }

        internal static string Header(this HttpContext c, string headerKey)
        {
            string paramValue = c.Request.Headers[headerKey];

            if (string.IsNullOrEmpty((paramValue ?? "").Trim()))
                throw new ArgumentException("Missing expected header: " + headerKey);

            return paramValue;
        }
    }
}
