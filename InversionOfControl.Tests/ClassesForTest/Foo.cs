using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests
{
    public class Foo : IFoo
    {
        public int Counter { get; set; }
        public decimal Price { get; set; }
        public string Model { get; set; }
    }
}
