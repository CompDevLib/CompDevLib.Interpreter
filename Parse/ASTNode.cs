using System;
using System.Globalization;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter.Parse
{
    public abstract class ASTNode
    {
        public abstract ValueInfo Evaluate(Evaluator evaluator);

        public virtual bool IsConstValue()
        {
            return false;
        }

        public virtual ASTNode Optimize(Evaluator evaluator)
        {
            return this;
        }

        public object GetAnyValue(Evaluator evaluator)
        {
            var valueInfo = Evaluate(evaluator);
            return evaluator.PopTopValue(valueInfo);
        }
        
        public object GetAnyValue(Evaluator evaluator, Type typeHint)
        {
            var valueInfo = Evaluate(evaluator);
            var convertedInfo = evaluator.ConvertValue(valueInfo, typeHint);
            var value = evaluator.PopTopValue(convertedInfo);
            return evaluator.ConvertValue(value, typeHint);
        }

        public string GetAnyValueAsString(Evaluator evaluator)
        {
            var valueInfo = Evaluate(evaluator);
            return evaluator.PopTopValueAsString(valueInfo);
        }

        public int GetIntValue(Evaluator evaluator)
        {
            var valueInfo = Evaluate(evaluator);
            if (valueInfo.ValueType != EValueType.Int)
                throw new Exception($"Invalid return type: {EValueType.Int} expected, {valueInfo.ValueType} given.");
            return evaluator.EvaluationStack.PopUnmanaged<int>();
        }
        
        public bool GetBoolValue(Evaluator evaluator)
        {
            var valueInfo = Evaluate(evaluator);
            if (valueInfo.ValueType != EValueType.Bool)
                throw new Exception($"Invalid return type: {EValueType.Bool} expected, {valueInfo.ValueType} given.");
            return evaluator.EvaluationStack.PopUnmanaged<bool>();
        }
        public float GetFloatValue(Evaluator evaluator)
        {
            var valueInfo = Evaluate(evaluator);
            if (valueInfo.ValueType != EValueType.Float)
                throw new Exception($"Invalid return type: {EValueType.Float} expected, {valueInfo.ValueType} given.");
            return evaluator.EvaluationStack.PopUnmanaged<float>();
        }

        public string GetStringValue(Evaluator evaluator)
        {
            var valueInfo = Evaluate(evaluator);
            if (valueInfo.ValueType != EValueType.Str)
                throw new Exception($"Invalid return type: {EValueType.Str} expected, {valueInfo.ValueType} given.");
            return evaluator.EvaluationStack.PopObject<string>();
        }
        
        public T GetObjectValue<T>(Evaluator evaluator) where T : class
        {
            var valueInfo = Evaluate(evaluator);
            if (valueInfo.ValueType != EValueType.Obj)
                throw new Exception($"Invalid return type: {EValueType.Obj} expected, {valueInfo.ValueType} given.");
            return evaluator.EvaluationStack.PopObject<T>();
        }
    }
}