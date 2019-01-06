using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests.ClassesForTest.Stackoverflow
{
    public class Store1 : IStore1
    {
        public Store1(IStore4 store4)
        {
            Store4 = store4 ?? throw new ArgumentNullException(nameof(store4));
        }

        public IStore4 Store4 { get; set; }
    }
}
