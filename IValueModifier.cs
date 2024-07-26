namespace CompDevLib.Interpreter
{
    public interface IValueModifier<in TContext> where TContext : ICompInterpreterContext<TContext>
    {
        ValueInfo ModifyValue(TContext context, ValueInfo srcValueInfo);
    }
}