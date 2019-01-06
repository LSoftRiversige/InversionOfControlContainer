using System;

namespace InversionOfControlContainer
{
    public interface IContainer
    {
        ClassDescription Bind<TKey, TValue>();
        ClassDescription Bind(Type intf, Type cls);
        T Get<T>();
        Lazy<T> GetLazy<T>();
    }
}