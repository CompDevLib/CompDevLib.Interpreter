using System;

namespace CompDevLib.Interpreter.Parse
{
    public class VariableAstNode : ASTNode
    {
        public readonly string Identifier;

        public VariableAstNode(string identifier)
        {
            Identifier = identifier;
        }

        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            return evaluator.SelectValue(Identifier);
        }
    }
    
    public abstract class ValueAstNode<T> : ASTNode
    {
        protected readonly T Value;

        protected ValueAstNode(T value)
        {
            Value = value;
        }

        public override ASTNode Optimize(Evaluator evaluator)
        {
            return this;
        }

        public override bool IsConstValue()
        {
            return true;
        }
    }
    
    public class IntValueAstNode : ValueAstNode<int>
    {
        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            return evaluator.PushEvaluationResult(Value);
        }

        public IntValueAstNode(int value) : base(value)
        {
        }
    }
    public class FloatValueAstNode : ValueAstNode<float>
    {
        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            return evaluator.PushEvaluationResult(Value);
        }

        public FloatValueAstNode(float value) : base(value)
        {
        }
    }
    public class BoolValueAstNode : ValueAstNode<bool>
    {
        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            return evaluator.PushEvaluationResult(Value);
        }

        public BoolValueAstNode(bool value) : base(value)
        {
        }
    }
    public class StringValueAstNode : ValueAstNode<string>
    {
        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            return evaluator.PushEvaluationResult(Value);
        }

        public StringValueAstNode(string value) : base(value)
        {
        }
    }

    public class ListValueAstNode : ValueAstNode<Array>
    {
        public Type ElementType;
        public ASTNode[] Elements;
        public ListValueAstNode(Type elementType, ASTNode[] elements) : base(default)
        {
            ElementType = elementType;
            Elements = elements;
        }

        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            var instance = Array.CreateInstance(ElementType, Elements.Length);
            for (int i = 0; i < Elements.Length; i++)
            {
                var result = Elements[i].GetAnyValue(evaluator);
                instance.SetValue(result, i);
            }
            return evaluator.PushEvaluationResult(instance);
        }
        
        public override ASTNode Optimize(Evaluator evaluator)
        {
            bool isConstValue = true;
            for (int i = 0; i < Elements.Length; i++)
            {
                var optimizedNode = Elements[i].Optimize(evaluator);
                if (!optimizedNode.IsConstValue())
                    isConstValue = false;
                Elements[i] = optimizedNode;
            }

            if (!isConstValue) return this;
            
            Evaluate(evaluator);
            var evaluationStack = evaluator.EvaluationStack;
            return new ObjectValueAstNode(evaluationStack.PopObject<object>());
        }
    }

    public class ObjectValueAstNode : ValueAstNode<object>
    {
        public string TypeIdentifier;
        public ASTNode[] Fields;

        public ObjectValueAstNode(object value) : base(value)
        {
            TypeIdentifier = null;
            Fields = Array.Empty<ASTNode>();
        }
        
        public ObjectValueAstNode(string typeIdentifier, ASTNode[] fields) : base(null)
        {
            TypeIdentifier = typeIdentifier;
            Fields = fields;
        }

        public ObjectValueAstNode() : base(null)
        {
            TypeIdentifier = null;
            Fields = Array.Empty<ASTNode>();
        }

        public override ValueInfo Evaluate(Evaluator evaluator)
        {
            return TypeIdentifier != null
                ? evaluator.Evaluate(TypeIdentifier, Fields)
                : evaluator.PushEvaluationResult(Value);
        }
        
        public override ASTNode Optimize(Evaluator evaluator)
        {
            if (TypeIdentifier == null)
                return this;
            
            bool isConstValue = true;
            for (int i = 0; i < Fields.Length; i++)
            {
                var optimizedNode = Fields[i].Optimize(evaluator);
                if (!optimizedNode.IsConstValue())
                    isConstValue = false;
                Fields[i] = (ExpressionAstNode) optimizedNode;
            }

            if (!isConstValue) return this;
            
            var result = Evaluate(evaluator);
            var evaluationStack = evaluator.EvaluationStack;
            return result.ValueType switch
            {
                EValueType.Int => new IntValueAstNode(evaluationStack.PopUnmanaged<int>()),
                EValueType.Float => new FloatValueAstNode(evaluationStack.PopUnmanaged<float>()),
                EValueType.Bool => new BoolValueAstNode(evaluationStack.PopUnmanaged<bool>()),
                EValueType.Str => new StringValueAstNode(evaluationStack.PopObject<string>()),
                EValueType.Obj => new ObjectValueAstNode(evaluationStack.PopObject<object>()),
                _ => this
            };
        }
    }
}