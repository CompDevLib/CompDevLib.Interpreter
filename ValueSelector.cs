namespace CompDevLib.Interpreter
{
    public interface IValueSelector
    {
        public delegate ValueInfo SelectValueFunc(Evaluator context, string value);
        ValueInfo SelectValue(Evaluator evaluator, string identifier);
    }
    
    public class ValueSelector : IValueSelector
    {
        private readonly IValueSelector.SelectValueFunc _func;

        public ValueSelector(IValueSelector.SelectValueFunc func)
        {
            _func = func;
        }

        public ValueInfo SelectValue(Evaluator evaluator, string identifier)
        {
            return _func?.Invoke(evaluator, identifier) ?? ValueInfo.Void;
        }

        public bool EqualsToFunc(IValueSelector.SelectValueFunc func) => func == _func;
    }
}