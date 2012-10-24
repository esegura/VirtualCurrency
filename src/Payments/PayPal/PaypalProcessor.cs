using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Reflection;
using System.Web;
using com.paypal.sdk.profiles;
using com.paypal.sdk.services;
using com.paypal.soap.api;
using CommonUtils;
using log4net;
using Payments.Properties;
using Payments.PayPal.Model;

namespace Payments.PayPal
{
    class PayPalProcessor : IPaymentProcessor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Uri Buy(string customerId, string prodId, Decimal paymentAmount)
        {
            string paymentAmountStr = paymentAmount.ToString("F", NumberFormatInfo.InvariantInfo);
            string returnUrl = Settings.Default.PayPalApiReturn;
            string cancelUrl = Settings.Default.PayPalApiCancel;
            var paymentAction = PaymentActionCodeType.Sale;
            var currencyCode = CurrencyCodeType.USD;


            var ppResponse = this.ECSetExpressCheckoutCode(paymentAmountStr, returnUrl, cancelUrl, paymentAction, currencyCode);

            if (ppResponse.Ack == AckCodeType.Success)
            {
                // save and redirect
                SavePayPalTransactionBegin(customerId, prodId, ppResponse);
                string payPalServer = Settings.Default.PayPalApiServer;
                string payPalCheckoutCmd = "/webscr?cmd=_express-checkout&token=" + ppResponse.Token;
                return new Uri(payPalServer + payPalCheckoutCmd);
            }
            else
            {
                // save and show error
                SavePayPalTransactionBeginError(customerId, prodId, ppResponse);
                throw new ApplicationException("PayPal returned error: " + ppResponse.Errors[0].LongMessage);
            }
        }

        protected SetExpressCheckoutResponseType ECSetExpressCheckoutCode(string paymentAmount, string returnURL, string cancelURL,
            PaymentActionCodeType paymentAction, CurrencyCodeType currencyCodeType)
        {
            CallerServices caller = CreateCaller();

            var pp_request = new SetExpressCheckoutRequestType()
            {
                Version = "51.0",
                SetExpressCheckoutRequestDetails = new SetExpressCheckoutRequestDetailsType()
                {
                    PaymentAction = paymentAction,
                    PaymentActionSpecified = true,
                    OrderTotal = new BasicAmountType()
                    {
                        currencyID = currencyCodeType,
                        Value = paymentAmount
                    },
                    CancelURL = cancelURL,
                    ReturnURL = returnURL,
                }
            };


            return (SetExpressCheckoutResponseType)caller.Call("SetExpressCheckout", pp_request);
        }

        private static CallerServices CreateCaller()
        {
            CallerServices caller = new CallerServices();
            IAPIProfile profile = ProfileFactory.createSignatureAPIProfile();
            profile.APIUsername = Settings.Default.PayPalApiUsername;
            profile.APIPassword = Settings.Default.PayPalApiPassword;
            profile.APISignature = Settings.Default.PayPalApiSignature;
            profile.Environment = Settings.Default.PayPalApiEnvironment;
            caller.APIProfile = profile;
            return caller;
        }

        protected void SavePayPalTransactionBegin(string customerId, string prodId, SetExpressCheckoutResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);
            Log.InfoFormat("Saving new PayPal transaction: customerId={0}, prodId={1}, response={2}",
                customerId, prodId, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutBegin (CustomerId, ProdId, Token, Response)" +
                                      "VALUES (@customerId, @prodId, @token, @response)";
                    var customerIdParam = cmd.Parameters.Add("@customerId", SqlDbType.VarChar);
                    customerIdParam.Value = customerId;

                    var prodIdParam = cmd.Parameters.Add("@prodId", SqlDbType.VarChar);
                    prodIdParam.Value = prodId;

                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = response.Token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }

        protected void SavePayPalTransactionBeginError(string customerId, string prodId, SetExpressCheckoutResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);
            Log.InfoFormat("Saving new-with-error PayPal transaction: customerId={0}, prodId={1}, response={2}",
                customerId, prodId, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutBeginError (CustomerId, ProdId, Token, Response)" +
                                      "VALUES (@customerId, @prodId, @token, @response)";
                    var customerIdParam = cmd.Parameters.Add("@customerId", SqlDbType.VarChar);
                    customerIdParam.Value = customerId;

                    var prodIdParam = cmd.Parameters.Add("@prodId", SqlDbType.VarChar);
                    prodIdParam.Value = prodId;

                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = response.Token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }

        public IPayment Confirm(HttpContext context)
        {
            string token = context.Request.Params["token"];
            var checkoutDetails = ECGetExpressCheckoutCode(token);

            if (checkoutDetails.Ack != AckCodeType.Success)
            {
                SavePayPalTransactionDetailsError(checkoutDetails);
                throw new ApplicationException("Payment fetching failed. " + checkoutDetails.Errors[0].LongMessage);
            }

            SavePayPalTransactionDetails(checkoutDetails);
            
            string payerId = checkoutDetails.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID;
            string paymentAmount = checkoutDetails.GetExpressCheckoutDetailsResponseDetails.PaymentDetails.OrderTotal.Value;
            var paymentActionCode = PaymentActionCodeType.Sale;
            var currencyCode = checkoutDetails.GetExpressCheckoutDetailsResponseDetails.PaymentDetails.OrderTotal.currencyID;

            var pp_response = this.ECDoExpressCheckoutCode(token, payerId, paymentAmount, paymentActionCode, currencyCode);
            if (pp_response.Ack != AckCodeType.Success)
            {
                SavePayPalTransactionConfirmedWithError(token, pp_response);
                throw new ApplicationException("Payment processing failed. " + pp_response.Errors[0].LongMessage);
            }

            //save to the db and credit account
            SavePayPalTransactionSuccessfulyConfirmed(pp_response);

            var transaction = GetTransaction(token);

            return new PaymentRecord()
            {
                CustomerId = (string)transaction.Tables[0].Rows[0]["CustomerId"],
                ProdId = (string)transaction.Tables[0].Rows[0]["ProdId"],
                Amount = paymentAmount
            };
        }

