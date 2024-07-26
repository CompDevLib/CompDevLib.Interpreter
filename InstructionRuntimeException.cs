using System;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class InstructionRuntimeException : Exception
    {
        public InstructionRuntimeException(string message) : base(message)
        {
        }

        public static InstructionRuntimeException CreateInvalidReturnType(string instructionStr,
            Type expectedType, Type actualType)
            => new($"Instruction {instructionStr} is expecting a return value of type {expectedType.Name}, but {actualType.Name} is returned.");

        public static InstructionRuntimeException CreateInvalidReturnType(string instructionStr, 
            EValueType expectedType, EValueType actualType)
            => new($"Instruction {instructionStr} is expecting a return value of type {expectedType}, but {actualType} is returned.");
    }
}