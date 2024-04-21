using CompDevLib.Interpreter.Parse;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter
{
    public class CompInstruction<TContext> where TContext : ICompInterpreterContext<TContext>
    {
        private readonly IFunction<TContext> _function;
        private readonly ASTNode[] _parameters;
        private readonly IValueModifier<TContext>[] _returnValueModifiers;
        public readonly string InstructionStr;

        public CompInstruction(string instructionStr, IFunction<TContext> func, ASTNode[] parameters, IValueModifier<TContext>[] returnValueModifiers)
        {
            InstructionStr = instructionStr;
            _function = func;
            _parameters = parameters;
            _returnValueModifiers = returnValueModifiers;
        }

        public ValueInfo Execute(TContext context)
        {
            context.OnExecuteInstruction(this);
            var ret = _function.Invoke(context, _parameters);
            for (int i = 0; i < _returnValueModifiers.Length; i++)
                ret = _returnValueModifiers[i].ModifyValue(context, ret);
            return ret;
        }

        public T Execute<T>(TContext context)
        {
            var ret = Execute(context);
            return GetResult<T>(context.Environment, ret);
        }
        
        private T GetResult<T>(CompEnvironment environment, ValueInfo retValInfo)
        {
            var evaluationStack = environment.EvaluationStack;
            
            var expectedRetType = typeof(T);
            switch (retValInfo.ValueType)
            {
                case EValueType.Void:
                    if (expectedRetType != typeof(void))
                        throw CompInstructionException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(void));
                    break;
                case EValueType.Int:
                {
                    var retVal = evaluationStack.PopUnmanaged<int>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(int));
                }
                case EValueType.Float:
                {
                    var retVal = evaluationStack.PopUnmanaged<float>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(float));
                }
                case EValueType.Bool:
                {
                    var retVal = evaluationStack.PopUnmanaged<bool>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(bool));
                }
                case EValueType.Str:
                {
                    var retVal = evaluationStack.PopObject<string>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(string));
                }
                case EValueType.Obj:
                {
                    var retVal = evaluationStack.PopObject<object>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(ToString(), expectedRetType, retVal.GetType());
                }
            }

            return default;
        }
        
        public void Optimize(TContext context)
        {
            if(_parameters == null) return;
            for (int i = 0; i < _parameters.Length; i++)
                _parameters[i] = _parameters[i].Optimize(context.Environment);
        }

        public override string ToString()
        {
            return InstructionStr;
        }
    }
}