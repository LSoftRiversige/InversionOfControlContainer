using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests.ClassesForTest.Stackoverflow
{
    public class Store4 : IStore4
    {
        public Store4(IStore1 store1)
        {
            Store1 = store1 ?? throw new ArgumentNullException(nameof(store1));
        }

        public IStore1 Store1 { get; set; }
    }
}
