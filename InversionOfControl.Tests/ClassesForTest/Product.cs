using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests.ClassesForTest
{
    public class Product : IProduct
    {
        public Product(IInvoice invoice)
        {
            Invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));
        }

        public IInvoice Invoice { get; }
    }
}
