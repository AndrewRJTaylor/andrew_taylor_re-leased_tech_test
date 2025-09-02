using System;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class PaymentValidationService : IPaymentValidationService
    {
        public void ValidatePayment(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            if (payment.Amount < 0)
                throw new ArgumentException("Payment amount cannot be negative", nameof(payment));

            if (string.IsNullOrWhiteSpace(payment.Reference))
                throw new ArgumentException("Payment reference cannot be null or empty", nameof(payment));
        }

        public void ValidateInvoice(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            if (invoice.Amount < 0)
                throw new ArgumentException("Invoice amount cannot be negative", nameof(invoice));
        }
    }
}