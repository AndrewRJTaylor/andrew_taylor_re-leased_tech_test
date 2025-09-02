using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public interface ITaxCalculationService
    {
        void ApplyTax(Invoice invoice, decimal paymentAmount);
    }
}