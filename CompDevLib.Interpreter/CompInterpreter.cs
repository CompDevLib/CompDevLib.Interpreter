using System;
using System.Collections.Generic;
using CompDevLib.Interpreter.Tokenization;
using CompDevLib.Interpreter.Parse;
namespace CompDevLib.Interpreter
{
    public class CompInterpreter<TContext> where TContext : ICompInterpreterContext
    {
        public readonly Dictionary<string, CompInstruction<TContext>.Function> DefinedFunctions;

        private readonly Lexer _lexer;

        private struct OperatorInfo
        {
            public EOpCode OpCode;
            public byte OperandCount;
            public byte Precedence;
            public bool LeftAssociative;

            public OperatorInfo(EOpCode opCode, byte operandCount, byte precedence, bool leftAssociative = true)
            {
                OpCode = opCode;
                OperandCount = operandCount;
                Precedence = precedence;
                LeftAssociative = leftAssociative;
            }
        }

        private readonly Dictionary<ETokenType, OperatorInfo> _opCodes = new Dictionary<ETokenType, OperatorInfo>
        {
            {ETokenType.EQ, new OperatorInfo(EOpCode.Eq, 2, 2)},
            {ETokenType.NE, new OperatorInfo(EOpCode.Ne, 2, 2)},
            {ETokenType.GT, new OperatorInfo(EOpCode.Gt, 2, 3)},
            {ETokenType.LT, new OperatorInfo(EOpCode.Lt, 2, 3)},
            {ETokenType.GE, new OperatorInfo(EOpCode.Ge, 2, 3)},
            {ETokenType.LE, new OperatorInfo(EOpCode.Le, 2, 3)},
            {ETokenType.ADD, new OperatorInfo(EOpCode.Add, 2, 4)},
            {ETokenType.SUB, new OperatorInfo(EOpCode.Sub, 2, 4)},
            {ETokenType.MULT, new OperatorInfo(EOpCode.Mult, 2, 5)},
            {ETokenType.DIV, new OperatorInfo(EOpCode.Div, 2, 5)},
            {ETokenType.MOD, new OperatorInfo(EOpCode.Mod, 2, 5)},
            {ETokenType.POW, new OperatorInfo(EOpCode.Pow, 2, 6)},
            {ETokenType.AND, new OperatorInfo(EOpCode.And, 2, 1)},
            {ETokenType.OR, new OperatorInfo(EOpCode.Or, 2, 1)},
            {ETokenType.NOT, new OperatorInfo(EOpCode.Not, 1, 1)},
            {ETokenType.TYPE_MEMBER, new OperatorInfo(EOpCode.Member, 2, 9)},
        };

        private readonly Dictionary<ETokenType, EValueType> _values = new Dictionary<ETokenType, EValueType>
        {
            {ETokenType.IDENTIFIER, EValueType.Obj},
            {ETokenType.INT, EValueType.Int},
            {ETokenType.FLOAT, EValueType.Float},
            {ETokenType.BOOL, EValueType.Bool},
            {ETokenType.STR, EValueType.Str},
        };

        private readonly Stack<Token> _operatorTokenStack;
        private readonly Stack<ASTNode> _nodeStack;
        private readonly List<ASTNode> _result;

        public CompInterpreter()
        {
            DefinedFunctions = new Dictionary<string, CompInstruction<TContext>.Function>();
            _lexer = new Lexer();
            _operatorTokenStack = new Stack<Token>();
            _nodeStack = new Stack<ASTNode>();
            _result = new List<ASTNode>();
        }

        public void AddFunctionDefinition(string funcIdentifier, CompInstruction<TContext>.Function func)
        {
            DefinedFunctions.Add(funcIdentifier, func);
        }

        public ValueInfo Execute(TContext context, string instructionStr)
        {
            var instruction = BuildInstruction(instructionStr);
            return instruction.Execute(context);
        }

