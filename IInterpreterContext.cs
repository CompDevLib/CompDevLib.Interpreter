using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public interface IInterpreterContext<T> where T : IInterpreterContext<T>
    {
        public Evaluator Evaluator { get; }
        void OnInstructionEvaluated(ValueInfo ret);
        void OnExecuteInstruction(Instruction<T> instruction, ASTNode[] parameters);
        Instruction<T> GetExecutingInstruction();
    }
}