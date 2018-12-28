namespace InversionOfControlContainer
{
    public interface IStringDependencyParser
    {
        void BindTextTo(IContainer container, string[] text);
    }
}