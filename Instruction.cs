using CompDevLib.Interpreter.Parse;
using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter
{
    public class Instruction<TContext> where TContext : IInterpreterContext<TContext>
    {
        private readonly IFunction<TContext> _function;
        private readonly ASTNode[] _parameters;
        private readonly IValueModifier<TContext>[] _returnValueModifiers;
        public readonly string InstructionStr;

        public Instruction(string instructionStr, IFunction<TContext> func, ASTNode[] parameters, IValueModifier<TContext>[] returnValueModifiers)
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
            context.OnInstructionEvaluated(ret);
            return ret;
        }

        public T Execute<T>(TContext context)
        {
            var ret = Execute(context);
            return GetResult<T>(context.Evaluator, ret);
        }
        
        private T GetResult<T>(Evaluator environment, ValueInfo retValInfo)
        {
            var evaluationStack = environment.EvaluationStack;
            
            var expectedRetType = typeof(T);
            switch (retValInfo.ValueType)
            {
                case EValueType.Void:
                    if (expectedRetType != typeof(void))
                        throw InstructionRuntimeException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(void));
                    break;
                case EValueType.Int:
                {
                    var retVal = evaluationStack.PopUnmanaged<int>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(int));
                }
                case EValueType.Float:
                {
                    var retVal = evaluationStack.PopUnmanaged<float>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(float));
                }
                case EValueType.Bool:
                {
                    var retVal = evaluationStack.PopUnmanaged<bool>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(bool));
                }
                case EValueType.Str:
                {
                    var retVal = evaluationStack.PopObject<string>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(ToString(), expectedRetType, typeof(string));
                }
                case EValueType.Obj:
                {
                    var retVal = evaluationStack.PopObject<object>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(ToString(), expectedRetType, retVal.GetType());
                }
            }

            return default;
        }
        
        public void Optimize(Evaluator evaluator)
        {
            if(_parameters == null) return;
            for (int i = 0; i < _parameters.Length; i++)
                _parameters[i] = _parameters[i].Optimize(evaluator);
        }

        public override string ToString()
        {
            return InstructionStr;
        }
    }
}