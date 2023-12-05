using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CompDevLib.Pool;

namespace CompDevLib.Interpreter.Parse
{
    public class ASTContext
    {
        public readonly FixedDataBuffer FixedDataBuffer;

        public ASTContext()
        {
            FixedDataBuffer = new FixedDataBuffer();
        }
        
        public void Clear() => FixedDataBuffer.Clear();
        
        #region GetValues
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetInt(int offset) => FixedDataBuffer.GetUnmanaged<int>(offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBool(int offset) => FixedDataBuffer.GetUnmanaged<bool>(offset);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetFloat(int offset) => FixedDataBuffer.GetUnmanaged<float>(offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetObject<T>(int offset) where T : class 
            => FixedDataBuffer.GetObject<T>(offset);
        #endregion
        
        #region Evaluate Operation
        public NodeValueInfo Evaluate(EOpCode opCode, NodeValueInfo valueInfo)
        {
            switch (valueInfo.ValueType)
            {
                case EValueType.Int:
                {
                    var val = GetInt(valueInfo.Offset);
                    return Evaluate(opCode, val);
                }
                case EValueType.Float:
                {
                    var val = GetFloat(valueInfo.Offset);
                    return Evaluate(opCode, val);
                }
                case EValueType.Bool:
                {
                    var val = GetBool(valueInfo.Offset);
                    return Evaluate(opCode, val);
                }
                default:
                    throw new EvaluationException(opCode, valueInfo);
            }
        }
        
        public NodeValueInfo Evaluate(EOpCode opCode, NodeValueInfo valueInfoA, NodeValueInfo valueInfoB)
        {
            switch (valueInfoA.ValueType)
            {
                case EValueType.Int:
                {
                    var valueA = GetInt(valueInfoA.Offset);
                    switch (valueInfoB.ValueType)
                    {
                        case EValueType.Int:
                        {
                            var valueB = GetInt(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                        case EValueType.Float:
                        {
                            var valueB = GetFloat(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                    }

                    break;
                }
                case EValueType.Float:
                {
                    var valueA = GetFloat(valueInfoA.Offset);
                    switch (valueInfoB.ValueType)
                    {
                        case EValueType.Int:
                        {
                            var valueB = GetInt(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                        case EValueType.Float:
                        {
                            var valueB = GetFloat(valueInfoB.Offset);
                            return Evaluate(opCode, valueA, valueB);
                        }
                    }

                    break;
                }
                case EValueType.Bool:
                {
                    var valueA = GetBool(valueInfoA.Offset);
                    if (valueInfoB.ValueType == EValueType.Bool)
                    {
                        var valueB = GetBool(valueInfoB.Offset);
                        return Evaluate(opCode, valueA, valueB);
                    }
                    break;
                }
                case EValueType.Str:
                {
                    var valueA = GetObject<string>(valueInfoA.Offset);
                    if (valueInfoB.ValueType == EValueType.Str)
                    {
                        var valueB = GetObject<string>(valueInfoB.Offset);
                        return Evaluate(opCode, valueA, valueB);
                    }
                    break;
                }
            }
            
            throw new EvaluationException(opCode, valueInfoA, valueInfoB);
        }

        private NodeValueInfo Evaluate(EOpCode opCode, int valA, int valB)
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
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private NodeValueInfo Evaluate(EOpCode opCode, float valA, float valB)
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
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private NodeValueInfo Evaluate(EOpCode opCode, int valA, float valB)
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
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private NodeValueInfo Evaluate(EOpCode opCode, float valA, int valB)
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
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private NodeValueInfo Evaluate(EOpCode opCode, bool valA, bool valB)
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
        
        private NodeValueInfo Evaluate(EOpCode opCode, string valA, string valB)
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

        private NodeValueInfo Evaluate(EOpCode opCode, bool val)
        {
            switch (opCode)
            {
                case EOpCode.Not:
                    return AppendEvaluationResult(!val);
                default:
                    throw new EvaluationException(opCode, val.GetType());
            }
        }

        private NodeValueInfo Evaluate(EOpCode opCode, int val)
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

        private NodeValueInfo Evaluate(EOpCode opCode, float val)
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
        public NodeValueInfo AppendEvaluationResult(int value)
        {
            var offset = FixedDataBuffer.PushUnmanaged(value);
            return new NodeValueInfo
            {
                ValueType = EValueType.Int,
                Offset = offset,
            };
        }
        public NodeValueInfo AppendEvaluationResult(float value)
        {
            var offset = FixedDataBuffer.PushUnmanaged(value);
            return new NodeValueInfo
            {
                ValueType = EValueType.Float,
                Offset = offset,
            };
        }
        public NodeValueInfo AppendEvaluationResult(bool value)
        {
            var offset = FixedDataBuffer.PushUnmanaged(value);
            return new NodeValueInfo
            {
                ValueType = EValueType.Bool,
                Offset = offset,
            };
        }
        
        public NodeValueInfo AppendEvaluationResult(string value)
        {
            var offset = FixedDataBuffer.PushObject(value);
            return new NodeValueInfo
            {
                ValueType = EValueType.Str,
                Offset = offset,
            };
        }

        public NodeValueInfo AppendEvaluationResult(object value)
        {
            var offset = FixedDataBuffer.PushObject(value);
            return new NodeValueInfo
            {
                ValueType = EValueType.Obj,
                Offset = offset,
            };
        }
        #endregion
    }
}