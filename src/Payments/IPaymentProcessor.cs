using System;
using System.Web;

namespace Payments
{
    public interface IPaymentProcessor
    {
        Uri Buy(string customerId, string prodId, Decimal paymentAmount);

        IPayment Confirm(HttpContext context);

        void Cancel(HttpContext context);
    }
}
