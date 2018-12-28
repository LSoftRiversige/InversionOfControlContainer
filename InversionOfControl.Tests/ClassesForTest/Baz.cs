using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests.ClassesForTest
{
    public class Baz : IBaz
    {
        readonly IFoo foo;

        public Baz(IFoo foo, int value)
        {
            this.foo = foo ?? throw new ArgumentNullException(nameof(foo));
            Value = value;
        }

        public int Value { get; set; }
        public string Message { get; set; }
    }
}
