namespace CompDevLib.Interpreter
{
    public interface IValueSelector
    {
        public delegate ValueInfo SelectValueFunc(CompEnvironment context, string value);
        ValueInfo SelectValue(CompEnvironment compEnvironment, string identifier);
    }
    
    public class ValueSelector : IValueSelector
    {
        private readonly IValueSelector.SelectValueFunc _func;

        public ValueSelector(IValueSelector.SelectValueFunc func)
        {
            _func = func;
        }

        public ValueInfo SelectValue(CompEnvironment compEnvironment, string identifier)
        {
            return _func?.Invoke(compEnvironment, identifier) ?? ValueInfo.Void;
        }

        public bool EqualsToFunc(IValueSelector.SelectValueFunc func) => func == _func;
    }
}