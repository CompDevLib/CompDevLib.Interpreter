using System;

namespace CompDevLib.Interpreter.Parse
{
    public static class Utilities
    {
        public static EValueType ParseValueType(Type type)
        {
            if (type == typeof(int))
                return EValueType.Int;
            if (type == typeof(float))
                return EValueType.Float;
            if (type == typeof(bool))
                return EValueType.Bool;
            if (type == typeof(string))
                return EValueType.Str;
            return EValueType.Obj;
        }
    }
}