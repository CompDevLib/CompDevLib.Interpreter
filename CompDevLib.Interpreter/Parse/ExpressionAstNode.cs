using CompDevLib.Interpreter.Tokenization;

namespace CompDevLib.Interpreter.Parse
{
    public class ExpressionAstNode : ASTNode
    {
        public EOpCode OpCode;
        public ASTNode[] Operands;

        public ExpressionAstNode(Token token, ASTNode[] operands)
        {
        }

        public override NodeValueInfo Evaluate(ASTContext context)
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
                    var operand1Val = Operands[0].Evaluate(context);
                    var operand2Val = Operands[1].Evaluate(context);
                    return context.Evaluate(OpCode, operand1Val, operand2Val);
                }
                default:
                    throw new EvaluationException(OpCode, Operands.Length);
            }
        }
    }
}