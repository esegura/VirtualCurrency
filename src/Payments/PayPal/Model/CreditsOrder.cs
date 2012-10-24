using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CustomerManagement.Model;

namespace Payments.PayPal.Model
{
    /// <summary>
    /// Represents an order made by a customer in order to buy
    /// more credits.
    /// </summary>
    public class CreditsOrder
    {
        private string   orderID;
        private string   amount;
        private Customer customer;
        public CreditsOrder(Customer customer, string orderID, string amount)
        {
            this.customer = customer;
            this.orderID  = orderID;
            this.amount   = amount;
        }

        public string Amount
        {
            get { return amount;  }
        }

        public string OrderID
        {
            get { return orderID; }
        }

        public Customer Customer
        {
            get { return customer; }
        }
    }
}
