using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Payments.PayPal.Model
{
    interface PaymentProcessor
    {
        /// <summary>
        /// The customer's credit card. This card should not be persisted; period.
        /// </summary>
        CreditCard CreditCard { get; }
        /// <summary>
        /// charge the amount to the given credit card.
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        ChargeResult charge(string amount);
    }
}
