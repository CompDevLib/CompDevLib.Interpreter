using System;
using System.Reflection;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class ConvertedFunction<TContext> : IFunction<TContext> 
        where TContext : ICompInterpreterContext<TContext>
    {
        public readonly MethodInfo Function;
        public readonly EValueType ReturnValueType;
        public readonly bool NeedContext;
        private readonly object[] _paramObjects;

        public ConvertedFunction(MethodInfo methodInfo)
        {
            Function = methodInfo;
            var parameters = methodInfo.GetParameters();
            _paramObjects = new object[parameters.Length];
            NeedContext = parameters.Length > 0 && parameters[0].ParameterType == typeof(TContext);
            ReturnValueType = Utilities.ParseValueType(methodInfo.ReturnType);
        }

        public ConvertedFunction(Delegate function)
        {
            Function = function.Method;
            var parameters = function.Method.GetParameters();
            _paramObjects = new object[parameters.Length];
            NeedContext = parameters.Length > 0 && parameters[0].ParameterType == typeof(TContext);
            ReturnValueType = Utilities.ParseValueType(function.Method.ReturnType);
        }

        public ValueInfo Invoke(TContext context, ASTNode[] parameters)
        {
            if (NeedContext)
            {
                if (_paramObjects.Length != parameters.Length + 1)
                    throw new ArgumentException($"Insufficient argument count: {_paramObjects.Length - 1} needed, {parameters.Length} given.");
                _paramObjects[0] = context;
                for (int i = 1; i < _paramObjects.Length; i++)
                    _paramObjects[i] = parameters[i - 1].GetAnyValue(context.Environment);
            }
            else
            {
                if (_paramObjects.Length != parameters.Length)
                    throw new ArgumentException($"Insufficient argument count: {_paramObjects.Length} needed, {parameters.Length} given.");
                for (int i = 0; i < parameters.Length; i++)
                    _paramObjects[i] = parameters[i].GetAnyValue(context.Environment);
            }
            // TODO: Type conversion here is pretty slow and unnecessary for most cases, so it is not supported for now.
            //Convert.ChangeType(parameters[i].GetAnyValue(context.Environment), parameterInfos[i].ParameterType);
            
            // execution
            var result = Function.Invoke(null, _paramObjects);
            
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