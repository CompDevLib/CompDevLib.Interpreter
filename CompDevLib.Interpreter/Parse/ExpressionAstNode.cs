using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter.Parse
{
    public class ExpressionAstNode : ASTNode
    {
        public EOpCode OpCode;
        public ASTNode[] Operands;

        public ExpressionAstNode(EOpCode opCode, ASTNode[] operands)
        {
            OpCode = opCode;
            Operands = operands;
        }

        public override ValueInfo Evaluate(CompEnvironment context)
        {
            switch (Operands.Length)
            {
                case 1:
                {
                    var operand1Val = Operands[0].Evaluate(context);
                    return context.Evaluate(OpCode, operand1Val);
                }
                case 2:
                {
                    var operand2Val = Operands[1].Evaluate(context);
                    var operand1Val = Operands[0].Evaluate(context);
                    return context.Evaluate(OpCode, operand1Val, operand2Val);
                }
                default:
                    throw new EvaluationException(OpCode, Operands.Length);
            }
        }

        public override ASTNode Optimize(CompEnvironment context)
        {
            bool isConstValue = true;
            for (int i = 0; i < Operands.Length; i++)
            {
                var optimizedNode = Operands[i].Optimize(context);
                if (!optimizedNode.IsConstValue())
                    isConstValue = false;
                Operands[i] = optimizedNode;
            }

            if (!isConstValue) return this;
            
            var result = Evaluate(context);
            var evaluationStack = context.EvaluationStack;
            return result.ValueType switch
            {
                EValueType.Int => new IntValueAstNode(evaluationStack.PopUnmanaged<int>()),
                EValueType.Float => new FloatValueAstNode(evaluationStack.PopUnmanaged<float>()),
                EValueType.Bool => new BoolValueAstNode(evaluationStack.PopUnmanaged<bool>()),
                EValueType.Str => new StringValueAstNode(evaluationStack.PopObject<string>()),
                EValueType.Obj => new ObjectValueAstNode(evaluationStack.PopObject<object>()),
                _ => this
            };
        }
    }
}