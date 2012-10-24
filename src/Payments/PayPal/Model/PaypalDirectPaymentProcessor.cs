using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using com.paypal.sdk.services;
using com.paypal.soap.api;
using com.paypal.sdk.profiles;

namespace Payments.PayPal.Model
{
    public class PaypalDirectPaymentProcessor : PaymentProcessor
    {
        private CreditCard     creditCard;
        private PayPalSettings settings;
        private CallerServices caller;
        private IAPIProfile    profile;

        public PaypalDirectPaymentProcessor(CreditCard creditCard) :this(
            creditCard, PayPalConfigSettings.Default
        ){}

        public PaypalDirectPaymentProcessor(CreditCard creditCard, PayPalSettings settings)
        {
            this.settings = settings;
            this.caller   = caller;

            profile.APIUsername  = settings.Username;
            profile.APIPassword  = settings.Password;
            profile.APISignature = settings.Signature;
            profile.Environment  = settings.Environment;

            caller.APIProfile = profile;
            this.creditCard = creditCard;

        }

        public CreditCard CreditCard
        {
            get { return creditCard;  }
        }

        public ChargeResult charge(string amount)
        {
            DoDirectPaymentRequestType request = new DoDirectPaymentRequestType();
            request.Version = settings.Version;
            request.DoDirectPaymentRequestDetails = new DoDirectPaymentRequestDetailsType();
            // todo (Huascar) not idea what this value is for us...
            // from here
            request.DoDirectPaymentRequestDetails.IPAddress = "10.244.43.106"; 
            request.DoDirectPaymentRequestDetails.MerchantSessionId = "1X911810264059026";
            // to here
            request.DoDirectPaymentRequestDetails.PaymentAction = creditCard.PaymentActionCode;
            request.DoDirectPaymentRequestDetails.CreditCard = new CreditCardDetailsType();
            request.DoDirectPaymentRequestDetails.CreditCard.CreditCardNumber = creditCard.CreditCardNumber;

            switch (creditCard.CreditCardType)
            {
                case CreditCardType.Visa:
                    request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.Visa;
                    break;
                case CreditCardType.MasterCard:
                    request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.MasterCard;
                    break;
                case CreditCardType.Discover:
                    request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.Discover;
                    break;
                case CreditCardType.Amex:
                    request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.Amex;
                    break;
            }
             
            // todo (Huascar) implement the rest....to the point when we call the Paypal api
            ChargeResult result = new ChargeResult();
            return result;

        }

    }
}
