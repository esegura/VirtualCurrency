using System;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using System.Web.Services;
using CommonUtils;
using CustomerManagement;
using log4net;
using Payments;
using UserManagement;
using VirtualCurrencyWebSvc.Data;
using VirtualCurrencyWebSvc.Util;

namespace VirtualCurrencyWebSvc
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Payment : IHttpHandler
    {
        private enum Operation
        {
            Unknown,
            Buy,
            Confirm,
            Cancel
        }

        private static readonly log4net.ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void ProcessRequest(HttpContext context)
        {
            string result = null;
            SiteResponse response = null;
            Operation op = GetOperation(context);

            try
            {
                switch (op)
                {
                    case Operation.Buy:
                        response = Buy(context);
                        break;
                    case Operation.Confirm:
                        response = Confirm(context);
                        break;
                    case Operation.Cancel:
                        response = Cancel(context);
                        break;
                    default:
                        throw new ApplicationException("Operation invalid");
                }

                result = JSONHelper.Serialize<SiteResponse>(response);
            }
            catch (UserManagementServiceException umse)
            {
                ExceptionHelper.Code exceptionCode = ExceptionHelper.Code.UnexpectedException;
                string message = umse.Message;

                switch (umse.Code)
                {
                    case UserManagementServiceException.ErrorCode.UnexpectedError:
                        break;
                    case UserManagementServiceException.ErrorCode.ObjectNotFound:
                        exceptionCode = ExceptionHelper.Code.InvalidLogin;
                        break;
                    case UserManagementServiceException.ErrorCode.InvalidOperationOnResource:
                        exceptionCode = ExceptionHelper.Code.InvalidOperation;
                        break;
                    case UserManagementServiceException.ErrorCode.AccessDenied:
                        exceptionCode = ExceptionHelper.Code.AccessDenied;
                        break;
                    case UserManagementServiceException.ErrorCode.CouldNotConnectToDatabase:
                        message = "Could not connect to the database. " + message;
                        break;
                    default:
                        message = "Unknown ErrorCode: " + umse.Code + ". Message: " + message;
                        break;
                }

                result = JSONHelper.Serialize(ExceptionHelper.Handle(umse, exceptionCode, message, log));
            }
            catch (Exception e)
            {
                result = JSONHelper.Serialize(ExceptionHelper.Handle(e, log));
            }
            finally
            {
                context.Response.ContentType = MediaTypeNames.Text.Plain;
                context.Response.Write(result);
            }
        }

        private Operation GetOperation(HttpContext context)
        {
            string op = context.Param(SiteParameters.OPERATION);

            if (Enum.IsDefined(typeof(Operation), op))
                return (Operation)Enum.Parse(typeof(Operation), op);

            return Operation.Unknown;
        }

        private SiteResponse Buy(HttpContext context)
        {
            string loginToken = context.Param(SiteParameters.LOGIN_TOKEN);
            var paymentType = context.Param<PaymentProcessorType>(SiteParameters.PAYMENT_TYPE);
            string productId = context.Param(SiteParameters.PRODUCT_ID);
            var lt = JSONHelper.Deserialize<LoginToken>(loginToken);

            VerifySession(lt);

            var processor = PaymentProcessorFactory.Create(paymentType);
            string customerId = GetCustomerId(lt).ToString();
            decimal paymentAmount = GetPaymentAmount(productId);

            var uri = processor.Buy(customerId, productId, paymentAmount);

            var response = new SiteResponse()
            {
                response = uri,
                status = SiteResponse.Status.Success,
                syncKey = "aSyncKey"
            };

            return response;
        }

        private SiteResponse Confirm(HttpContext context)
        {
            var paymentType = context.Param<PaymentProcessorType>(SiteParameters.PAYMENT_TYPE);
            var processor = PaymentProcessorFactory.Create(paymentType);
            
            var transaction = processor.Confirm(context);
            var balance = CreditCustomerAccount(transaction);

            return new SiteResponse
            {
                response = "New balance: " + balance,
                status = SiteResponse.Status.Success,
                syncKey = "aSyncKey"
            };
        }

        private decimal CreditCustomerAccount(IPayment transaction)
        {
            log.InfoFormat("Crediting Customer account: customerId={0}, prodId={1}, amount={2}",
                transaction.CustomerId, transaction.ProdId, transaction.Amount);

            int customerId = Convert.ToInt32(transaction.CustomerId);
            decimal amount = Convert.ToDecimal(transaction.Amount);
            CustomerManagementService.ModifyBalance(customerId, amount);
            return CustomerManagementService.FindCustomerWithId(customerId).Balance;
        }

        private SiteResponse Cancel(HttpContext context)
        {
            var paymentType = context.Param<PaymentProcessorType>(SiteParameters.PAYMENT_TYPE);
            var processor = PaymentProcessorFactory.Create(paymentType);
            processor.Cancel(context);
            return new SiteResponse
            {
                response = "Success",
                status = SiteResponse.Status.Success,
                syncKey = "aSyncKey"
            };
        }

        private void VerifySession(LoginToken lt)
        {
            bool isValid = UserManagementService.ValidateLoginToken(lt);

            if (!isValid)
                throw new ApplicationException("Invalid session");
        }

        private int GetCustomerId(LoginToken lt)
        {
            var user = UserManagementService.FindUserWithLoginToken(lt);
            var customer = CustomerManagementService.FindCustomerWithUserId(user.Id);
            return customer.Id;
        }

        private decimal GetPaymentAmount(string productIdStr)
        {
            int productId = Convert.ToInt32(productIdStr);
            var itemPricings = CustomerManagementService.FindItemPricingsWithItemId(productId);

            // todo: there may be several item pricings for the given product. Decide here
            // which one corresponds to this order;
            foreach (var item in itemPricings)
            {
                return item.UnitPrice; // for now we just return the first one
            }

            throw new ArgumentOutOfRangeException("productIdStr: " + productIdStr);
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}
