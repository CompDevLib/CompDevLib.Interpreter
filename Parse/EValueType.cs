using System;

namespace CompDevLib.Interpreter.Parse
{
    public enum EValueType
    {
        Void, Int, Float, Bool, Str, Obj
    }

    public static class EValueTypeHelper
    {
        public static EValueType Parse(string str)
        {
            switch (str)
            {
                case "void":
                    return EValueType.Void;
                case "int":
                    return EValueType.Int;
                case "float":
                    return EValueType.Float;
                case "bool":
                    return EValueType.Bool;
                case "string":
                    return EValueType.Str;
                default:
                    return EValueType.Obj;
            }
        }

        public static Type GetRuntimeType(this EValueType valueType)
        {
            switch (valueType)
            {
                case EValueType.Void:
                    return typeof(void);
                case EValueType.Int:
                    return typeof(int);
                case EValueType.Float:
                    return typeof(float);
                case EValueType.Bool:
                    return typeof(bool);
                case EValueType.Str:
                    return typeof(string);
                case EValueType.Obj:
                    return typeof(object);
            }

            throw new ArgumentException($"Unrecognized value type {valueType}");
        }
    }
}