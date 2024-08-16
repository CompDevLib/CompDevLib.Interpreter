using System;

namespace CompDevLib.Interpreter
{
    public interface IStackTopValueConverter
    {
        ValueInfo Convert(Evaluator evaluator, ValueInfo valueInfo);
    }
    
    public interface IValueConverter
    {
        object Convert(object srcValue);
    }

    public class StackTopValueConverter : IStackTopValueConverter
    {
        public delegate ValueInfo Conversion(Evaluator evaluator, ValueInfo srcValueInfo);
        private readonly Conversion _convert;

        public StackTopValueConverter(Conversion convert)
        {
            _convert = convert;
        }

        public ValueInfo Convert(Evaluator evaluator, ValueInfo valueInfo) =>
            _convert?.Invoke(evaluator, valueInfo) ?? ValueInfo.Void;
    }

    public class ValueConverter : IValueConverter
    {
        public delegate object Conversion(object srcValue);
        private readonly Conversion _convert;
        
        public ValueConverter(Conversion convert)
        {
            _convert = convert;
        }

        public object Convert(object srcValue) => _convert?.Invoke(srcValue);
    }
}