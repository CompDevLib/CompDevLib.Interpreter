using System;

namespace CompDevLib.Interpreter.Parse
{
    public class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message)
        {
        }
        
        public EvaluationException(EOpCode opCode, ASTNode operandA, ASTNode operandB)
            : base($"Failed to evaluate \"{opCode}: {operandA}, {operandB}\"")
        {
        }

        public EvaluationException(EOpCode opCode, ASTNode operand)
            : base($"Failed to evaluate \"{opCode}: {operand}\"")
        {
        }
        
        public EvaluationException(EOpCode opCode, ValueInfo valueInfoA, ValueInfo valueInfoB)
            : base($"Failed to evaluate \"{opCode}: {valueInfoA.ValueType}, {valueInfoB.ValueType}\"")
        {
        }
        
        public EvaluationException(EOpCode opCode, ValueInfo valueInfo)
            : base($"Failed to evaluate \"{opCode}: {valueInfo.ValueType}\"")
        {
        }

        public EvaluationException(EOpCode opCode, Type operandTypeA, Type operandTypeB)
            : base($"Failed to evaluate \"{opCode}: {operandTypeA}, {operandTypeB}\"")
        {
        }
        
        public EvaluationException(EOpCode opCode, Type operandType)
            : base($"Failed to evaluate \"{opCode}: {operandType}\"")
        {
        }

        public EvaluationException(EOpCode opCode, int operandCount)
            : base($"Failed to evaluate {opCode} with operand count {operandCount}.")
        {
        }
    }
}