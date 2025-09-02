using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;
using RefactorThis.Domain;

namespace RefactorThis.Domain.Tests
{
	[TestFixture]
	public class InvoicePaymentProcessorTests
	{
		private IInvoiceRepository _repository;
		private ITaxCalculationService _taxService;
		private IPaymentValidationService _validationService;
		private InvoiceService _paymentProcessor;

		[SetUp]
		public void Setup()
		{
			_repository = new InvoiceRepository();
			_taxService = new TaxCalculationService(new TaxConfiguration());
			_validationService = new PaymentValidationService();
			_paymentProcessor = new InvoiceService(_repository, _taxService, _validationService);
		}
		[Test]
		public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference( )
		{
			var payment = new Payment { Reference = "unknown" };
			var failureMessage = "";

			try
			{
				var result = _paymentProcessor.ProcessPayment( payment );
			}
			catch ( InvalidOperationException e )
			{
				failureMessage = e.Message;
			}

			Assert.AreEqual( "There is no invoice matching this payment", failureMessage );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 0,
				AmountPaid = 0,
				Payments = null
			};

			_repository.Add( invoice );

			var payment = new Payment { Reference = "test-ref" };

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "no payment needed", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 10,
				AmountPaid = 10,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 10
					}
				}
			};
			_repository.Add( invoice );

			var payment = new Payment { Reference = "test-ref" };

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			_repository.Add( invoice );

			var payment = new Payment
			{
				Reference = "test-ref",
				Amount = 6
			};

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the partial amount remaining", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 5,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			_repository.Add( invoice );

			var payment = new Payment
			{
				Reference = "test-ref",
				Amount = 6
			};

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "the payment is greater than the invoice amount", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			_repository.Add( invoice );

			var payment = new Payment
			{
				Reference = "test-ref",
				Amount = 5
			};

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "final partial payment received, invoice is now fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( ) { new Payment( ) { Amount = 10 } }
			};
			_repository.Add( invoice );

			var payment = new Payment
			{
				Reference = "test-ref",
				Amount = 10
			};

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice was already fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 10,
				AmountPaid = 5,
				Payments = new List<Payment>
				{
					new Payment
					{
						Amount = 5
					}
				}
			};
			_repository.Add( invoice );

			var payment = new Payment
			{
				Reference = "test-ref",
				Amount = 1
			};

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "another partial payment received, still not fully paid", result );
		}

		[Test]
		public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount( )
		{
			var invoice = new Invoice
			{
				Reference = "test-ref",
				Amount = 10,
				AmountPaid = 0,
				Payments = new List<Payment>( )
			};
			_repository.Add( invoice );

			var payment = new Payment
			{
				Reference = "test-ref",
				Amount = 1
			};

			var result = _paymentProcessor.ProcessPayment( payment );

			Assert.AreEqual( "invoice is now partially paid", result );
		}
	}
}