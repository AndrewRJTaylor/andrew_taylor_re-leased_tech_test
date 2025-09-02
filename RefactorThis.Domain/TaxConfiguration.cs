namespace RefactorThis.Domain
{
    public class TaxConfiguration : ITaxConfiguration
    {
        private const decimal DefaultTaxRate = 0.14m;

        public decimal GetTaxRate()
        {
            return DefaultTaxRate;
        }
    }
}