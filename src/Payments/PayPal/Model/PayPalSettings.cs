using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Payments.PayPal.Model
{
    public interface PayPalSettings
    {
        /// <summary>
        /// The PayPal API username
        /// </summary>
        string Username { get; }
        /// <summary>
        /// The PayPal API password (This should be encrypted)
        /// </summary>
        string Password { get; }
        /// <summary>
        /// The PayPal API signature (This should be encrypted)
        /// </summary>
        string Signature { get; }
        /// <summary>
        /// The PayPal API version 
        /// </summary>
        string Version { get; }
        /// <summary>
        /// The PayPal API Environment 
        /// </summary>
        string Environment { get; }
        /// <summary>
        /// The CurrencyTypeCode
        /// </summary>
        string CurrencyCode { get; }
        /// <summary>
        /// Specifies whether to use the PayPal production environment or the PayPal developer sandbox
        /// </summary>
        bool TestMode { get; }
        /// <summary>
        /// The web address that should be redirected to after a user has made a successful PayPal payment
        /// </summary>
        string ReturnUrl { get; }
        /// <summary>
        /// The web address that will handle PayPal IPN notifications
        /// </summary>
        string NotifyUrl { get; }
        /// <summary>
        /// The web address that should be redirected to if a user cancels the payment process whilst on PayPal's payment pages
        /// </summary>
        string CancelUrl { get; }
    }
}
