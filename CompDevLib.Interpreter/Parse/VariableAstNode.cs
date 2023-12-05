using System;

namespace CompDevLib.Interpreter.Parse
{
    public delegate NodeValueInfo ValueSelector(ASTContext context);

    public class VariableAstNode : ASTNode
    {
        public readonly string Identifier;
        protected readonly ValueSelector Delegate;

        public VariableAstNode(string identifier)
        {
            Identifier = identifier;
            if (Delegate == null)
                throw new ArgumentException($"Unrecognizable identifier {identifier}.");
        }
        
        public VariableAstNode(ValueSelector selectValueDel)
        {
            Delegate = selectValueDel;
        }

        public override NodeValueInfo Evaluate(ASTContext context)
        {
            return Delegate(context);
        }
    }
    
    public abstract class ValueAstNode<T> : ASTNode
    {
        protected readonly T Value;

        protected ValueAstNode(T value)
        {
            Value = value;
        }
    }
    
    public class IntValueAstNode : ValueAstNode<int>
    {
        public override NodeValueInfo Evaluate(ASTContext context)
        {
            return context.AppendEvaluationResult(Value);
        }

        public IntValueAstNode(int value) : base(value)
        {
        }
    }
    public class FloatValueAstNode : ValueAstNode<float>
    {
        public override NodeValueInfo Evaluate(ASTContext context)
        {
            return context.AppendEvaluationResult(Value);
        }

        public FloatValueAstNode(float value) : base(value)
        {
        }
    }
    public class BoolValueAstNode : ValueAstNode<bool>
    {
        public override NodeValueInfo Evaluate(ASTContext context)
        {
            return context.AppendEvaluationResult(Value);
        }

        public BoolValueAstNode(bool value) : base(value)
        {
        }
    }
    public class StringValueAstNode : ValueAstNode<string>
    {
        public override NodeValueInfo Evaluate(ASTContext context)
        {
            return context.AppendEvaluationResult(Value);
        }

        public StringValueAstNode(string value) : base(value)
        {
        }
    }

    public class ObjectValueAstNode : ValueAstNode<object>
    {
        public ObjectValueAstNode(object value) : base(value)
        {
        }

        public override NodeValueInfo Evaluate(ASTContext context)
        {
            return context.AppendEvaluationResult(Value);
        }
    }
}