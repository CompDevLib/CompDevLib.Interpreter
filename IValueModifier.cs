namespace CompDevLib.Interpreter
{
    public interface IValueModifier<in TContext> where TContext : IInterpreterContext<TContext>
    {
        ValueInfo ModifyValue(TContext context, ValueInfo srcValueInfo);
    }
}