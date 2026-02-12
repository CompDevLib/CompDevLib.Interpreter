using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class Instruction<TContext> where TContext : IInterpreterContext<TContext>
    {
        public readonly IFunction<TContext> Function;
        public readonly ASTNode[] Parameters;
        public readonly IValueModifier<TContext>[] ReturnValueModifiers;
        public readonly string InstructionStr;

        public Instruction(string instructionStr, IFunction<TContext> func, ASTNode[] parameters, IValueModifier<TContext>[] returnValueModifiers)
        {
            InstructionStr = instructionStr;
            Function = func;
            Parameters = parameters;
            ReturnValueModifiers = returnValueModifiers;
        }

        public ValueInfo Execute(TContext context)
        {
            context.OnExecuteInstruction(this, Parameters);
            var ret = Function.Invoke(context, Parameters);
            for (int i = 0; i < ReturnValueModifiers.Length; i++)
                ret = ReturnValueModifiers[i].ModifyValue(context, ret);
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
            if(Parameters == null) return;
            for (int i = 0; i < Parameters.Length; i++)
                Parameters[i] = Parameters[i].Optimize(evaluator);
        }

        public override string ToString()
        {
            return InstructionStr;
        }
    }
}