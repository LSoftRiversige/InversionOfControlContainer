using System;
using System.Collections.Generic;
using System.Text;

namespace InversionOfControl.Tests.ClassesForTest.Stackoverflow
{
    public class Store3 : IStore3
    {
        public Store3(IStore4 store4)
        {
            Store4 = store4 ?? throw new ArgumentNullException(nameof(store4));
        }

        public IStore4 Store4 { get; set; }
    }
}
