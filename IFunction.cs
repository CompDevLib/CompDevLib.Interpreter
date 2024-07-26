using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public interface IFunction<in TContext> where TContext : ICompInterpreterContext<TContext>
    {
        string Name { get; }
        ValueInfo Invoke(TContext context, ASTNode[] parameters);
    }
}