using CompDevLib.Interpreter.Parse;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter
{
    public class CompInstruction<TContext> where TContext : ICompInterpreterContext
    {
        private readonly IFunction<TContext> _function;
        private readonly ASTNode[] _parameters;

        public CompInstruction(IFunction<TContext> func, ASTNode[] parameters)
        {
            _function = func;
            _parameters = parameters;
        }

        public ValueInfo Execute(TContext context)
        {
            return _function.Invoke(context, _parameters);
        }
    }
}