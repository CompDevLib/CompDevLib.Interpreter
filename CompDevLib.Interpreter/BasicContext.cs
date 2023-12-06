namespace CompDevLib.Interpreter
{
    public class BasicContext : ICompInterpreterContext
    {
        public CompEnvironment Environment => _compEnv;

        private CompEnvironment _compEnv;

        public BasicContext()
        {
            _compEnv = new CompEnvironment();
        }
    }
}