using System;
using CompDevLib.Interpreter.Parse;
using CompDevLib.Pool;

namespace CompDevLib.Interpreter
{
    public class CompEnvironment
    {
        public readonly FixedDataBuffer EvaluationStack;
        public readonly ValueSelector ValueSelector;
        private object _currentOwner;

        public CompEnvironment(ValueSelector valueSelector = null)
        {
            ValueSelector = valueSelector;
            EvaluationStack = new FixedDataBuffer();
        }

        #region Evaluate Operation
        public ValueInfo Evaluate(EOpCode opCode, ASTNode operand)
        {
            var valueInfo = operand.Evaluate(this);
            switch (valueInfo.ValueType)
            {
                case EValueType.Int:
                {
                    var val = EvaluationStack.PopUnmanaged<int>();
                    return Evaluate(opCode, val);
                }
                case EValueType.Float:
                {
                    var val = EvaluationStack.PopUnmanaged<float>();
                    return Evaluate(opCode, val);
                }
                case EValueType.Bool:
                {
                    var val = EvaluationStack.PopUnmanaged<bool>();
                    return Evaluate(opCode, val);
                }
                default:
                    throw new EvaluationException(opCode, valueInfo);
            }
        }
        
        public ValueInfo Evaluate(EOpCode opCode, ASTNode operandA, ASTNode operandB)
        {
            var valueInfoA = operandA.Evaluate(this);
            switch (valueInfoA.ValueType)
            {
                case EValueType.Int:
                {
                    var valueA = EvaluationStack.PopUnmanaged<int>();
                    var valueInfoB = operandB.Evaluate(this);
                    switch (valueInfoB.ValueType)
                    {
                        case EValueType.Int:
                        {
                            var valueB = EvaluationStack.PopUnmanaged<int>();
                            return Evaluate(opCode, valueA, valueB);
                        }
                        case EValueType.Float:
                        {
                            var valueB = EvaluationStack.PopUnmanaged<float>();
                            return Evaluate(opCode, valueA, valueB);
                        }
                    }

                    break;
                }
                case EValueType.Float:
                {
                    var valueA = EvaluationStack.PopUnmanaged<float>();
                    var valueInfoB = operandB.Evaluate(this);
                    switch (valueInfoB.ValueType)
                    {
                        case EValueType.Int:
                        {
                            var valueB = EvaluationStack.PopUnmanaged<int>();
                            return Evaluate(opCode, valueA, valueB);
                        }
                        case EValueType.Float:
                        {
                            var valueB = EvaluationStack.PopUnmanaged<float>();
                            return Evaluate(opCode, valueA, valueB);
                        }
                    }

                    break;
                }
                case EValueType.Bool:
                {
                    var valueA = EvaluationStack.PopUnmanaged<bool>();
                    var valueInfoB = operandB.Evaluate(this);
                    if (valueInfoB.ValueType == EValueType.Bool)
                    {
                        var valueB = EvaluationStack.PopUnmanaged<bool>();
                        return Evaluate(opCode, valueA, valueB);
                    }
                    break;
                }
                case EValueType.Str:
                {
                    var valueA = EvaluationStack.PopObject<string>();
                    var valueInfoB = operandB.Evaluate(this);
                    if (valueInfoB.ValueType == EValueType.Str)
                    {
                        var valueB = EvaluationStack.PopObject<string>();
                        return Evaluate(opCode, valueA, valueB);
                    }
                    break;
                }
                case EValueType.Obj:
                {
                    var valueA = EvaluationStack.PopObject<object>();
                    if (opCode == EOpCode.Member)
                    {
                        _currentOwner = valueA;
                        if (operandB is VariableAstNode astNode)
                        {
                            var valueInfoB = astNode.Evaluate(this);
                            _currentOwner = null;
                            return valueInfoB;
                        }
                        // TODO: indexing
                    }
                    break;
                }
            }
            
            throw new EvaluationException(opCode, operandA, operandB);
        }

