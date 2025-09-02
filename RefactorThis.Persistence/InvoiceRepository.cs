using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
	public class InvoiceRepository : IInvoiceRepository
	{
		private readonly Dictionary<string, Invoice> _invoices = new Dictionary<string, Invoice>();

		public Invoice GetInvoice(string reference)
		{
			return _invoices.TryGetValue(reference, out var invoice) ? invoice : null;
		}

		public void SaveInvoice(Invoice invoice)
		{
			if (!string.IsNullOrEmpty(invoice.Reference))
			{
				_invoices[invoice.Reference] = invoice;
			}
		}

		public void Add(Invoice invoice)
		{
			if (!string.IsNullOrEmpty(invoice.Reference))
			{
				_invoices[invoice.Reference] = invoice;
			}
		}
	}
}