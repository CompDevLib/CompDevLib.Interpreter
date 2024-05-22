using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class CompEnvironment
    {
        public readonly EvaluationStack EvaluationStack;
        private object _currentOwner;
        private readonly List<IValueSelector> _valueSelectors;

        public CompEnvironment()
        {
            EvaluationStack = new EvaluationStack();
            _valueSelectors = new List<IValueSelector>();
        }

        public T CurrentOwnerAs<T>()
        {
            return (T)_currentOwner;
        }

        public void RegisterValueSelector(IValueSelector.SelectValueFunc selectValueFunc)
        {
            _valueSelectors.Add(new ValueSelector(selectValueFunc));
        }

        public void RegisterValueSelector(IValueSelector valueSelector)
        {
            _valueSelectors.Add(valueSelector);
        }

        public void UnregisterValueSelector(IValueSelector.SelectValueFunc selectValueFunc)
        {
            for (int i = _valueSelectors.Count - 1; i >= 0; i--)
            {
                if (_valueSelectors[i] is not ValueSelector valueSelector ||
                    !valueSelector.EqualsToFunc(selectValueFunc)) continue;
                _valueSelectors.RemoveAt(i);
                return;
            }
        }

        public void UnregisterValueSelector(IValueSelector valueSelector)
        {
            for (int i = _valueSelectors.Count - 1; i >= 0; i--)
            {
                if (_valueSelectors[i] != valueSelector) continue;
                _valueSelectors.RemoveAt(i);
                return;
            }
        }

        public ValueInfo SelectValue(string key)
        {
            if (_currentOwner is IValueSelector valueSelector)
            {
                var result = valueSelector.SelectValue(this, key);
                if (result.Offset >= 0) return result;
                throw new ArgumentOutOfRangeException(nameof(key),
                    $"Unable to select member of owner {_currentOwner} with key {key}");
            }

            for (int i = _valueSelectors.Count - 1; i >= 0; i--)
            {
                var result = _valueSelectors[i].SelectValue(this, key);
                if (result.Offset >= 0)
                    return result;
            }

            throw new ArgumentOutOfRangeException(nameof(key), $"Unable to select value with key: {key}");
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

                    switch (valueInfoB.ValueType)
                    {
                        case EValueType.Int:
                        {
                            var valueB = EvaluationStack.PopUnmanaged<int>();
                            return Evaluate(opCode, valueA, valueB.ToString());
                        }
                        case EValueType.Float:
                        {
                            var valueB = EvaluationStack.PopUnmanaged<float>();
                            return Evaluate(opCode, valueA, valueB.ToString(CultureInfo.InvariantCulture));
                        }
                        case EValueType.Bool:
                        {
                            var valueB = EvaluationStack.PopUnmanaged<bool>();
                            return Evaluate(opCode, valueA, valueB.ToString(CultureInfo.InvariantCulture));
                        }
                        case EValueType.Str:
                        {
                            var valueB = EvaluationStack.PopObject<string>();
                            return Evaluate(opCode, valueA, valueB);
                        }
                        case EValueType.Obj:
                        {
                            var valueB = EvaluationStack.PopObject<object>();
                            return Evaluate(opCode, valueA, valueB.ToString());
                        }
                    }
                    break;
                }
                case EValueType.Obj:
                {
                    var valueA = EvaluationStack.PopObject<object>();
                    if (opCode == EOpCode.Member)
                    {
                        _currentOwner = valueA;
                        if (operandB is VariableAstNode variableAstNode)
                        {
                            var valueInfoB = variableAstNode.Evaluate(this);
                            _currentOwner = null;
                            return valueInfoB;
                        }
                        if (operandB is IntValueAstNode intValueAstNode)
                        {
                            var index = intValueAstNode.GetIntValue(this);
                            var elementValueInfo = EvaluateListElement(_currentOwner, index);
                            _currentOwner = null;
                            return elementValueInfo;
                        }
                    }
                    break;
                }
            }
            
            throw new EvaluationException(opCode, operandA, operandB);
        }

        public ValueInfo Evaluate(EOpCode opCode, ASTNode operandA, ASTNode operandB, ASTNode operandC)
        {
            if (opCode != EOpCode.Ternary) throw new EvaluationException(opCode, 3);
            
            var valueInfoA = operandA.Evaluate(this);
            if (valueInfoA.ValueType != EValueType.Bool) throw new EvaluationException(opCode, 3);

            var condition = EvaluationStack.PopUnmanaged<bool>();
            return condition ? operandB.Evaluate(this) : operandC.Evaluate(this);
        }

        public ValueInfo EvaluateListElement(object collection, int index)
        {
            return collection switch
            {
                IList<int> intList => PushEvaluationResult(intList[index]),
                IList<float> floatList => PushEvaluationResult(floatList[index]),
                IList<bool> boolList => PushEvaluationResult(boolList[index]),
                IList<string> strList => PushEvaluationResult(strList[index]),
                IList objList => PushEvaluationResult(objList[index]),
                _ => ValueInfo.Void,
            };
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
                case EOpCode.Pos:
                    return PushEvaluationResult(val);
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