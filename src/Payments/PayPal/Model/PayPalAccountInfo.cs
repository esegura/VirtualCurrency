using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.paypal.sdk.services;
using com.paypal.soap.api;
using com.paypal.sdk.profiles;

namespace Payments.PayPal.Model
{
    public class PayPalAccountInfo
    {
        private PayPalSettings settings;
        private CallerServices caller;
        private IAPIProfile    profile;

        public PayPalAccountInfo(PayPalSettings settings) :this(
            settings, 
            ProfileFactory.createSignatureAPIProfile(), 
            new CallerServices()
        ){}

        public PayPalAccountInfo(PayPalSettings settings, IAPIProfile profile, CallerServices caller)
        {
            this.settings = settings;
            this.caller   = caller;

            profile.APIUsername  = settings.Username;
            profile.APIPassword  = settings.Password;
            profile.APISignature = settings.Signature;
            profile.Environment  = settings.Environment;

            caller.APIProfile = profile;

        }
        /// <summary>
        /// show PayPal account balance
        /// </summary>
        /// <returns>a balance code</returns>
        public string ShowBalance()
        {
            GetBalanceRequestType request = new GetBalanceRequestType();
            request.Version = settings.Version;
            GetBalanceResponseType response = new GetBalanceResponseType();
            response = (GetBalanceResponseType)caller.Call("GetBalance", request);
            return response.Ack.ToString();

        }
        /// <summary>
        /// Get a detail information about a transaction.
        /// </summary>
        /// <param name="transactionID">ID of transaction of interest.</param>
        /// <returns>a transaction details code</returns>
        public string getTransactionDetails(string transactionID)
        {
            GetTransactionDetailsRequestType request = new GetTransactionDetailsRequestType();
            request.TransactionID = transactionID;
            request.Version = settings.Version;
            GetTransactionDetailsResponseType response = new GetTransactionDetailsResponseType();
            response = (GetTransactionDetailsResponseType)caller.Call("GetTransactionDetails", request);
            return response.Ack.ToString();
        }
    }
}
