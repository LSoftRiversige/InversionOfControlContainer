using System;

namespace InversionOfControlContainer
{
    public interface IContainer
    {
        IContainer Bind<TKey, TValue>();
        T Get<T>();
        Lazy<T> GetLazy<T>();
        IContainer WithConstructorArgument(string paramName, object paramValue);
        IContainer WithPropertyValue(string propertyName, object propertyValue);

    }
}