        private void SavePayPalTransactionDetails(GetExpressCheckoutDetailsResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);
            var details = response.GetExpressCheckoutDetailsResponseDetails;

            Log.InfoFormat("Saving retrieved PayPal transaction: token={0}, response={1}", details.Token, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutRetrieved (Token, Response)" +
                                      "VALUES (@token, @response)";
                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = details.Token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }

        private void SavePayPalTransactionDetailsError(GetExpressCheckoutDetailsResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);
            var details = response.GetExpressCheckoutDetailsResponseDetails;

            Log.InfoFormat("Saving retrieved-with-error PayPal transaction: token={0}, response={1}", details.Token, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutRetrievedError (Token, Response)" +
                                      "VALUES (@token, @response)";
                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = details.Token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }

        private void SavePayPalTransactionSuccessfulyConfirmed(DoExpressCheckoutPaymentResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);
            var details = response.DoExpressCheckoutPaymentResponseDetails;

            Log.InfoFormat("Saving confirmed PayPal transaction: token={0}, response={1}", details.Token, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutConfirmed (Token, Response)" +
                                      "VALUES (@token, @response)";
                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = details.Token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }

        private void SavePayPalTransactionConfirmedWithError(string token, DoExpressCheckoutPaymentResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);

            Log.InfoFormat("Saving confirmed-with-error PayPal transaction: token={0}, response={1}", token, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutConfirmedError (Token, Response)" +
                                      "VALUES (@token, @response)";
                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }

        protected GetExpressCheckoutDetailsResponseType ECGetExpressCheckoutCode(string token)
        {
            var caller = CreateCaller();

            var pp_request = new GetExpressCheckoutDetailsRequestType
            {
                Version = "51.0",
                Token = token
            };

            return (GetExpressCheckoutDetailsResponseType)caller.Call("GetExpressCheckoutDetails", pp_request);
        }

        protected DoExpressCheckoutPaymentResponseType ECDoExpressCheckoutCode(string token, string payerID, string paymentAmount,
            PaymentActionCodeType paymentAction, CurrencyCodeType currencyCodeType)
        {
            var caller = CreateCaller();

            var pp_request = new DoExpressCheckoutPaymentRequestType
            {
                Version = "51.0",
                DoExpressCheckoutPaymentRequestDetails = new DoExpressCheckoutPaymentRequestDetailsType
                {
                    Token = token,
                    PayerID = payerID,
                    PaymentAction = paymentAction,
                    PaymentDetails = new PaymentDetailsType
                    {
                        OrderTotal = new BasicAmountType
                        {
                            currencyID = currencyCodeType,
                            Value = paymentAmount
                        }
                    }
                }
            };

            return (DoExpressCheckoutPaymentResponseType)caller.Call("DoExpressCheckoutPayment", pp_request);
        }

        private DataSet GetTransaction(string token)
        {
            using (var conn = new SqlConnection(Settings.Default.PayPalDb))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * " +
                                  "FROM PayPalExpressCheckoutBegin " +
                                  "WHERE token = @token";
                var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                tokenParam.Value = token;

                conn.Open();

                using (var da = new SqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    da.Fill(ds);
                    return ds;
                }
            }
        }

        public void Cancel(HttpContext context)
        {
            string token = context.Request.Params["token"];
            var checkoutDetails = ECGetExpressCheckoutCode(token);

            if (checkoutDetails.Ack != AckCodeType.Success)
            {
                SavePayPalCancelError(checkoutDetails);
                throw new ApplicationException("Payment fetching failed. " + checkoutDetails.Errors[0].LongMessage);
            }

            SavePayPalCancel(checkoutDetails);
        }

        private void SavePayPalCancel(GetExpressCheckoutDetailsResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);
            var details = response.GetExpressCheckoutDetailsResponseDetails;

            Log.InfoFormat("Saving retrieved PayPal transaction: token={0}, response={1}", details.Token, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutCancel (Token, Response)" +
                                      "VALUES (@token, @response)";
                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = details.Token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }

        private void SavePayPalCancelError(GetExpressCheckoutDetailsResponseType response)
        {
            string responseStr = JSONHelper.Serialize(response);
            var details = response.GetExpressCheckoutDetailsResponseDetails;

            Log.InfoFormat("Saving cancel-with-error PayPal transaction: token={0}, response={1}", details.Token, responseStr);

            try
            {
                using (var conn = new SqlConnection(Settings.Default.PayPalDb))
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO PayPalExpressCheckoutCancelError (Token, Response)" +
                                      "VALUES (@token, @response)";
                    var tokenParam = cmd.Parameters.Add("@token", SqlDbType.VarChar);
                    tokenParam.Value = details.Token;

                    var responseParam = cmd.Parameters.Add("@response", SqlDbType.VarChar);
                    responseParam.Value = responseStr;

                    conn.Open();
                    int rows = cmd.ExecuteNonQuery();

                    if (rows != 1)
                        throw new ApplicationException("Inserting a transaction returned rows != 1: " + rows);
                }
            }
            catch (Exception e)
            {
                Log.Error("Saving transaction failed. ", e);
                // we don't rethrow the exception. We'll try to complete the transaction.
            }
        }
    }
}