using System;

namespace InversionOfControlContainer
{
    public interface IContainer
    {
        IClassDescription Bind<TKey, TValue>();
        IClassDescription Bind(Type intf, Type cls);
        T Get<T>();
        Lazy<T> GetLazy<T>();
    }
}