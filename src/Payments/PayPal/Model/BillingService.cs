using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.PayPal.Model
{
    public interface BillingService
    {
        /// <summary>
        /// make the payments in a Firt in First out fashion.
        /// </summary>
        /// <param name="payments">a FIFO payment queue</param>
        /// <returns>a list of confirmation messages</returns>
        List<Receipt> processPayments(PaymentQueue payments);
        /// <summary>
        /// charge a credit card the amount stated in the credits order.
        /// </summary>
        /// <param name="order">a pucharse of credits</param>
        /// <param name="creditCard">credit card information</param>
        /// <returns>a confirmation message</returns>
        Receipt chargeOrder(CreditsOrder order, CreditCard creditCard);
    }
}
