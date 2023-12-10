using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public interface IFunction<in TContext> where TContext : ICompInterpreterContext<TContext>
    {
        public ValueInfo Invoke(TContext context, ASTNode[] parameters);
    }
}