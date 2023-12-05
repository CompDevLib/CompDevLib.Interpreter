using CompDevLib.Interpreter;
using CompDevLib.Interpreter.Parse;

namespace UnitTest;

public class Context : ICompInterpreterContext
{
    public CompEnvironment Environment => _compEnv;

    private CompEnvironment _compEnv;

    public Context()
    {
        _compEnv = new CompEnvironment();
    }
}