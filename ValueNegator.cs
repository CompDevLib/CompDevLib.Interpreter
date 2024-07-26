using System;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class ValueNegator<TContext> : IValueModifier<TContext> where TContext : ICompInterpreterContext<TContext>
    {
        public ValueInfo ModifyValue(TContext context, ValueInfo srcValueInfo)
        {
            var env = context.Environment;
            switch (srcValueInfo.ValueType)
            {
                case EValueType.Int:
                {
                    var srcValue = env.EvaluationStack.PopUnmanaged<int>();
                    return env.PushEvaluationResult(-srcValue);
                }
                case EValueType.Float:
                {
                    var srcValue = env.EvaluationStack.PopUnmanaged<float>();
                    return env.PushEvaluationResult(-srcValue);
                }
                case EValueType.Bool:
                {
                    var srcValue = env.EvaluationStack.PopUnmanaged<bool>();
                    return env.PushEvaluationResult(!srcValue);
                }
            }

            throw new InvalidOperationException($"Unable to negate value of type {srcValueInfo.ValueType}");
        }
    }
}