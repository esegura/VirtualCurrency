using com.paypal.soap.api;
using Payments.PayPal;

namespace Payments.Tests
{
    class PayPalProcessorAccessor : PayPalProcessor
    {
        public new SetExpressCheckoutResponseType ECSetExpressCheckoutCode(string paymentAmount, string returnURL, string cancelURL,
            PaymentActionCodeType paymentAction, CurrencyCodeType currencyCodeType)
        {
            return base.ECSetExpressCheckoutCode(paymentAmount, returnURL, cancelURL, paymentAction, currencyCodeType);
        }

        public new void SavePayPalTransactionStart(string customerId, string prodId, SetExpressCheckoutResponseType response)
        {
            base.SavePayPalTransactionBegin(customerId, prodId, response);
        }
    }
}
