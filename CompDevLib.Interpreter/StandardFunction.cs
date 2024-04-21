using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class StandardFunction<TContext> : IFunction<TContext> 
        where TContext : ICompInterpreterContext<TContext>
    {
        public delegate ValueInfo Function(TContext context, ASTNode[] parameters);

        public string Name { get; }

        private readonly Function _func;

        public StandardFunction(string name, Function func)
        {
            Name = name;
            _func = func;
        }

        public ValueInfo Invoke(TContext context, ASTNode[] parameters)
        {
            return _func.Invoke(context, parameters);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}