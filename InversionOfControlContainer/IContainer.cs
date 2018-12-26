namespace InversionOfControlContainer
{
    public interface IContainer
    {
        IContainer Bind<TKey, TValue>();
        T Get<T>();
        IContainer WithConstructorArgument(string paramName, object paramValue);
    }
}