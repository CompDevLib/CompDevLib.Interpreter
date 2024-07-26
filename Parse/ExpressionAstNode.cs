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

        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            switch (Operands.Length)
            {
                case 1:
                    return evaluator.Evaluate(OpCode, Operands[0]);
                case 2:
                    return evaluator.Evaluate(OpCode, Operands[0], Operands[1]);
                case 3:
                    return evaluator.Evaluate(OpCode, Operands[0], Operands[1], Operands[2]);
                default:
                    throw new EvaluationException(OpCode, Operands.Length);
            }
        }

        public override ASTNode Optimize(Evaluator evaluator)
        {
            bool isConstValue = true;
            for (int i = 0; i < Operands.Length; i++)
            {
                var optimizedNode = Operands[i].Optimize(evaluator);
                if (!optimizedNode.IsConstValue())
                    isConstValue = false;
                Operands[i] = optimizedNode;
            }

            if (!isConstValue) return this;
            
            var result = Evaluate(evaluator);
            var evaluationStack = evaluator.EvaluationStack;
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