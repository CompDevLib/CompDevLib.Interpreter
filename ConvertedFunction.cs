using System;
using System.Reflection;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    /// <summary>
    /// Converted function from method info.
    /// Boxing and type conversion are involved on invocation, so it is not recommended for cases where efficiency is a factor.
    /// </summary>
    public class ConvertedFunction<TContext> : IFunction<TContext> 
        where TContext : IInterpreterContext<TContext>
    {
        public readonly MethodInfo Function;
        public readonly EValueType ReturnValueType;
        public readonly bool NeedContext;
        private readonly object[] _paramObjects;
        private readonly ParameterInfo[] _paramInfos;
        public string Name { get; }

        public ConvertedFunction(string name, MethodInfo methodInfo)
        {
            Name = name;
            Function = methodInfo;
            _paramInfos = methodInfo.GetParameters();
            _paramObjects = new object[_paramInfos.Length];
            NeedContext = _paramInfos.Length > 0 && _paramInfos[0].ParameterType == typeof(TContext);
            ReturnValueType = Utilities.ParseValueType(methodInfo.ReturnType);
        }

        public ConvertedFunction(string name, Delegate function)
        {
            Name = name;
            Function = function.Method;
            _paramInfos = function.Method.GetParameters();
            _paramObjects = new object[_paramInfos.Length];
            NeedContext = _paramInfos.Length > 0 && _paramInfos[0].ParameterType == typeof(TContext);
            ReturnValueType = Utilities.ParseValueType(function.Method.ReturnType);
        }

        public ValueInfo Invoke(TContext context, ASTNode[] parameters)
        {
            if (NeedContext)
            {
                _paramObjects[0] = context;
                for (int i = 1; i < _paramObjects.Length; i++)
                {
                    var paramInfo = _paramInfos[i];
                    if (parameters.Length >= i)
                        _paramObjects[i] = parameters[i - 1].GetAnyValue(context.Evaluator, _paramInfos[i].ParameterType);
                    else if (paramInfo.HasDefaultValue)
                        _paramObjects[i] = paramInfo.DefaultValue;
                    else
                        throw new ArgumentException(
                            $"Insufficient argument count: {_paramObjects.Length - 1} needed, {parameters.Length} given.");
                }
            }
            else
            {
                for (int i = 0; i < _paramObjects.Length; i++)
                {
                    var paramInfo = _paramInfos[i];
                    if (parameters.Length > i)
                        _paramObjects[i] = parameters[i].GetAnyValue(context.Evaluator, _paramInfos[i].ParameterType);
                    else if (paramInfo.HasDefaultValue)
                        _paramObjects[i] = paramInfo.DefaultValue;
                    else
                        throw new ArgumentException(
                            $"Insufficient argument count: {_paramObjects.Length} needed, {parameters.Length} given.");
                }
            }
            
            // execution
            var result = Function.Invoke(null, _paramObjects);
            
            // prepare return value
            return ReturnValueType switch
            {
                EValueType.Void => ValueInfo.Void,
                EValueType.Int => context.Evaluator.PushEvaluationResult(Convert.ToInt32(result)),
                EValueType.Float => context.Evaluator.PushEvaluationResult(Convert.ToSingle(result)),
                EValueType.Bool => context.Evaluator.PushEvaluationResult(Convert.ToBoolean(result)),
                EValueType.Str => context.Evaluator.PushEvaluationResult((string) result),
                EValueType.Obj => context.Evaluator.PushEvaluationResult(result),
                _ => throw new Exception("Impossible branch."),
            };
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
}