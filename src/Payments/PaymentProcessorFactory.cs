using System;
using Payments.PayPal;

namespace Payments
{
    public static class PaymentProcessorFactory
    {
        public static IPaymentProcessor Create(PaymentProcessorType ppt)
        {
            switch (ppt)
            {
                case PaymentProcessorType.PayPal:
                    return new PayPalProcessor();
                default:
                    throw new ApplicationException("PaymentProcessorType not found: " + ppt);
            }
        }
    }
}
