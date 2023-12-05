using System;

namespace CompDevLib.Interpreter
{
    public class CompInstructionException : Exception
    {
        public CompInstructionException(string message) : base(message)
        {
        }

        public static CompInstructionException CreateInvalidReturnType(string instructionStr, Type expectedType, Type actualType)
        {
            return new CompInstructionException($"Instruction {instructionStr} is expecting a return value of type {expectedType}, but {actualType.Name} is returned.");
        }
    }
}