using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace InversionOfControl.Tests
{
    public static class TestUtils
    {
        public static void AssertThrowsCheckMessage(Action action, string sampleMessage)
        {
            string m = string.Empty;
            try
            {
                action();
            }
            catch (Exception e)
            {
                m = e.Message;
            }

            Assert.Equal(sampleMessage, m);
        }
    }
}
