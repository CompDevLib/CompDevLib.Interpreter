using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class BasicContext : ICompInterpreterContext<BasicContext>
    {
        public CompEnvironment Environment => _compEnv;
        private CompEnvironment _compEnv;
        private CompInstruction<BasicContext> _currInstruction;

        public BasicContext()
        {
            _compEnv = new CompEnvironment();
        }

        public void OnExecuteInstruction(CompInstruction<BasicContext> instruction)
        {
            _currInstruction = instruction;
        }

        public CompInstruction<BasicContext> GetExecutingInstruction()
        {
            return _currInstruction;
        }
    }
}