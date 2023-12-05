using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public interface ICompInterpreterContext
    {
        public ASTContext ASTContext { get; }
    }
}