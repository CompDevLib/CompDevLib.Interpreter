using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public interface IFunction<in TContext> where TContext : IInterpreterContext<TContext>
    {
        string Name { get; }
        ValueInfo Invoke(TContext context, ASTNode[] parameters);
    }
}