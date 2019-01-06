using InversionOfControl.Tests.ClassesForTest;
using InversionOfControl.Tests.ClassesForTest.Stackoverflow;
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
        public void Get_BindInterfaceWithParams_NewInstanceByInterface()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();
            container.Bind<IFoo, Foo>();

            IBar bar = container.Get<IBar>();

            Assert.Equal(typeof(Bar), bar.GetType());
        }

        [Fact]
        public void Get_InterfaceAndValueParams_CheckValue()
        {
            var container = new Container();
            container.Bind<IBaz, Baz>()
                .WithConstructorArgument("value", 500);
            container.Bind<IFoo, Foo>();

            IBaz baz = container.Get<IBaz>();

            Assert.Equal(500, baz.Value);
        }

        [Fact]
        public void Bind_StackOverflow_Throws()
        {
            var container = new Container();
            container.Bind<IProduct, Product>();

            StackOverflowException e = Assert.Throws<StackOverflowException>(() => container.Bind<IInvoice, Invoice>());
            Assert.Equal("Circular constructor parameter reference for class 'Invoice'", e.Message);
        }

        [Fact]
        public void Bind_StackOverflowCheckChainStoreX_Throws()
        {
            var container = new Container();
            container.Bind<IStore1, Store1>();
            container.Bind<IStore2, Store2>();
            container.Bind<IStore3, Store3>();

            Exception e = Assert.Throws<StackOverflowException>(() => container.Bind<IStore4, Store4>());
            Assert.Equal("Circular constructor parameter reference for class 'Store4'", e.Message);
        }

        [Fact]
        public void Bind_DuplicateRegister_Throws()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();

            Exception e = Assert.Throws<InvalidOperationException>(() => container.Bind<IBar, Bar>());
            Assert.Equal("Duplicate register of binding 'IBar' => 'Bar'", e.Message);
        }

        [Fact]
        public void Bind_SameRefs_Throws()
        {
            var container = new Container();

            Exception e = Assert.Throws<InvalidOperationException>(() => container.Bind<Bar, Bar>());
            Assert.Equal("'Bar' and 'Bar' types must be different", e.Message);
        }

        [Fact]
        public void Bind_SecondParamCannotBeAnInterface_Throws()
        {
            var container = new Container();

            Exception e = Assert.Throws<InvalidOperationException>(() => container.Bind<Bar, IBar>());
            Assert.Equal("'IBar' cannot be an interface", e.Message);
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


            Exception e = Assert.Throws<InvalidOperationException>(() =>
                container.Bind<IWarrior, Warrior>()
                .WithConstructorArgument("name1", "NameOfWarrior"));
            Assert.Equal("No constructor was found with parameter 'name1' in the class 'Warrior'", e.Message);
        }

        [Fact]
        public void WithConstructorArgument_ArgumentAlreadyRegistered_Throws()
        {
            var container = new Container();

            InvalidOperationException e = Assert.Throws<InvalidOperationException>(
                () => container.Bind<IWarrior, Warrior>()
                .WithConstructorArgument("name", "NameOfWarrior")
                .WithConstructorArgument("name", "NameSecond"));
            Assert.Equal("Parameter 'name' already registered", e.Message);
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

            InvalidOperationException e = Assert.Throws<InvalidOperationException>(() =>
                container.Bind<IFoo, Foo>()
                    .WithPropertyValue("Counter", c100)
                    .WithPropertyValue("Counter", 1));
            Assert.Equal("Property 'Counter' already registered", e.Message);
        }

        [Fact]
        public void WithPropertyValue_UnknownName_Throw()
        {
            var container = new Container();
            const int c100 = 100;

            InvalidOperationException e = Assert.Throws<InvalidOperationException>(() =>
                container.Bind<IFoo, Foo>()
                .WithPropertyValue("Counter1", c100));
            Assert.Equal("Property 'Counter1' not found in class 'Foo'", e.Message);
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

        [Fact]
        public void GetLazy_LazyResolve_NewObject()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();
            container.Bind<IFoo, Foo>();

            Lazy<IBar> lazyBar = container.GetLazy<IBar>();

            Assert.Equal(typeof(Bar), lazyBar.Value.GetType());
        }

        [Fact]
        public void GetLazy_LazyResolveClass_NewObject()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();
            container.Bind<IFoo, Foo>();

            Lazy<Bar> lazyBar = container.GetLazy<Bar>();

            Assert.Equal(typeof(Bar), lazyBar.Value.GetType());
        }
    }
}