        public T Execute<T>(TContext context, string instructionStr)
        {
            var evaluationStack = context.Environment.EvaluationStack;
            var instruction = BuildInstruction(instructionStr);
            var retValInfo = instruction.Execute(context);
            var expectedRetType = typeof(T);
            // TODO: change getting value at offset to pop
            switch (retValInfo.ValueType)
            {
                case EValueType.Void:
                    if (expectedRetType != typeof(void))
                        throw CompInstructionException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(void));
                    break;
                case EValueType.Int:
                {
                    var retVal = evaluationStack.PopUnmanaged<int>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(int));
                }
                case EValueType.Float:
                {
                    var retVal = evaluationStack.PopUnmanaged<float>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(float));
                }
                case EValueType.Bool:
                {
                    var retVal = evaluationStack.PopUnmanaged<bool>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(bool));
                }
                case EValueType.Str:
                {
                    var retVal = evaluationStack.PopObject<string>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(string));
                }
                case EValueType.Obj:
                {
                    var retVal = evaluationStack.PopObject<object>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw CompInstructionException.CreateInvalidReturnType(instructionStr, expectedRetType, retVal.GetType());
                }
            }

            return default;
        }
        
        public CompInstruction<TContext> BuildInstruction(string instructionStr)
        {
            _lexer.Process(instructionStr);
            var tokens = _lexer.GetTokens();
            if (tokens.Count == 0 || tokens.Count == 2)
                throw new ArgumentException($"Unable to parse \"{instructionStr}\" to an instruction.");

            var funcIdentifierToken = tokens[0];
            if (funcIdentifierToken.TokenType != ETokenType.IDENTIFIER)
                throw new ArgumentException(
                    $"The first token of the given instruction \"{instructionStr}\" is not a valid function identifier.");
            
            if(!DefinedFunctions.TryGetValue(funcIdentifierToken.Value, out var func))
                throw new ArgumentException($"Undefined function {funcIdentifierToken.Value}.");

            if (tokens.Count > 2 && tokens[1].TokenType != ETokenType.COLON)
                throw new ArgumentException($"Colon is expected between function identifier and parameters for instruction \"{instructionStr}\".");
            
            var parameters = ParseParameters(tokens, 2);
            
            return new CompInstruction<TContext>(func, parameters);
        }
        
        public CompInstruction<TContext> BuildInstruction(string funcIdentifier, string paramStr)
        {
            if (!DefinedFunctions.TryGetValue(funcIdentifier, out var func))
                throw new ArgumentException($"Undefined function {funcIdentifier}.");
            
            _lexer.Process(paramStr);
            var tokens = _lexer.GetTokens();
            var parameters = ParseParameters(tokens, 0);
            return new CompInstruction<TContext>(func, parameters);
        }

        /// <summary>
        /// Parse tokens with expressions as parameters with shunting yard algorithm.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="beginIndex"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private ASTNode[] ParseParameters(IReadOnlyList<Token> tokens, int beginIndex)
        {
            if (beginIndex >= tokens.Count) return null;
            
            _result.Clear();
            for (int i = beginIndex; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.TokenType == ETokenType.OPEN_PR)
                {
                    _operatorTokenStack.Push(token);
                }
                else if (token.TokenType == ETokenType.CLOSE_PR)
                {
                    var topOperator = _operatorTokenStack.Pop();
                    while (topOperator.TokenType != ETokenType.OPEN_PR)
                    {
                        _nodeStack.Push(BuildExpressionNode(topOperator));
                        topOperator = _operatorTokenStack.Pop();
                    }
                    // TODO: Handle function on top of stack
                }
                else if (token.TokenType == ETokenType.COMMA)
                {
                    while (_operatorTokenStack.Count > 0)
                    {
                        var topOperator = _operatorTokenStack.Pop();
                        _nodeStack.Push(BuildExpressionNode(topOperator));
                    }
                    
                    var topNode = _nodeStack.Pop();
                    _result.Add(topNode);
                    if (_nodeStack.Count != 0)
                        throw new Exception($"node stack is not empty with {_nodeStack.Count} elements.");
                }
                else if (_opCodes.TryGetValue(token.TokenType, out var operatorInfo))
                {
                    while (_operatorTokenStack.Count > 0)
                    {
                        var topOperator = _operatorTokenStack.Peek();
                        if(topOperator.TokenType == ETokenType.OPEN_PR)
                            break;
                    
                        var topOperatorInfo = _opCodes[topOperator.TokenType];
                        if ((operatorInfo.Precedence < topOperatorInfo.Precedence) ||
                            (operatorInfo.Precedence == topOperatorInfo.Precedence &&
                             operatorInfo.LeftAssociative))
                        {
                            _operatorTokenStack.Pop();
                            _nodeStack.Push(BuildExpressionNode(topOperator));
                            continue;
                        }
                        break;
                    }

                    _operatorTokenStack.Push(token);
                }
                else if (_values.ContainsKey(token.TokenType))
                    _nodeStack.Push(BuildValueNode(token));
                else
                    throw new Exception($"Unable to process token of type {token.TokenType} at the given position.");
            }

            while (_operatorTokenStack.Count > 0)
            {
                var topOperator = _operatorTokenStack.Pop();
                _nodeStack.Push(BuildExpressionNode(topOperator));
            }

            var paramNode = _nodeStack.Pop();
            if (_nodeStack.Count != 0)
                throw new Exception($"node stack is not empty with {_nodeStack.Count} elements.");
            _result.Add(paramNode);
            return _result.ToArray();
        }

        private ASTNode BuildValueNode(Token token)
        {
            switch (token.TokenType)
            {
                case ETokenType.INT:
                {
                    int value = int.Parse(token.Value);
                    return new IntValueAstNode(value);
                }
                case ETokenType.FLOAT:
                {
                    float value = float.Parse(token.Value);
                    return new FloatValueAstNode(value);
                }
                case ETokenType.BOOL:
                {
                    bool value = bool.Parse(token.Value);
                    return new BoolValueAstNode(value);
                }
                case ETokenType.STR:
                {
                    return new StringValueAstNode(token.Value);
                }
                case ETokenType.IDENTIFIER:
                {
                    if (token.Value == "null")
                        return new ObjectValueAstNode(null);
                    return new VariableAstNode(token.Value);
                }
            }
            throw new ArgumentException($"Invalid token type as value: {token.TokenType}");
        }

        private ASTNode BuildExpressionNode(Token operatorToken)
        {
            var operatorInfo = _opCodes[operatorToken.TokenType];
            var operands = new ASTNode[operatorInfo.OperandCount];
            for (int i = operatorInfo.OperandCount - 1; i >= 0; i--)
                operands[i] = _nodeStack.Pop();
            return new ExpressionAstNode(operatorInfo.OpCode, operands);
        }

    }
}