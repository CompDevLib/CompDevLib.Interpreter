namespace CompDevLib.Interpreter
{
    public interface IFieldValueSelector
    {
        ValueInfo SelectValue(object obj, Evaluator evaluator, string identifier);
    }

    public interface IFieldValueSelector<in T> : IFieldValueSelector
    {
        ValueInfo SelectValue(T obj, Evaluator evaluator, string identifier);
        
        ValueInfo IFieldValueSelector.SelectValue(object obj, Evaluator context, string value)
        {
            return SelectValue((T)obj, context, value);
        }
    }

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