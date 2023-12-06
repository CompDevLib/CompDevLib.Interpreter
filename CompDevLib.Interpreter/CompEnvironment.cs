using System;
using CompDevLib.Interpreter.Parse;
using CompDevLib.Pool;

namespace CompDevLib.Interpreter
{
    public class CompEnvironment
    {
        public readonly FixedDataBuffer EvaluationStack;

        public CompEnvironment()
        {
            EvaluationStack = new FixedDataBuffer();
        }

        #region Evaluate Operation
        public ValueInfo Evaluate(EOpCode opCode, ValueInfo valueInfo)
        {
            switch (valueInfo.ValueType)
            {
                case EValueType.Int:
                {
                    var val = EvaluationStack.GetUnmanaged<int>(valueInfo.Offset);
                    return Evaluate(opCode, val);
                }
                case EValueType.Float:
                {
                    var val = EvaluationStack.GetUnmanaged<float>(valueInfo.Offset);
                    return Evaluate(opCode, val);
                }
                case EValueType.Bool:
                {
                    var val = EvaluationStack.GetUnmanaged<bool>(valueInfo.Offset);
                    return Evaluate(opCode, val);
                }
                default:
                    throw new EvaluationException(opCode, valueInfo);
            }
        }
        
        public ValueInfo Evaluate(EOpCode opCode, ValueInfo valueInfoA, ValueInfo valueInfoB)
        {
            switch (valueInfoA.ValueType)
            {
                case EValueType.Int:
                {
                    var valueA = EvaluationStack.GetUnmanaged<int>(valueInfoA.Offset);
                    switch (valueInfoB.ValueType)
                    {
                        case EValueType.Int:
                        {
                            var valueB = EvaluationStack.GetUnmanaged<int>(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                        case EValueType.Float:
                        {
                            var valueB = EvaluationStack.GetUnmanaged<float>(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                    }

                    break;
                }
                case EValueType.Float:
                {
                    var valueA = EvaluationStack.GetUnmanaged<float>(valueInfoA.Offset);
                    switch (valueInfoB.ValueType)
                    {
                        case EValueType.Int:
                        {
                            var valueB = EvaluationStack.GetUnmanaged<int>(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                        case EValueType.Float:
                        {
                            var valueB = EvaluationStack.GetUnmanaged<float>(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                    }

                    break;
                }
                case EValueType.Bool:
                {
                    var valueA = EvaluationStack.GetUnmanaged<bool>(valueInfoA.Offset);
                    if (valueInfoB.ValueType == EValueType.Bool)
                    {
                        var valueB = EvaluationStack.GetUnmanaged<bool>(valueInfoB.Offset);
                        return Evaluate(opCode, valueA, valueB);
                    }
                    break;
                }
                case EValueType.Str:
                {
                    var valueA = EvaluationStack.GetObject<string>(valueInfoA.Offset);
                    if (valueInfoB.ValueType == EValueType.Str)
                    {
                        var valueB = EvaluationStack.GetObject<string>(valueInfoB.Offset);
                        return Evaluate(opCode, valueA, valueB);
                    }
                    break;
                }
            }
            
            throw new EvaluationException(opCode, valueInfoA, valueInfoB);
        }

        private ValueInfo Evaluate(EOpCode opCode, int valA, int valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return AppendEvaluationResult(valA == valB);
                case EOpCode.Ne:
                    return AppendEvaluationResult(valA != valB);
                case EOpCode.Lt:
                    return AppendEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return AppendEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return AppendEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return AppendEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return AppendEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return AppendEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return AppendEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return AppendEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return AppendEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return AppendEvaluationResult((int)MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private ValueInfo Evaluate(EOpCode opCode, float valA, float valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return AppendEvaluationResult(Math.Abs(valA - valB) <= float.Epsilon);
                case EOpCode.Ne:
                    return AppendEvaluationResult(Math.Abs(valA - valB) > float.Epsilon);
                case EOpCode.Lt:
                    return AppendEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return AppendEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return AppendEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return AppendEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return AppendEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return AppendEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return AppendEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return AppendEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return AppendEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return AppendEvaluationResult(MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, int valA, float valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return AppendEvaluationResult(Math.Abs(valA - valB) <= float.Epsilon);
                case EOpCode.Ne:
                    return AppendEvaluationResult(Math.Abs(valA - valB) > float.Epsilon);
                case EOpCode.Lt:
                    return AppendEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return AppendEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return AppendEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return AppendEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return AppendEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return AppendEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return AppendEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return AppendEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return AppendEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return AppendEvaluationResult(MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, float valA, int valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return AppendEvaluationResult(Math.Abs(valA - valB) <= float.Epsilon);
                case EOpCode.Ne:
                    return AppendEvaluationResult(Math.Abs(valA - valB) > float.Epsilon);
                case EOpCode.Lt:
                    return AppendEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return AppendEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return AppendEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return AppendEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return AppendEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return AppendEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return AppendEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return AppendEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return AppendEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return AppendEvaluationResult(MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private ValueInfo Evaluate(EOpCode opCode, bool valA, bool valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return AppendEvaluationResult(valA == valB);
                case EOpCode.Ne:
                    return AppendEvaluationResult(valA != valB);
                case EOpCode.And:
                    return AppendEvaluationResult(valA && valB);
                case EOpCode.Or:
                    return AppendEvaluationResult(valA || valB);
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private ValueInfo Evaluate(EOpCode opCode, string valA, string valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return AppendEvaluationResult(valA == valB);
                case EOpCode.Ne:
                    return AppendEvaluationResult(valA != valB);
                case EOpCode.Add:
                    return AppendEvaluationResult(valA + valB);
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, bool val)
        {
            switch (opCode)
            {
                case EOpCode.Not:
                    return AppendEvaluationResult(!val);
                default:
                    throw new EvaluationException(opCode, val.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, int val)
        {
            switch (opCode)
            {
                case EOpCode.Neg:
                    return AppendEvaluationResult(-val);
                case EOpCode.Inc:
                    return AppendEvaluationResult(val + 1);
                case EOpCode.Dec:
                    return AppendEvaluationResult(val - 1);
                default:
                    throw new EvaluationException(opCode, val.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, float val)
        {
            switch (opCode)
            {
                case EOpCode.Inc:
                    return AppendEvaluationResult(val + 1);
                case EOpCode.Dec:
                    return AppendEvaluationResult(val - 1);
                case EOpCode.Neg:
                    return AppendEvaluationResult(-val);
                default:
                    throw new EvaluationException(opCode, val.GetType());
            }
        }
        #endregion

        #region Evaluation Result
        public ValueInfo AppendEvaluationResult(int value)
        {
            var offset = EvaluationStack.PushUnmanaged(value);
            return new ValueInfo(EValueType.Int, offset);
        }
        public ValueInfo AppendEvaluationResult(float value)
        {
            var offset = EvaluationStack.PushUnmanaged(value);
            return new ValueInfo(EValueType.Float, offset);
        }
        public ValueInfo AppendEvaluationResult(bool value)
        {
            var offset = EvaluationStack.PushUnmanaged(value);
            return new ValueInfo(EValueType.Bool, offset);
        }
        
        public ValueInfo AppendEvaluationResult(string value)
        {
            var offset = EvaluationStack.PushObject(value);
            return new ValueInfo(EValueType.Str, offset);
        }

        public ValueInfo AppendEvaluationResult(object value)
        {
            var offset = EvaluationStack.PushObject(value);
            return new ValueInfo(EValueType.Obj, offset);
        }
        #endregion
    }
}