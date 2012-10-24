
namespace Payments
{
    public interface IPayment
    {
        string CustomerId { get; }
        string ProdId { get; }
        string Amount { get; }
    }
}
