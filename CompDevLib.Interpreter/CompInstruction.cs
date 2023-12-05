using CompDevLib.Interpreter.Parse;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter
{
    public class CompInstruction<TContext> where TContext : ICompInterpreterContext
    {
        public delegate ValueInfo Function(TContext context, ASTNode[] parameters);

        private Function _function;
        private ASTNode[] _parameters;

        public CompInstruction(Function func, ASTNode[] parameters)
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