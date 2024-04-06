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

        public override ValueInfo Evaluate(CompEnvironment context)
        {
            return context.SelectValue(Identifier);
        }
    }
    
    public abstract class ValueAstNode<T> : ASTNode
    {
        protected readonly T Value;

        protected ValueAstNode(T value)
        {
            Value = value;
        }

        public override ASTNode Optimize(CompEnvironment context)
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
        public override ValueInfo Evaluate(CompEnvironment context)
        {
            return context.PushEvaluationResult(Value);
        }

        public IntValueAstNode(int value) : base(value)
        {
        }
    }
    public class FloatValueAstNode : ValueAstNode<float>
    {
        public override ValueInfo Evaluate(CompEnvironment context)
        {
            return context.PushEvaluationResult(Value);
        }

        public FloatValueAstNode(float value) : base(value)
        {
        }
    }
    public class BoolValueAstNode : ValueAstNode<bool>
    {
        public override ValueInfo Evaluate(CompEnvironment context)
        {
            return context.PushEvaluationResult(Value);
        }

        public BoolValueAstNode(bool value) : base(value)
        {
        }
    }
    public class StringValueAstNode : ValueAstNode<string>
    {
        public override ValueInfo Evaluate(CompEnvironment context)
        {
            return context.PushEvaluationResult(Value);
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

        public override ValueInfo Evaluate(CompEnvironment context)
        {
            return context.PushEvaluationResult(Value);
        }
    }
}