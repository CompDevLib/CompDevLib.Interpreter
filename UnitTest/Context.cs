using CompDevLib.Interpreter;
using CompDevLib.Interpreter.Parse;

namespace UnitTest;

public class Context : ICompInterpreterContext
{
    public ASTContext ASTContext => _astContext;

    private ASTContext _astContext;

    public Context()
    {
        _astContext = new ASTContext();
    }
}