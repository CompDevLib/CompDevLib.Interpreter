namespace CompDevLib.Interpreter
{
    public interface ICompInterpreterContext<T> where T : ICompInterpreterContext<T>
    {
        public CompEnvironment Environment { get; }
        void OnExecuteInstruction(CompInstruction<T> instruction);
        CompInstruction<T> GetExecutingInstruction();
    }
}