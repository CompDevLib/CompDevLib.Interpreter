using System;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter.Parse
{
    public abstract class ASTNode
    {
        public Token Token;

        public abstract ValueInfo Evaluate(CompEnvironment context);

        public int GetIntValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Int)
                throw new Exception($"Invalid return type: {EValueType.Int} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.GetUnmanaged<int>(valueInfo.Offset);
        }
        
        public bool GetBoolValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Bool)
                throw new Exception($"Invalid return type: {EValueType.Bool} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.GetUnmanaged<bool>(valueInfo.Offset);
        }
        public float GetFloatValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Float)
                throw new Exception($"Invalid return type: {EValueType.Float} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.GetUnmanaged<float>(valueInfo.Offset);
        }

        public string GetStringValue(CompEnvironment context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Str)
                throw new Exception($"Invalid return type: {EValueType.Str} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.GetObject<string>(valueInfo.Offset);
        }
        
        public T GetObjectValue<T>(CompEnvironment context) where T : class
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Obj)
                throw new Exception($"Invalid return type: {EValueType.Obj} expected, {valueInfo.ValueType} given.");
            return context.EvaluationStack.GetObject<T>(valueInfo.Offset);
        }
    }
}