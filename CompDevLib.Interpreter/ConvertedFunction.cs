using System;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class ConvertedFunction<TContext> : IFunction<TContext> where TContext : ICompInterpreterContext
    {
        public readonly Delegate Function;
        public readonly EValueType ReturnValueType;
        private readonly object[] _paramObjects;

        public ConvertedFunction(Delegate function)
        {
            Function = function;
            var parameters = function.Method.GetParameters();
            _paramObjects = new object[parameters.Length];
            
            ReturnValueType = Utilities.ParseValueType(function.Method.ReturnType);
        }

        public ValueInfo Invoke(TContext context, ASTNode[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
                _paramObjects[i] = parameters[i].GetAnyValue(context.Environment);

            // TODO: Type conversion here is pretty slow and unnecessary for most cases, so it is not supported for now.
            //Convert.ChangeType(parameters[i].GetAnyValue(context.Environment), parameterInfos[i].ParameterType);
            
            // execution
            var result = Function.Method.Invoke(null, _paramObjects);
            
            // prepare return value
            return ReturnValueType switch
            {
                EValueType.Void => ValueInfo.Void,
                EValueType.Int => context.Environment.PushEvaluationResult((int) result),
                EValueType.Float => context.Environment.PushEvaluationResult((float) result),
                EValueType.Bool => context.Environment.PushEvaluationResult((bool) result),
                EValueType.Str => context.Environment.PushEvaluationResult((string) result),
                EValueType.Obj => context.Environment.PushEvaluationResult(result),
                _ => throw new Exception()
            };
        }
    }
}