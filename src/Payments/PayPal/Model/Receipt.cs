using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Payments.PayPal.Model
{
    public class Receipt
    {
        private string receiptMessage;
        private Receipt(string preprend, int amount, string append):this(
            preprend + amount + append
        ){}

        public Receipt(string message)
        {
            this.receiptMessage = message;
        }

        public static Receipt newSuccessfulCharge(int amount)
        {
            return new Receipt("The total amount of ", amount, " has been successfully charged.");
        }

        public static Receipt newDeclinedCharge(int amount)
        {
            return new Receipt("The total amount of ", amount, " has been declined by billing provider.");
        }

        public static Receipt newSystemFailure(string errorMessage)
        {
            return new Receipt(errorMessage);
        }
    }
}
