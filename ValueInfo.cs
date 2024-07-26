using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public struct ValueInfo
    {
        public readonly EValueType ValueType;
        public readonly int Offset;

        public ValueInfo(EValueType valueType, int offset)
        {
            ValueType = valueType;
            Offset = offset;
        }

        public static readonly ValueInfo Void = new ValueInfo(EValueType.Void, -1);
    }
}