using System;
using System.Globalization;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter.Parse
{
    public abstract class ASTNode
    {
        public abstract ValueInfo Evaluate(CompEnvironment context);

        public virtual bool IsConstValue()
        {
            return false;
        }

        public virtual ASTNode Optimize(CompEnvironment context)
        {
            return this;
        }

        public object GetAnyValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            return valueInfo.ValueType switch
            {
                EValueType.Int => context.EvaluationStack.PopUnmanaged<int>(),
                EValueType.Float => context.EvaluationStack.PopUnmanaged<float>(),
                EValueType.Bool => context.EvaluationStack.PopUnmanaged<bool>(),
                EValueType.Str => context.EvaluationStack.PopObject<string>(),
                EValueType.Obj => context.EvaluationStack.PopObject<object>(),
                _ => null
            };
        }
        
        public object GetAnyValue(CompEnvironment context, Type typeHint)
        {
            var valueInfo = Evaluate(context);
            switch (valueInfo.ValueType)
            {
                case EValueType.Int:
                    return context.EvaluationStack.PopUnmanaged<int>();
                case EValueType.Float:
                    return context.EvaluationStack.PopUnmanaged<float>();
                case EValueType.Bool:
                    return context.EvaluationStack.PopUnmanaged<bool>();
                case EValueType.Str:
                    return context.EvaluationStack.PopObject<string>();
                case EValueType.Obj:
                    var obj = context.EvaluationStack.PopObject<object>();
                    if (obj is IFormatProvider formatProvider)
                        return formatProvider.GetFormat(typeHint);
                    return obj;
                default:
                    return null;
            }
        }

        public string GetAnyValueAsString(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            return valueInfo.ValueType switch
            {
                EValueType.Int => context.EvaluationStack.PopUnmanaged<int>().ToString(),
                EValueType.Float => context.EvaluationStack.PopUnmanaged<float>().ToString(CultureInfo.InvariantCulture),
                EValueType.Bool => context.EvaluationStack.PopUnmanaged<bool>().ToString(),
                EValueType.Str => context.EvaluationStack.PopObject<string>(),
                EValueType.Obj => context.EvaluationStack.PopObject<object>()?.ToString() ?? null,
                _ => null
            };
        }

        public int GetIntValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Int)
                throw new Exception($"Invalid return type: {EValueType.Int} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.PopUnmanaged<int>();
        }
        
        public bool GetBoolValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Bool)
                throw new Exception($"Invalid return type: {EValueType.Bool} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.PopUnmanaged<bool>();
        }
        public float GetFloatValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Float)
                throw new Exception($"Invalid return type: {EValueType.Float} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.PopUnmanaged<float>();
        }

        public string GetStringValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Str)
                throw new Exception($"Invalid return type: {EValueType.Str} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.PopObject<string>();
        }
        
        public T GetObjectValue<T>(CompEnvironment context) where T : class
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Obj)
                throw new Exception($"Invalid return type: {EValueType.Obj} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.PopObject<T>();
        }
    }
}