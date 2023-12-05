using System;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter.Parse
{
    public abstract class ASTNode
    {
        public Token Token;

        public abstract NodeValueInfo Evaluate(ASTContext context);

        public int GetIntValue(ASTContext context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Int)
                throw new Exception($"Invalid return type: {EValueType.Int} expected, {valueInfo.ValueType}");
            return context.FixedDataBuffer.PopUnmanaged<int>();
        }
        
        public bool GetBoolValue(ASTContext context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Bool)
                throw new Exception($"Invalid return type: {EValueType.Bool} expected, {valueInfo.ValueType}");
            return context.FixedDataBuffer.PopUnmanaged<bool>();
        }
        public float GetFloatValue(ASTContext context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Float)
                throw new Exception($"Invalid return type: {EValueType.Float} expected, {valueInfo.ValueType}");
            return context.FixedDataBuffer.PopUnmanaged<float>();
        }

        public string GetStringValue(ASTContext context)
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Str)
                throw new Exception($"Invalid return type: {EValueType.Str} expected, {valueInfo.ValueType}");
            return context.FixedDataBuffer.PopObject<string>();
        }
        
        public T GetObjectValue<T>(ASTContext context) where T : class
        {
            var valueInfo = Evaluate(context);
            if (valueInfo.ValueType != EValueType.Obj)
                throw new Exception($"Invalid return type: {EValueType.Obj} expected, {valueInfo.ValueType}");
            return context.FixedDataBuffer.PopObject<T>();
        }
    }
}