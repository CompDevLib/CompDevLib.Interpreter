using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class BasicContext : IInterpreterContext<BasicContext>
    {
        public Evaluator Evaluator { get; } = new();

        private Instruction<BasicContext> _currInstruction;

        public void OnExecuteInstruction(Instruction<BasicContext> instruction)
        {
            _currInstruction = instruction;
        }

        public void OnInstructionEvaluated(ValueInfo ret)
        {
        }

        public Instruction<BasicContext> GetExecutingInstruction()
        {
            return _currInstruction;
        }
    }
}