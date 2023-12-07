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

        public void Optimize(TContext context)
        {
            if(_parameters == null) return;
            for (int i = 0; i < _parameters.Length; i++)
                _parameters[i] = _parameters[i].Optimize(context.Environment);
        }
    }
}