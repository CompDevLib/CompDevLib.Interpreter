using System;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class CompInstructionException : Exception
    {
        public CompInstructionException(string message) : base(message)
        {
        }

        public static CompInstructionException CreateInvalidReturnType(string instructionStr,
            Type expectedType, Type actualType)
            => new($"Instruction {instructionStr} is expecting a return value of type {expectedType.Name}, but {actualType.Name} is returned.");

        public static CompInstructionException CreateInvalidReturnType(string instructionStr, 
            EValueType expectedType, EValueType actualType)
            => new($"Instruction {instructionStr} is expecting a return value of type {expectedType}, but {actualType} is returned.");
    }
}