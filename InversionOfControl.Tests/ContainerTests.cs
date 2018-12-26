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
        public void Bind_BindInterfaceWithParams_NewInstanceByInterface()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();
            container.Bind<IFoo, Foo>();

            IBar bar = container.Get<IBar>();

            Assert.Equal(typeof(Bar), bar.GetType());
        }

        [Fact]
        public void Bind_StackOverflow_Exception()
        {
            var container = new Container();
            container.Bind<IProduct, Product>();
            container.Bind<IInvoice, Invoice>();

            Assert.Throws<InvalidOperationException>(()=> container.Get<IProduct>());
        }

        [Fact]
        public void Bind_Exception_DuplicateRegister()
        {
            var container = new Container();
            container.Bind<IBar, Bar>();

            Assert.Throws<InvalidOperationException>(() => container.Bind<IBar, IBar>());
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
        public void WithConstructorArgument_Exception_ArgumentNotFound()
        {
            var container = new Container();
            container.Bind<IWarrior, Warrior>()
                .WithConstructorArgument("name1", "NameOfWarrior")
                .WithConstructorArgument("power", 1300);

            Assert.Throws<KeyNotFoundException>(() => container.Get<IWarrior>());
        }

        [Fact]
        public void WithConstructorArgument_Exception_ArgumentAlreadyRegistered()
        {
            var container = new Container();

            Assert.Throws<InvalidOperationException>(
                () => container.Bind<IWarrior, Warrior>()
                .WithConstructorArgument("name", "NameOfWarrior")
                .WithConstructorArgument("name", "NameSecond"));
        }

        [Fact]
        public void WithConstructorArgument_Exception_MustAfterBind()
        {
            var container = new Container();

            Assert.Throws<InvalidOperationException>(
                () => container.WithConstructorArgument("name", "NameOfWarrior"));

        }
    }
}
