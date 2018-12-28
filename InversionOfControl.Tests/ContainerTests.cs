using InversionOfControl.Tests.ClassesForTest;
using InversionOfControlContainer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace InversionOfControl.Tests
{
    public class ContainerTests
    {
        [Fact]
        public void Get_ResolveClass_NewObject()
        {
            var container = new Container();

            var obj = container.Get<Foo>();

            Assert.Equal(typeof(Foo), obj.GetType());
        }

        [Fact]
        public void Bind_BindInterface_NewInstanceByInterface()
        {
            var container = new Container();

            container.Bind<IFoo, Foo>();
            IFoo foo = container.Get<IFoo>();

            Assert.Equal(typeof(Foo), foo.GetType());
        }

        [Fact]
        public void Get_TwoObjects_MustDifferent()
        {
            var container = new Container();

            container.Bind<IFoo, Foo>();

            IFoo foo1 = container.Get<IFoo>();
            IFoo foo2 = container.Get<IFoo>();

            Assert.NotEqual(foo1, foo2);
        }

        [Fact]
        public void Bind_BindInterfaceWithParams_NewInstanceByInterface()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();
            container.Bind<IFoo, Foo>();

            IBar bar = container.Get<IBar>();

            Assert.Equal(typeof(Bar), bar.GetType());
        }

        [Fact]
        public void Bind_StackOverflow_Throws()
        {
            var container = new Container();
            container.Bind<IProduct, Product>();
            container.Bind<IInvoice, Invoice>();

            Assert.Throws<InvalidOperationException>(()=> container.Get<IProduct>());
        }

        [Fact]
        public void Bind_StackOverflow_ThrowsMessage()
        {
            var container = new Container();
            container.Bind<IProduct, Product>();
            container.Bind<IInvoice, Invoice>();

            AssertThrowsCheckMessage(()=> container.Get<IProduct>(), 
                "Circular constructor parameter reference for class 'IProduct'");
        }

        [Fact]
        public void Bind_DuplicateRegister_Throws()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();

            Assert.Throws<InvalidOperationException>(() => container.Bind<IBar, Bar>());
        }

        [Fact]
        public void Bind_DuplicateRegister_ThrowsMessage()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();

            AssertThrowsCheckMessage(() => container.Bind<IBar, Bar>(),
                "Duplicate register of binding 'IBar' => 'Bar'");
        }

        [Fact]
        public void Bind_SameRefs_Throws()
        {
            var container = new Container();

            Assert.Throws<InvalidOperationException>(() => container.Bind<IBar, IBar>());
        }

        [Fact]
        public void Bind_FirstIsClass_Throws()
        {
            var container = new Container();

            Assert.Throws<InvalidOperationException>(() => container.Bind<Bar, IBar>());
        }

        [Fact]
        public void Bind_SameRefs_ThrowsMessage()
        {
            var container = new Container();
            AssertThrowsCheckMessage(()=> container.Bind<IBar, IBar>(), "'IBar' and 'IBar' types must be different");
        }

        [Fact]
        public void Bind_FirstIsClass_ThrowsMessage()
        {
            var container = new Container();
            AssertThrowsCheckMessage(() => container.Bind<Bar, IBar>(), "'Bar' must be an interface");
        }

        [Fact]
        public void WithConstructorArgument_CheckValues_NewInstance()
        {
            var container = new Container();
            container.Bind<IWarrior, Warrior>()
                .WithConstructorArgument("name", "NameOfWarrior")
                .WithConstructorArgument("power", 1300);

            IWarrior warrior = container.Get<IWarrior>();

            Assert.Equal(typeof(Warrior), warrior.GetType());
        }

        [Fact]
        public void WithConstructorArgument_ParamNotFound_Throws()
        {
            var container = new Container();
            container.Bind<IWarrior, Warrior>();

            Assert.Throws<InvalidOperationException>(() => container.WithConstructorArgument("name1", "NameOfWarrior"));
        }

        [Fact]
        public void WithConstructorArgument_ParamNotFound_ThrowsMessage()
        {
            var container = new Container();
            container.Bind<IWarrior, Warrior>();

            AssertThrowsCheckMessage(
                () => container.WithConstructorArgument(
                    "name1", "NameOfWarrior"
                    ), "No constructor was found with parameter 'name1' in the class 'Warrior'");
        }

        [Fact]
        public void WithConstructorArgument_ArgumentAlreadyRegistered_Throws()
        {
            var container = new Container();

            Assert.Throws<InvalidOperationException>(
                () => container.Bind<IWarrior, Warrior>()
                .WithConstructorArgument("name", "NameOfWarrior")
                .WithConstructorArgument("name", "NameSecond"));
        }

        private void AssertThrowsCheckMessage(Action action, string sampleMessage)
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

        private void AssertThrows<T>(Action action, string sampleMessage) where T: Exception 
        {
            string m = string.Empty;
            try
            {
                action();
            }
            catch (T e)
            {
                m = e.Message;
            }

            Assert.Equal(sampleMessage, m);
        }

        [Fact]
        public void WithConstructorArgument_ArgumentAlreadyRegistered_ThrowsMessage()
        {
            var container = new Container();
            AssertThrowsCheckMessage(() => container.Bind<IWarrior, Warrior>()
                .WithConstructorArgument("name", "NameOfWarrior")
                .WithConstructorArgument("name", "NameSecond"),
                "Parameter 'name' already registered");
        }

        [Fact]
        public void WithConstructorArgument_Exception_MustAfterBind()
        {
            var container = new Container();

            Assert.Throws<InvalidOperationException>(
                () => container.WithConstructorArgument("name", "NameOfWarrior"));

        }

        [Fact]
        public void WithPropertyValue_ResolveBar_New()
        {
            var container = new Container();
            const int c100 = 100;
            container.Bind<IFoo, Foo>()
                .WithPropertyValue("Counter", c100);

            var obj = container.Get<IFoo>();

            Assert.Equal(c100, obj.Counter);

        }

        [Fact]
        public void WithPropertyValue_Duplicate_Throw()
        {
            var container = new Container();
            const int c100 = 100;
            container.Bind<IFoo, Foo>()
                .WithPropertyValue("Counter", c100);

            Assert.Throws<InvalidOperationException>(()=>container.WithPropertyValue("Counter", 1));
        }

        [Fact]
        public void WithPropertyValue_Duplicate_ThrowMessage()
        {
            var container = new Container();
            const int c100 = 100;
            container.Bind<IFoo, Foo>()
                .WithPropertyValue("Counter", c100);

            AssertThrowsCheckMessage(() => container.WithPropertyValue("Counter", 1), 
                "Property 'Counter' already registered");
        }

        [Fact]
        public void WithPropertyValue_UnknownName_Throw()
        {
            var container = new Container();
            const int c100 = 100;
            container.Bind<IFoo, Foo>();

            Assert.Throws<InvalidOperationException>(() => container.WithPropertyValue("Counter1", c100));
        }

        [Fact]
        public void WithPropertyValue_UnknownName_ThrowMessage()
        {
            var container = new Container();
            const int c100 = 100;
            container.Bind<IFoo, Foo>();

            AssertThrowsCheckMessage(() => container.WithPropertyValue("Counter1", c100), 
                "Property 'Counter1' not found in class 'Foo'");
        }

        [Fact]
        public void GetSingleton_Resolve_MustSame()
        {
            var container = new Container();
            container.Bind<IFoo, Foo>();

            IFoo ston = container.GetSingltone<IFoo>();
            IFoo ston1 = container.GetSingltone<IFoo>();

            Assert.Equal(ston, ston1);
        }

        [Fact]
        public void GetSingleton_ResolveCtorParams_NewObject()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();
            container.Bind<IFoo, Foo>();

            IBar ston = container.GetSingltone<IBar>();

            Assert.Equal(typeof(Bar), ston.GetType());
        }
    }
}
