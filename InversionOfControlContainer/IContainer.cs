using System;

namespace InversionOfControlContainer
{
    public interface IContainer
    {
        IContainer Bind<TKey, TValue>();
        IContainer Bind(Type intf, Type cls);
        T Get<T>();
        Lazy<T> GetLazy<T>();
        IContainer WithConstructorArgument(string paramName, object paramValue);
        IContainer WithPropertyValue(string propertyName, object propertyValue);

    }
}