using System;
using System.Reflection;
using System.Xml;
using com.paypal.soap.api;
using log4net;
using log4net.Config;
using NUnit.Framework;
using System.Data;
using System.Data.SqlClient;
using Payments.Properties;
using System.Data.Common;
using CommonUtils;

namespace Payments.Tests
{
    [TestFixture]
    class PayPalProcessorTests
    {
        static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [TestFixtureSetUp]
        public void ClassSetUp()
        {
            XmlConfigurator.Configure();
            Log.Debug("Logging initialized");
        }

        [TearDown]
        public void TearDown()
        {
            using (var conn = new SqlConnection(Settings.Default.PayPalDb))
            using (var cmd = new SqlCommand("DELETE FROM PayPalBeginExpressCheckout", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        [Test]
        public void SavePayPalTransactionStartTest()
        {
            string customerId = "someCustomerId";
            string prodId = "someProdId";
            var response = new SetExpressCheckoutResponseType()
            {
                Ack = AckCodeType.CustomCode,
                Any = new XmlDocument().CreateElement("someXmlElement"),
                Build = "someBuild",
                CorrelationID = "someCorrelationId",
                Errors = new []{ new ErrorType
                    { 
                        ErrorCode = "someErrorCode",
                        ErrorParameters = new []{ new ErrorParameterType
                            {
                                ParamID = "someParamId",
                                Value = "someValue"
                            }},
                        LongMessage = "someLongMsg",
                        SeverityCode = SeverityCodeType.CustomCode,
                        ShortMessage = "someShortMsg"
                    }},
                Timestamp = new DateTime(),
                TimestampSpecified = true,
                Token = "someToken",
                Version = "someVersion"
            };



            var ppProcessor = new PayPalProcessorAccessor();
            ppProcessor.SavePayPalTransactionStart(customerId, prodId, response);

            //verify saved
            var ds = GetTable("PayPalBeginExpressCheckout");
            Assert.IsTrue(isObjectSaved(customerId, prodId, response, ds));
        }

        [Test]
        public void ECSetExpressCheckoutCodeTest()
        {
            string paymentAmount = new Decimal(1.0).ToString();
            string returnUrl = "http://vergencemedia.com/";
            string cancelUrl = "http://vergencemedia.com/";
            PaymentActionCodeType paymentAction = PaymentActionCodeType.Sale;
            CurrencyCodeType currencyCode = CurrencyCodeType.USD;
            var ppProcessor = new PayPalProcessorAccessor();

            var result = ppProcessor.ECSetExpressCheckoutCode(paymentAmount, returnUrl, cancelUrl, paymentAction, currencyCode);

            Assert.AreEqual(AckCodeType.Success, result.Ack, "The ack was {0}. Error: {1}", result.Ack,
                (result.Errors != null && result.Errors.Length > 0) ? result.Errors[0].ErrorCode + " " + result.Errors[0].LongMessage : "");
            Assert.IsNotNullOrEmpty(result.Token);
        }

        private bool isObjectSaved(string customerId, string prodId, SetExpressCheckoutResponseType response, DataSet ds)
        {
            foreach (DataTable table in ds.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    if ((string)row["CustomerId"] == customerId
                        && (string)row["ProdId"] == prodId
                        && (string)row["Response"] == JSONHelper.Serialize(response))
                        return true;
                }
            }

            return false;
        }

        private DataSet GetTable(string table)
        {
            using (var da = new SqlDataAdapter("SELECT * FROM " + table, Settings.Default.PayPalDb))
            {
                var ds = new DataSet();
                da.Fill(ds);
                return ds;
            }
        }
    }
}
