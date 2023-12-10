using System.Runtime.InteropServices;

namespace CompDevLib.Interpreter
{
    [StructLayout(LayoutKind.Explicit)]
    public struct FixedSizeValue
    {
        [FieldOffset(0)] private int _intVal;
        [FieldOffset(0)] private float _floatVal;
        [FieldOffset(0)] private bool _boolVal;
        [FieldOffset(0)] private object _objRef;

        public FixedSizeValue(int val)
        {
            _floatVal = default;
            _boolVal = default;
            _objRef = default;
            _intVal = val;
        }
        public FixedSizeValue(float val)
        {
            _intVal = default;
            _boolVal = default;
            _objRef = default;
            _floatVal = val;
        }
        public FixedSizeValue(bool val)
        {
            _intVal = default;
            _floatVal = default;
            _objRef = default;
            _boolVal = val;
        }
        public FixedSizeValue(object val)
        {
            _intVal = default;
            _floatVal = default;
            _boolVal = default;
            _objRef = val;
        }

        public int AsInt() => _intVal;
        public float AsFloat() => _floatVal;
        public bool AsBool() => _boolVal;
        public object AsObject() => _objRef;
        
        public static implicit operator int(FixedSizeValue fixedSizeValue) => fixedSizeValue._intVal;
        public static implicit operator float(FixedSizeValue fixedSizeValue) => fixedSizeValue._floatVal;
        public static implicit operator bool(FixedSizeValue fixedSizeValue) => fixedSizeValue._boolVal;
    }
}