using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests.ClassesForTest
{
    public class Invoice : IInvoice
    {
        public Invoice(IProduct product)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
        }

        public IProduct Product { get; }
    }
}
