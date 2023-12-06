using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public interface IFunction<in TContext> where TContext : ICompInterpreterContext
    {
        public ValueInfo Invoke(TContext context, ASTNode[] parameters);
    }
}