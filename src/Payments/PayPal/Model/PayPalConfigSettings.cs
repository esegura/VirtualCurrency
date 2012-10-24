using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.PayPal.Model
{
    public class PayPalConfigSettings : PayPalSettings
    {
        private static string BLANK = "";
        private static PayPalConfigSettings DEFAULT = new PayPalConfigSettings();
        public static PayPalConfigSettings Default
        {
            get { return DEFAULT; }
        }   

        public string Username
        {
            // ideally we will use the ConfigurationManager.AppSettings('PayPal.APIUsername')
            // however, this is fine for now.
            get { return "esesel_1263685459_biz_api1.computer.org"; }
        }

        public string Password 
        {
            get { return "FXNAHJBTYDV78EKY"; }
        }

        public string Signature
        {
            get { return "Av1EF0H5TSuNjzjJE9jxc9HZacPmA40e7pFXXvJvGQatESCiRfVIyQoW ";  }
        }

        public string Version
        {
            get { return "51.0"; }
        }

        public string Environment
        {
            get { return "sandbox"; }
        }

        public string CurrencyCode
        {
            // USD = 125
            get { return "125"; }
        }

        public bool TestMode
        {
            get { return true; }
        }

        public string ReturnUrl
        {
            get { return BLANK; }
        }


        public string NotifyUrl
        {
            get { return BLANK; }
        }

        public string CancelUrl
        {
            get { return BLANK; }
        }

        public string LogoUrl
        {
            get { return BLANK; }
        }
    }
}
