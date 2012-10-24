using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.PayPal.Model
{
    public class PaymentRecord : IPayment
    {
        public string CustomerId { get; internal set; }

        public string ProdId { get; internal set; }

        public string Amount { get; internal set; }
    }
}
