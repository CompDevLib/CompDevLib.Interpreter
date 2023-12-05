using System;

namespace CompDevLib.Interpreter.Parse
{
    public class EvaluationException : Exception
    {
        public EvaluationException(EOpCode opCode, ValueInfo valueInfoA, ValueInfo valueInfoB)
        {
        }
        
        public EvaluationException(EOpCode opCode, ValueInfo valueInfo)
        {
        }

        public EvaluationException(EOpCode opCode, Type operandTypeA, Type operandTypeB)
        {
        }
        
        public EvaluationException(EOpCode opCode, Type operandType)
        {
        }

        public EvaluationException(EOpCode opCode, int operandCount)
        {
        }
    }
}