using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests
{
    public class Bar : IBar
    {
        public Bar(IFoo foo)
        {
            Foo = foo;
        }

        public IFoo Foo { get; }
    }
}