        private ValueInfo Evaluate(EOpCode opCode, int valA, int valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return PushEvaluationResult(valA == valB);
                case EOpCode.Ne:
                    return PushEvaluationResult(valA != valB);
                case EOpCode.Lt:
                    return PushEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return PushEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return PushEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return PushEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return PushEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return PushEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return PushEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return PushEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return PushEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return PushEvaluationResult((int)MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private ValueInfo Evaluate(EOpCode opCode, float valA, float valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return PushEvaluationResult(Math.Abs(valA - valB) <= float.Epsilon);
                case EOpCode.Ne:
                    return PushEvaluationResult(Math.Abs(valA - valB) > float.Epsilon);
                case EOpCode.Lt:
                    return PushEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return PushEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return PushEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return PushEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return PushEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return PushEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return PushEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return PushEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return PushEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return PushEvaluationResult(MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, int valA, float valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return PushEvaluationResult(Math.Abs(valA - valB) <= float.Epsilon);
                case EOpCode.Ne:
                    return PushEvaluationResult(Math.Abs(valA - valB) > float.Epsilon);
                case EOpCode.Lt:
                    return PushEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return PushEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return PushEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return PushEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return PushEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return PushEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return PushEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return PushEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return PushEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return PushEvaluationResult(MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, float valA, int valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return PushEvaluationResult(Math.Abs(valA - valB) <= float.Epsilon);
                case EOpCode.Ne:
                    return PushEvaluationResult(Math.Abs(valA - valB) > float.Epsilon);
                case EOpCode.Lt:
                    return PushEvaluationResult(valA < valB);
                case EOpCode.Gt:
                    return PushEvaluationResult(valA > valB);
                case EOpCode.Le:
                    return PushEvaluationResult(valA <= valB);
                case EOpCode.Ge:
                    return PushEvaluationResult(valA >= valB);
                case EOpCode.Add:
                    return PushEvaluationResult(valA + valB);
                case EOpCode.Sub:
                    return PushEvaluationResult(valA - valB);
                case EOpCode.Mult:
                    return PushEvaluationResult(valA * valB);
                case EOpCode.Div:
                    return PushEvaluationResult(valA / valB);
                case EOpCode.Mod:
                    return PushEvaluationResult(valA % valB);
                case EOpCode.Pow:
                    return PushEvaluationResult(MathF.Pow(valA, valB));
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private ValueInfo Evaluate(EOpCode opCode, bool valA, bool valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return PushEvaluationResult(valA == valB);
                case EOpCode.Ne:
                    return PushEvaluationResult(valA != valB);
                case EOpCode.And:
                    return PushEvaluationResult(valA && valB);
                case EOpCode.Or:
                    return PushEvaluationResult(valA || valB);
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }
        
        private ValueInfo Evaluate(EOpCode opCode, string valA, string valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return PushEvaluationResult(valA == valB);
                case EOpCode.Ne:
                    return PushEvaluationResult(valA != valB);
                case EOpCode.Add:
                    return PushEvaluationResult(valA + valB);
                default:
                    throw new EvaluationException(opCode, valA.GetType(), valB.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, bool val)
        {
            switch (opCode)
            {
                case EOpCode.Not:
                    return PushEvaluationResult(!val);
                default:
                    throw new EvaluationException(opCode, val.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, int val)
        {
            switch (opCode)
            {
                case EOpCode.Neg:
                    return PushEvaluationResult(-val);
                case EOpCode.Inc:
                    return PushEvaluationResult(val + 1);
                case EOpCode.Dec:
                    return PushEvaluationResult(val - 1);
                default:
                    throw new EvaluationException(opCode, val.GetType());
            }
        }

        private ValueInfo Evaluate(EOpCode opCode, float val)
        {
            switch (opCode)
            {
                case EOpCode.Inc:
                    return PushEvaluationResult(val + 1);
                case EOpCode.Dec:
                    return PushEvaluationResult(val - 1);
                case EOpCode.Neg:
                    return PushEvaluationResult(-val);
                default:
                    throw new EvaluationException(opCode, val.GetType());
            }
        }
        #endregion

        #region Evaluation Result
        public ValueInfo PushEvaluationResult(int value)
        {
            var offset = EvaluationStack.PushUnmanaged(value);
            return new ValueInfo(EValueType.Int, offset);
        }
        public ValueInfo PushEvaluationResult(float value)
        {
            var offset = EvaluationStack.PushUnmanaged(value);
            return new ValueInfo(EValueType.Float, offset);
        }
        public ValueInfo PushEvaluationResult(bool value)
        {
            var offset = EvaluationStack.PushUnmanaged(value);
            return new ValueInfo(EValueType.Bool, offset);
        }
        
        public ValueInfo PushEvaluationResult(string value)
        {
            var offset = EvaluationStack.PushObject(value);
            return new ValueInfo(EValueType.Str, offset);
        }

        public ValueInfo PushEvaluationResult(object value)
        {
            var offset = EvaluationStack.PushObject(value);
            return new ValueInfo(EValueType.Obj, offset);
        }
        #endregion
    }
}