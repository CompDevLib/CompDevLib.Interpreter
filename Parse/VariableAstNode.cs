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

        public override bool IsConstValue()
        {
            return false;
        }

        public override ASTNode Optimize(Evaluator evaluator)
        {
            for (int i = 0; i < Elements.Length; i++)
            {
                var optimizedNode = Elements[i].Optimize(evaluator);
                Elements[i] = optimizedNode;
            }

            return this;
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

        public override bool IsConstValue()
        {
            return false;
        }

        public override ASTNode Optimize(Evaluator evaluator)
        {
            if (TypeIdentifier == null)
                return this;
            
            for (int i = 0; i < Fields.Length; i++)
            {
                var optimizedNode = Fields[i].Optimize(evaluator);
                Fields[i] = (ExpressionAstNode) optimizedNode;
            }

            return this;
        }
    }
}