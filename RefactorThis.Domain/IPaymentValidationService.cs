using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface IPaymentValidationService
    {
        void ValidatePayment(Payment payment);
        void ValidateInvoice(Invoice invoice);
    }
}