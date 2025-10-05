using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class Evaluator
    {
        private object _currentOwner;
        public readonly EvaluationStack EvaluationStack = new();
        private readonly List<IValueSelector> _valueSelectors = new();
        private readonly Dictionary<Type, Dictionary<Type, IStackTopValueConverter>> _stackTopValueConverters = new();
        private readonly Dictionary<Type, Dictionary<Type, IValueConverter>> _valueConverters = new();
        private readonly Dictionary<string, IObjectInitializer> _objectInitializers = new ();

        public T CurrentOwnerAs<T>()
        {
            return (T)_currentOwner;
        }
        
        #region ObjectInitializer

        public void RegisterObjectInitializer(string typeIdentifier, IObjectInitializer initializer)
        {
            _objectInitializers.Add(typeIdentifier, initializer);
        }

        public void UnregisterObjectInitializer(string typeIdentifier)
        {
            _objectInitializers.Remove(typeIdentifier);
        }
        
        #endregion

        #region TypeConversion

        public void RegisterValueConversion(Type srcType, Type dstType, IStackTopValueConverter valueConverter)
        {
            if (!_stackTopValueConverters.TryGetValue(srcType, out var conversions))
            {
                conversions = new Dictionary<Type, IStackTopValueConverter>();
                _stackTopValueConverters.Add(srcType, conversions);
            }
            conversions.Add(dstType, valueConverter);
        }

        public void RegisterValueConversion(Type srcType, Type dstType, StackTopValueConverter.Conversion valueConversion)
        {
            RegisterValueConversion(srcType, dstType, new StackTopValueConverter(valueConversion));
        }

        public void RegisterValueConversion(Type srcType, Type dstType, IValueConverter valueConverter)
        {
            if (!_valueConverters.TryGetValue(srcType, out var conversions))
            {
                conversions = new Dictionary<Type, IValueConverter>();
                _valueConverters.Add(srcType, conversions);
            }
            conversions.Add(dstType, valueConverter);
        }
        public void RegisterValueConversion(Type srcType, Type dstType, ValueConverter.Conversion valueConversion)
        {
            RegisterValueConversion(srcType, dstType, new ValueConverter(valueConversion));
        }

        public void UnregisterValueConversion(Type srcType, Type dstType)
        {
            if (!_valueConverters.TryGetValue(srcType, out var conversions)) return;
            conversions.Remove(dstType);
        }

        public object ConvertValue(object obj, Type dstType)
        {
            if (obj == null) return null;
            
            var srcType = obj.GetType();
            if (dstType.IsAssignableFrom(srcType)) return obj;
            
            if (_valueConverters.TryGetValue(srcType, out var converters) &&
                converters.TryGetValue(dstType, out var converter))
                return converter.Convert(obj);

            if (obj is IFormatProvider formatProvider)
                return formatProvider.GetFormat(dstType);

            if (dstType.IsEnum)
                return Enum.ToObject(dstType, obj);
            
            return Convert.ChangeType(obj, dstType);
        }

        public ValueInfo ConvertValue(ValueInfo srcValueInfo, Type dstType)
        {
            var srcType = srcValueInfo.ValueType == EValueType.Obj
                ? EvaluationStack.GetObject<object>(srcValueInfo.Offset)?.GetType() ?? typeof(object)
                : srcValueInfo.ValueType.GetRuntimeType();

            if (srcType == dstType) return srcValueInfo;
            
            if (_stackTopValueConverters.TryGetValue(srcType, out var converters) &&
                converters.TryGetValue(dstType, out var converter))
                return converter.Convert(this, srcValueInfo);
            
            return srcValueInfo;
        }

        public void RemoveTopValue(ValueInfo valueInfo)
        {
            switch (valueInfo.ValueType)
            {
                case EValueType.Int:
                    EvaluationStack.PopUnmanaged<int>();
                    break;
                case EValueType.Float:
                    EvaluationStack.PopUnmanaged<float>();
                    break;
                case EValueType.Bool:
                    EvaluationStack.PopUnmanaged<bool>();
                    break;
                case EValueType.Str:
                    EvaluationStack.PopObject<string>();
                    break;
                case EValueType.Obj:
                    EvaluationStack.PopObject<object>();
                    break;
            }
        }

        public object PopTopValue(ValueInfo valueInfo)
        {
            return valueInfo.ValueType switch
            {
                EValueType.Int => EvaluationStack.PopUnmanaged<int>(),
                EValueType.Float => EvaluationStack.PopUnmanaged<float>(),
                EValueType.Bool => EvaluationStack.PopUnmanaged<bool>(),
                EValueType.Str => EvaluationStack.PopObject<string>(),
                EValueType.Obj => EvaluationStack.PopObject<object>(),
                _ => null
            };
        }

        public object GetValue(ValueInfo valueInfo)
        {
            return valueInfo.ValueType switch
            {
                EValueType.Int => EvaluationStack.GetUnmanaged<int>(valueInfo.Offset),
                EValueType.Float => EvaluationStack.GetUnmanaged<float>(valueInfo.Offset),
                EValueType.Bool => EvaluationStack.GetUnmanaged<bool>(valueInfo.Offset),
                EValueType.Str => EvaluationStack.GetObject<string>(valueInfo.Offset),
                EValueType.Obj => EvaluationStack.GetObject<object>(valueInfo.Offset),
                _ => null
            };
        }

        public string PopTopValueAsString(ValueInfo valueInfo)
        {
            return valueInfo.ValueType switch
            {
                EValueType.Int => EvaluationStack.PopUnmanaged<int>().ToString(),
                EValueType.Float => EvaluationStack.PopUnmanaged<float>().ToString(CultureInfo.InvariantCulture),
                EValueType.Bool => EvaluationStack.PopUnmanaged<bool>().ToString(),
                EValueType.Str => EvaluationStack.PopObject<string>(),
                EValueType.Obj => EvaluationStack.PopObject<object>()?.ToString(),
                _ => null
            };
        }

        public string GetValueAsString(ValueInfo valueInfo)
        {
            return valueInfo.ValueType switch
            {
                EValueType.Int => EvaluationStack.GetUnmanaged<int>(valueInfo.Offset).ToString(),
                EValueType.Float => EvaluationStack.GetUnmanaged<float>(valueInfo.Offset).ToString(CultureInfo.InvariantCulture),
                EValueType.Bool => EvaluationStack.GetUnmanaged<bool>(valueInfo.Offset).ToString(),
                EValueType.Str => EvaluationStack.GetObject<string>(valueInfo.Offset),
                EValueType.Obj => EvaluationStack.GetObject<object>(valueInfo.Offset)?.ToString(),
                _ => null
            };
        }
        
        #endregion
        

        #region ValueSelection
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
            if (_currentOwner is ICollection collection && key == "Count")
                return PushEvaluationResult(collection.Count);
            
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
                if (result.Offset < 0) continue;
                
                // non-indexing
                if (result.ValueType != EValueType.Int || _currentOwner == null) return result;
                
                // get element at index
                var index = EvaluationStack.PopUnmanaged<int>();
                return EvaluateListElement(_currentOwner, index);
            }

            if(_currentOwner == null)
                throw new ArgumentOutOfRangeException(nameof(key), $"Unable to select value with key: {key}");
            else
                throw new ArgumentOutOfRangeException(nameof(key), $"Unable to select member of {_currentOwner} with key: {key}");
        }
        #endregion

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
            if (opCode == EOpCode.Assign)
            {
                if (operandA is not VariableAstNode)
                    throw new EvaluationException(opCode, operandA, operandB);
                return operandB.Evaluate(this);
            }
            
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

                    // lazy evaluation
                    if (valueA)
                    {
                        if (opCode == EOpCode.Or) return PushEvaluationResult(true);
                    }
                    else
                    {
                        if (opCode == EOpCode.And) return PushEvaluationResult(false);
                    }

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
                    else
                    {
                        var valueInfoB = operandB.Evaluate(this);
                        if (valueInfoB.ValueType == EValueType.Obj)
                        {
                            var valueB = EvaluationStack.PopObject<object>();
                            return Evaluate(opCode, valueA, valueB);
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
                    return PushEvaluationResult((int)Math.Pow(valA, valB));
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
                    return PushEvaluationResult((float)Math.Pow(valA, valB));
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
                    return PushEvaluationResult((float)Math.Pow(valA, valB));
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
                    return PushEvaluationResult((float)Math.Pow(valA, valB));
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

        private ValueInfo Evaluate(EOpCode opCode, object valA, object valB)
        {
            switch (opCode)
            {
                case EOpCode.Eq:
                    return PushEvaluationResult(valA == valB);
                case EOpCode.Ne:
                    return PushEvaluationResult(valA != valB);
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
        
        public ValueInfo Evaluate(string typeIdentifier, ASTNode[] fields)
        {
            if (!_objectInitializers.TryGetValue(typeIdentifier, out var initializer))
                throw new EvaluationException($"Failed to initialize object of type {typeIdentifier}.");

            var instance = initializer.CreateInstance();
            foreach (var field in fields)
            {
                var expressionNode = (ExpressionAstNode) field;
                var fieldName = ((VariableAstNode) expressionNode.Operands[0]).Identifier;
                var fieldValue = expressionNode.Operands[1].GetAnyValue(this);
                initializer.SetField(instance, fieldName, fieldValue);
            }

            return PushEvaluationResult(instance);
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