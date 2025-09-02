using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ITaxCalculationService _taxCalculationService;
        private readonly IPaymentValidationService _paymentValidationService;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            ITaxCalculationService taxCalculationService,
            IPaymentValidationService paymentValidationService)
        {
            _invoiceRepository = invoiceRepository;
            _taxCalculationService = taxCalculationService;
            _paymentValidationService = paymentValidationService;
        }

        public string ProcessPayment(Payment payment)
        {
            _paymentValidationService.ValidatePayment(payment);
            
            var invoice = GetInvoiceOrThrow(payment.Reference);
            _paymentValidationService.ValidateInvoice(invoice);

            var result = ProcessPaymentForInvoice(invoice, payment);

            _invoiceRepository.SaveInvoice(invoice);
            return result;
        }

        private Invoice GetInvoiceOrThrow(string paymentReference)
        {
            var invoice = _invoiceRepository.GetInvoice(paymentReference);
            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }
            return invoice;
        }

        private string ProcessPaymentForInvoice(Invoice invoice, Payment payment)
        {
            if (invoice.Amount == 0)
            {
                return HandleZeroAmountInvoice(invoice);
            }

            if (IsInvoiceAlreadyFullyPaid(invoice))
            {
                return "invoice was already fully paid";
            }

            if (IsPaymentExcessive(invoice, payment))
            {
                return GetExcessivePaymentMessage(invoice);
            }

            return ApplyPaymentToInvoice(invoice, payment);
        }

        private string HandleZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
            {
                return "no payment needed";
            }
            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private bool IsInvoiceAlreadyFullyPaid(Invoice invoice)
        {
            return invoice.Payments != null &&
                   invoice.Payments.Any() &&
                   invoice.Payments.Sum(x => x.Amount) != 0 &&
                   invoice.Amount == invoice.Payments.Sum(x => x.Amount);
        }

        private bool IsPaymentExcessive(Invoice invoice, Payment payment)
        {
            if (HasExistingPayments(invoice))
            {
                return payment.Amount > GetRemainingAmount(invoice);
            }
            return payment.Amount > invoice.Amount;
        }

        private string GetExcessivePaymentMessage(Invoice invoice)
        {
            return HasExistingPayments(invoice)
                ? "the payment is greater than the partial amount remaining"
                : "the payment is greater than the invoice amount";
        }

        private string ApplyPaymentToInvoice(Invoice invoice, Payment payment)
        {
            if (invoice.Payments == null)
            {
                invoice.Payments = new List<Payment>();
            }

            invoice.Payments.Add(payment);

            if (HasExistingPayments(invoice) && invoice.AmountPaid > 0)
            {
                return ProcessSubsequentPayment(invoice, payment);
            }
            return ProcessInitialPayment(invoice, payment);
        }

        private string ProcessInitialPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid = payment.Amount;
            _taxCalculationService.ApplyTax(invoice, payment.Amount);

            return payment.Amount == invoice.Amount
                ? "invoice is now fully paid"
                : "invoice is now partially paid";
        }

        private string ProcessSubsequentPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            _taxCalculationService.ApplyTax(invoice, payment.Amount);

            return GetRemainingAmount(invoice) == 0
                ? "final partial payment received, invoice is now fully paid"
                : "another partial payment received, still not fully paid";
        }

        private bool HasExistingPayments(Invoice invoice)
        {
            return invoice.Payments != null && invoice.Payments.Any();
        }

        private decimal GetRemainingAmount(Invoice invoice)
        {
            return invoice.Amount - invoice.AmountPaid;
        }
    }
}