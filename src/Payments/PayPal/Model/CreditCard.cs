using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CustomerManagement;
using com.paypal.sdk.services;
using com.paypal.soap.api;
using com.paypal.sdk.profiles;

namespace Payments.PayPal.Model
{
    public class CreditCard
    {

        public CreditCard(){}

        public string CreditCardNumber { get; private set; }
        public string CustomerFirstName { get; private set; }
        public string CustomerMiddleName { get; private set; }
        public string CustomerLastName { get; private set; }
        public int    CreditCardExpirationMonth { get; private set; }
        public int    CreditCardExpirationYear { get; private set; }
        public string CustomerStreetAddress { get; private set; }
        public string CustomerStreetAddressAddional { get; private set; }
        public string CustomerCity { get; private set; }
        public string CustomerState { get; private set; }
        public string CustomerZipcode { get; private set; }
        public string CustomerCountryCode { get; private set; }
        public string CVV2 { get; private set; }
        public PaymentActionCodeType PaymentActionCode { get; private set; }
        public CreditCardType CreditCardType { get; private set; }


        /// <summary>
        /// Builder pattern as indicated by Joshua Bloch to avoid 
        /// the telescoping signature pattern in constructors (i.e.,
        /// more than 3 parameters in a constructor and/or method 
        /// signature).
        /// </summary>
        public sealed class Builder {
            private CreditCard creditCard = new CreditCard();
            public string CreditCardNumber { 
                get{return creditCard.CreditCardNumber;}
                set { creditCard.CreditCardNumber = value; } 
            }
            public string CustomerFirstName {
                get { return creditCard.CustomerFirstName; }
                set { creditCard.CustomerFirstName = value; } 
            }
            public string CustomerMiddleName {
                get { return creditCard.CustomerFirstName; }
                set { creditCard.CustomerFirstName = value; } 
            }
            public string CustomerLastName {
                get { return creditCard.CustomerLastName; }
                set { creditCard.CustomerLastName = value; } 
            }
            public int CreditCardExpirationMonth {
                get { return creditCard.CreditCardExpirationMonth; }
                set { creditCard.CreditCardExpirationMonth = value; } 
            }
            public int CreditCardExpirationYear {
                get { return creditCard.CreditCardExpirationYear; }
                set { creditCard.CreditCardExpirationYear = value; } 
            }
            public string CustomerStreetAddress {
                get { return creditCard.CustomerStreetAddress; }
                set { creditCard.CustomerStreetAddress = value; } 
            }
            public string CustomerStreetAddressAddional {
                get { return creditCard.CustomerStreetAddressAddional; }
                set { creditCard.CustomerStreetAddressAddional = value; } 
            }
            public string CustomerCity {
                get { return creditCard.CustomerCity; }
                set { creditCard.CustomerCity = value; } 
            }
            public string CustomerState {
                get { return creditCard.CustomerState; }
                set { creditCard.CustomerState = value; } 
            }
            public string CustomerZipcode {
                get { return creditCard.CustomerZipcode; }
                set { creditCard.CustomerZipcode = value; } 
            }
            public string CustomerCountryCode {
                get { return creditCard.CustomerCountryCode; }
                set { creditCard.CustomerCountryCode = value; } 
            }
            public string CVV2 {
                get { return creditCard.CVV2; }
                set { creditCard.CVV2 = value; } 
            }
            public PaymentActionCodeType PaymentActionCode
            {
                get { return creditCard.PaymentActionCode; }
                set { creditCard.PaymentActionCode = value; } 
            }
            public CreditCardType CreditCardType {
                get { return creditCard.CreditCardType; }
                set { creditCard.CreditCardType = value; } 
            }

            public CreditCard Build()
            {
                CreditCard current = creditCard;
                creditCard = null;
                return current;
            }
        } 

        
    }
    /// <summary>
    /// A credit card type. The values of this class match the values of 
    /// com.paypal.soap.api.CreditCardTypeType. The values of Switch and
    /// Solo are not included.
    /// </summary>
    public enum CreditCardType {
        Visa = 0,
        MasterCard = 1,
        Discover = 2,
        Amex = 3
    }

}
