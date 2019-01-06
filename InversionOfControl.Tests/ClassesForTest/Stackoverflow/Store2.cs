using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests.ClassesForTest.Stackoverflow
{
    public class Store2 : IStore2
    {
        public Store2(IStore3 store3)
        {
            Store3 = store3 ?? throw new ArgumentNullException(nameof(store3));
        }

        public IStore3 Store3 { get; set; }
    }
}
