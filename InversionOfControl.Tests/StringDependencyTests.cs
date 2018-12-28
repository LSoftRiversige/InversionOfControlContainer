using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using InversionOfControl.Tests.ClassesForTest;
using InversionOfControlContainer;
using System.Reflection;

namespace InversionOfControl.Tests
{
    public class StringDependencyTests
    {
        [Fact]
        public void Parse_CheckInterface_Type()
        {
            var s = new StringDependencyParser(Assembly.GetExecutingAssembly());

            (Type, Type) pare = s.Parse("IBar -> Bar;");

            Assert.Equal(typeof(IBar), pare.Item1);
        }

        [Fact]
        public void Parse_CheckClass_Type()
        {
            var s = new StringDependencyParser(Assembly.GetExecutingAssembly());

            (Type, Type) pare = s.Parse("IBar -> Bar;");

            Assert.Equal(typeof(Bar), pare.Item2);
        }

        [Fact]
        public void Parse_Delimiter_ThrowsMessage()
        {
            var s = new StringDependencyParser(Assembly.GetExecutingAssembly());

            TestUtils.AssertThrowsCheckMessage(()=> s.Parse("IBar - Bar;"), 
                "Pattern '->' not found in line 'IBar - Bar;'");
        }

        [Fact]
        public void Parse_EndDelim_ThrowsMessage()
        {
            var s = new StringDependencyParser(Assembly.GetExecutingAssembly());

            TestUtils.AssertThrowsCheckMessage(() => s.Parse("IBar -> Bar"),
                "Pattern ';' not found in line 'IBar -> Bar'");
        }

        [Fact]
        public void Bind_ParseLineAddBind_NewObject()
        {
            var s = new StringDependencyParser(Assembly.GetExecutingAssembly());
            Container container = new Container();
            s.BindTo(container, "IBar -> Bar;");
            s.BindTo(container, "IFoo -> Foo;");
            IBar bar = container.Get<IBar>();
            Assert.IsAssignableFrom<IBar>(bar);
        }

        [Fact]
        public void Bind_ParseComment_Null()
        {
            var s = new StringDependencyParser(Assembly.GetExecutingAssembly());
            Container container = new Container();
            s.BindTo(container, "//IBar -> Bar;");
            s.BindTo(container, "//IFoo -> Foo;");
            
            Assert.Empty(container.Bindings);
        }

        [Fact]
        public void Bind_Text_NewObject()
        {
            var s = new StringDependencyParser(Assembly.GetExecutingAssembly());
            Container container = new Container();
            s.BindTextTo(container,
                new string[] {$"IBar -> Bar;", "IFoo -> Foo;"});

            IBar bar = container.Get<IBar>();

            Assert.IsAssignableFrom<IBar>(bar);
        }
    }
}
