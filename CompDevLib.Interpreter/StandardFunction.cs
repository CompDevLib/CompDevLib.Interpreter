using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class StandardFunction<TContext> : IFunction<TContext> where TContext : ICompInterpreterContext
    {
        public delegate ValueInfo Function(TContext context, ASTNode[] parameters);

        private readonly Function _func;

        public StandardFunction(Function func)
        {
            _func = func;
        }

        public ValueInfo Invoke(TContext context, ASTNode[] parameters)
        {
            return _func.Invoke(context, parameters);
        }
    }
}