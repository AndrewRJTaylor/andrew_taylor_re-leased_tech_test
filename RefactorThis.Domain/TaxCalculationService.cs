using System;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class TaxCalculationService : ITaxCalculationService
    {
        private readonly ITaxConfiguration _taxConfiguration;

        public TaxCalculationService(ITaxConfiguration taxConfiguration)
        {
            _taxConfiguration = taxConfiguration;
        }

        public void ApplyTax(Invoice invoice, decimal paymentAmount)
        {
            var taxRate = _taxConfiguration.GetTaxRate();
            
            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    invoice.TaxAmount = invoice.AmountPaid * taxRate;
                    break;
                case InvoiceType.Commercial:
                    invoice.TaxAmount += paymentAmount * taxRate;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}