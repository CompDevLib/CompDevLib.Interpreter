using System;
using System.Collections.Generic;
using System.Reflection;
using CompDevLib.Interpreter.Tokenization;
using CompDevLib.Interpreter.Parse;

namespace CompDevLib.Interpreter
{
    public class Interpreter<TContext> where TContext : IInterpreterContext<TContext>
    {
        public readonly Dictionary<string, IFunction<TContext>> DefinedFunctions;
        public readonly Dictionary<string, IValueModifier<TContext>> DefinedValueModifiers;

        private readonly Lexer _lexer;

        private struct OperatorInfo
        {
            public readonly EOpCode OpCode;
            public readonly byte OperandCount;
            public readonly byte Precedence;
            public readonly bool LeftAssociative;

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
            {ETokenType.NEG, new OperatorInfo(EOpCode.Neg, 1, 6)},
            {ETokenType.POS, new OperatorInfo(EOpCode.Pos, 1, 6)},
            {ETokenType.POW, new OperatorInfo(EOpCode.Pow, 2, 7)},
            {ETokenType.AND, new OperatorInfo(EOpCode.And, 2, 1)},
            {ETokenType.OR, new OperatorInfo(EOpCode.Or, 2, 1)},
            {ETokenType.NOT, new OperatorInfo(EOpCode.Not, 1, 1)},
            {ETokenType.TYPE_MEMBER, new OperatorInfo(EOpCode.Member, 2, 9)},
            {ETokenType.ASSIGN, new OperatorInfo(EOpCode.Assign, 2, 0)},
        };

        private readonly Dictionary<ETokenType, EValueType> _values = new Dictionary<ETokenType, EValueType>
        {
            {ETokenType.IDENTIFIER, EValueType.Obj},
            {ETokenType.INT, EValueType.Int},
            {ETokenType.FLOAT, EValueType.Float},
            {ETokenType.BOOL, EValueType.Bool},
            {ETokenType.STR, EValueType.Str},
        };

        private readonly Dictionary<ETokenType, ETokenType> _specialUnary = new()
        {
            {ETokenType.ADD, ETokenType.POS},
            {ETokenType.SUB, ETokenType.NEG},
        };

        private readonly HashSet<ETokenType> _closures = new HashSet<ETokenType>()
        {
            ETokenType.CLOSE_BRACE,
            ETokenType.CLOSE_BRACKET,
        };
        
        private readonly HashSet<string> ElementTypeNames = new HashSet<string>()
        {
            "int",
            "float",
            "bool",
            "string"
        };
        
        
        private class ExpressionParserScope
        {
            public readonly Stack<ASTNode> NodeStack = new Stack<ASTNode>();
            public readonly Stack<Token> OperatorTokenStack = new Stack<Token>();
            public readonly List<ASTNode> Result = new List<ASTNode>();

            public void Clear()
            {
                NodeStack.Clear();
                OperatorTokenStack.Clear();
                Result.Clear();
            }
        }
        private readonly List<ExpressionParserScope> _scopePool;
        private readonly List<IValueModifier<TContext>> _modifiers;
        public bool OptimizeInstructionOnBuild;
        public TContext DefaultContext;

        public Interpreter(bool optimizeInstructionOnBuild = true)
        {
            DefinedFunctions = new Dictionary<string, IFunction<TContext>>();
            DefinedValueModifiers = new Dictionary<string, IValueModifier<TContext>>();
            _scopePool = new List<ExpressionParserScope>();
            _lexer = new Lexer();
            _modifiers = new List<IValueModifier<TContext>>();
            OptimizeInstructionOnBuild = optimizeInstructionOnBuild;
            InitializePredefinedFunctions();
        }
        
        public Interpreter(TContext defaultContext, bool optimizeInstructionOnBuild = true)
        {
            DefinedFunctions = new Dictionary<string, IFunction<TContext>>();
            DefinedValueModifiers = new Dictionary<string, IValueModifier<TContext>>();
            _scopePool = new List<ExpressionParserScope>();
            _lexer = new Lexer();
            OptimizeInstructionOnBuild = optimizeInstructionOnBuild;
            DefaultContext = defaultContext;
            InitializePredefinedFunctions();
        }

        private void InitializePredefinedFunctions()
        {
            AddFunctionDefinition(nameof(PredefinedFunctions.Print), PredefinedFunctions.Print);
            AddFunctionDefinition(nameof(PredefinedFunctions.Evaluate), PredefinedFunctions.Evaluate);
            AddReturnValueModifierDefinition("neg", new ValueNegator<TContext>());
        }
        
        #region FunctionDefinition
        
        public void AddFunctionDefinition(string funcIdentifier, Delegate func)
        {
            var convertedFunction = new ConvertedFunction<TContext>(funcIdentifier, func);
            DefinedFunctions.Add(funcIdentifier, convertedFunction);
        }

        public void AddFunctionDefinition(string funcIdentifier, MethodInfo methodInfo)
        {
            var convertedFunction = new ConvertedFunction<TContext>(funcIdentifier, methodInfo);
            DefinedFunctions.Add(funcIdentifier, convertedFunction);
        }
        
        public void AddFunctionDefinition(string funcIdentifier, StandardFunction<TContext>.Function func)
        {
            var standardFunction = new StandardFunction<TContext>(funcIdentifier, func);
            DefinedFunctions.Add(funcIdentifier, standardFunction);
        }

        public void AddFunctionDefinition(string funcIdentifier, IFunction<TContext> func)
        {
            DefinedFunctions.Add(funcIdentifier, func);
        }
        
        #endregion

        public void AddReturnValueModifierDefinition(string funcIdentifier, IValueModifier<TContext> modifier)
        {
            DefinedValueModifiers.Add(funcIdentifier, modifier);
        }

        #region Execution

        public ValueInfo Execute(string instructionStr) => Execute(DefaultContext, instructionStr);
        public T Execute<T>(string instructionStr) => Execute<T>(DefaultContext, instructionStr);
        public T Execute<T>(Instruction<TContext> instruction) => Execute<T>(DefaultContext, instruction);
        public T EvaluateExpression<T>(string expressionStr) => EvaluateExpression<T>(DefaultContext, expressionStr);
        
        public ValueInfo Execute(TContext context, string instructionStr)
        {
            var instruction = BuildInstruction(context, instructionStr);
            return instruction.Execute(context);
        }

        public T Execute<T>(TContext context, string instructionStr)
        {
            var evaluationStack = context.Evaluator.EvaluationStack;
            var instruction = BuildInstruction(context, instructionStr);
            var retValInfo = instruction.Execute(context);
            
            return GetResult<T>(evaluationStack, retValInfo, instructionStr);
        }

        public T Execute<T>(TContext context, Instruction<TContext> instruction)
        {
            var evaluationStack = context.Evaluator.EvaluationStack;
            var retValInfo = instruction.Execute(context);
            
            return GetResult<T>(evaluationStack, retValInfo, instruction.ToString());
        }

        public T EvaluateExpression<T>(TContext context, string expressionStr)
        {
            _lexer.Process(expressionStr);
            var tokens = _lexer.GetTokens();
            var beginIndex = 0;
            var parsedResult = ParseParameters(tokens, ref beginIndex);
            if (parsedResult.Length != 1)
                throw new ArgumentException($"Unable to parse \"{expressionStr}\" as a single expression");
            var result = parsedResult[0].Evaluate(context.Evaluator);
            return GetResult<T>(context.Evaluator.EvaluationStack, result, expressionStr);
        }

        private T GetResult<T>(EvaluationStack evaluationStack, ValueInfo retValInfo, string instructionStr)
        {
            var expectedRetType = typeof(T);
            switch (retValInfo.ValueType)
            {
                case EValueType.Void:
                    if (expectedRetType != typeof(void))
                        throw InstructionRuntimeException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(void));
                    break;
                case EValueType.Int:
                {
                    var retVal = evaluationStack.PopUnmanaged<int>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(int));
                }
                case EValueType.Float:
                {
                    var retVal = evaluationStack.PopUnmanaged<float>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(float));
                }
                case EValueType.Bool:
                {
                    var retVal = evaluationStack.PopUnmanaged<bool>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(bool));
                }
                case EValueType.Str:
                {
                    var retVal = evaluationStack.PopObject<string>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(instructionStr, expectedRetType, typeof(string));
                }
                case EValueType.Obj:
                {
                    var retVal = evaluationStack.PopObject<object>();
                    if (retVal is T parsedRetVal) return parsedRetVal;
                    throw InstructionRuntimeException.CreateInvalidReturnType(instructionStr, expectedRetType, retVal.GetType());
                }
            }

            return default;
        }
        #endregion
        
        #region Parsing
        public Instruction<TContext> BuildInstruction(TContext context, string instructionStr)
        {
            _lexer.Process(instructionStr);
            var tokens = _lexer.GetTokens();
            if (tokens.Count == 0)
                throw new ArgumentException($"Unable to parse \"{instructionStr}\" to an instruction.");

            var funcIdentifierToken = tokens[0];
            if (funcIdentifierToken.TokenType != ETokenType.IDENTIFIER)
                throw new ArgumentException(
                    $"The first token of the given instruction \"{instructionStr}\" is not a valid function identifier.");
            
            if(!DefinedFunctions.TryGetValue(funcIdentifierToken.Value, out var func))
                throw new ArgumentException($"Undefined function {funcIdentifierToken.Value}.");

            var beginIndex = 1;
            var parameters = ParseParameters(tokens, ref beginIndex);
            
            var instruction = new Instruction<TContext>(instructionStr, func, parameters, Array.Empty<IValueModifier<TContext>>());
            if(OptimizeInstructionOnBuild) instruction.Optimize(context);
            return instruction;
        }
        
        public Instruction<TContext> BuildInstruction(TContext context, string funcIdentifier, string paramStr, string modifierStr = null)
        {
            if (!DefinedFunctions.TryGetValue(funcIdentifier, out var func))
                throw new ArgumentException($"Undefined function {funcIdentifier}.");
            
            _lexer.Process(paramStr);
            var tokens = _lexer.GetTokens();
            var beginIndex = 0;
            var parameters = ParseParameters(tokens, ref beginIndex);

            _lexer.Process(modifierStr);
            tokens = _lexer.GetTokens();
            var modifiers = ParseValueModifiers(tokens, 0);

            var instruction = new Instruction<TContext>($"{funcIdentifier}: {paramStr}", func, parameters, modifiers);
            if(OptimizeInstructionOnBuild) instruction.Optimize(context);
            return instruction;
        }

        public Instruction<TContext> BuildInstruction(TContext context, string funcIdentifier, string[] parameterStrings)
        {
            if (!DefinedFunctions.TryGetValue(funcIdentifier, out var func))
                throw new ArgumentException($"Undefined function {funcIdentifier}.");

            ASTNode[] parameters = Array.Empty<ASTNode>();
            
            if (parameterStrings != null && parameterStrings.Length > 0)
            {
                var scope = GetExpressionParserScope();
                parameters = new ASTNode[parameterStrings.Length];
                for (int i = 0; i < parameterStrings.Length; i++)
                {
                    _lexer.Process(parameterStrings[i]);
                    var tokens = _lexer.GetTokens();
                    int beginIndex = 0;
                    parameters[i] = ParseExpression(scope, tokens, ref beginIndex);
                }
                ReturnExpressionParserScope(scope);
            }
            
            var instruction = new Instruction<TContext>(funcIdentifier, func, parameters, Array.Empty<IValueModifier<TContext>>());
            if(OptimizeInstructionOnBuild) instruction.Optimize(context);
            return instruction;
        }

        /// <summary>
        /// Parse tokens with expressions as parameters with shunting yard algorithm.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="beginIndex"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private ASTNode[] ParseParameters(IReadOnlyList<Token> tokens, ref int beginIndex)
        {
            if (beginIndex >= tokens.Count) return Array.Empty<ASTNode>();

            var scope = GetExpressionParserScope();
            for (;;)
            {
                var node = ParseExpression(scope, tokens, ref beginIndex);
                if(node == null) break;
                scope.Result.Add(node);
            }
            var result = scope.Result.ToArray();
            ReturnExpressionParserScope(scope);
            return result;
        }

        private IValueModifier<TContext>[] ParseValueModifiers(IReadOnlyList<Token> tokens, int beginIndex)
        {
            var length = tokens.Count;
            if (beginIndex >= length) return Array.Empty<IValueModifier<TContext>>();
            
            _modifiers.Clear();
            var index = beginIndex;
            for (;;)
            {
                // add modifier
                if(index >= length) break;
                var token = tokens[index];
                if (token.TokenType == ETokenType.IDENTIFIER)
                {
                    var modifier = DefinedValueModifiers[token.Value];
                    _modifiers.Add(modifier);
                }
                index++;
                
                // skip comma and check has next
                if(index >= length) break;
                token = tokens[index];
                if(token.TokenType != ETokenType.COMMA) break;
                index++;
            }

            return _modifiers.ToArray();
        }

        private ASTNode ParseExpression(ExpressionParserScope parserScope, IReadOnlyList<Token> tokens, ref int index)
        {
            if (index >= tokens.Count || _closures.Contains(tokens[index].TokenType)) return null;
            parserScope.NodeStack.Clear();
            parserScope.OperatorTokenStack.Clear();
            for (int i = index; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.TokenType == ETokenType.OPEN_PR)
                {
                    parserScope.OperatorTokenStack.Push(token);
                }
                else if (token.TokenType == ETokenType.CLOSE_PR)
                {
                    var topOperator = parserScope.OperatorTokenStack.Pop();
                    while (topOperator.TokenType != ETokenType.OPEN_PR)
                    {
                        parserScope.NodeStack.Push(BuildExpressionNode(parserScope, topOperator));
                        topOperator = parserScope.OperatorTokenStack.Pop();
                    }
                    // TODO: Handle function on top of stack
                }
                else if (token.TokenType == ETokenType.OPEN_BRACE)
                {
                    var objBeginIndex = i + 1;
                    var parameters = ParseParameters(tokens, ref objBeginIndex);
                    
                    string typeIdentifier;
                    if (i > index && tokens[i - 1].TokenType == ETokenType.IDENTIFIER)
                    {
                        typeIdentifier = tokens[i - 1].Value;
                        parserScope.NodeStack.Pop();
                    }
                    else
                        typeIdentifier = string.Empty;
                    parserScope.NodeStack.Push(new ObjectValueAstNode(typeIdentifier, parameters));

                    i = objBeginIndex;
                    if (tokens[i].TokenType != ETokenType.CLOSE_BRACE)
                        throw new Exception("Brace not properly closed.");
                }
                else if (token.TokenType == ETokenType.CLOSE_BRACE)
                {
                    index = i;
                    return TerminateExpression(parserScope);
                }
                else if (token.TokenType == ETokenType.OPEN_BRACKET)
                {
                    var objBeginIndex = i + 1;
                    var parameters = ParseParameters(tokens, ref objBeginIndex);
                    
                    string typeIdentifier;
                    if (i > index && tokens[i - 1].TokenType == ETokenType.IDENTIFIER)
                    {
                        typeIdentifier = tokens[i - 1].Value;
                        parserScope.NodeStack.Pop();
                    }
                    else
                        typeIdentifier = string.Empty;

                    var type = EValueTypeHelper.Parse(typeIdentifier.ToLower()).GetRuntimeType();
                    parserScope.NodeStack.Push(new ListValueAstNode(type, parameters));

                    i = objBeginIndex;
                    if (tokens[i].TokenType != ETokenType.CLOSE_BRACKET)
                        throw new Exception("bracket not properly closed.");
                }
                else if (token.TokenType == ETokenType.CLOSE_BRACKET)
                {
                    index = i;
                    return TerminateExpression(parserScope);
                }
                else if (token.TokenType == ETokenType.COMMA)
                {
                    index = i + 1;
                    return TerminateExpression(parserScope);;
                }
                else if (_opCodes.TryGetValue(token.TokenType, out var operatorInfo))
                {
                    if (_specialUnary.TryGetValue(token.TokenType, out var specialUnaryToken))
                    {
                        if (i == index || 
                            tokens[i - 1].TokenType == ETokenType.OPEN_PR ||
                            _opCodes.ContainsKey(tokens[i - 1].TokenType))
                        {
                            token.TokenType = specialUnaryToken;
                            operatorInfo = _opCodes[specialUnaryToken];
                        }
                    }
                    while (parserScope.OperatorTokenStack.Count > 0)
                    {
                        var topOperator = parserScope.OperatorTokenStack.Peek();
                        if(topOperator.TokenType == ETokenType.OPEN_PR)
                            break;
                    
                        var topOperatorInfo = _opCodes[topOperator.TokenType];
                        if ((operatorInfo.Precedence < topOperatorInfo.Precedence) ||
                            (operatorInfo.Precedence == topOperatorInfo.Precedence &&
                             operatorInfo.LeftAssociative))
                        {
                            parserScope.OperatorTokenStack.Pop();
                            parserScope.NodeStack.Push(BuildExpressionNode(parserScope, topOperator));
                            continue;
                        }
                        break;
                    }

                    parserScope.OperatorTokenStack.Push(token);
                }
                else if (_values.ContainsKey(token.TokenType))
                    parserScope.NodeStack.Push(BuildValueNode(token));
                else
                    throw new Exception($"Unable to process token of type {token.TokenType} at the given position.");
            }

            var paramNode = TerminateExpression(parserScope);
            index = tokens.Count;
            return paramNode;
        }
        
        private ASTNode TerminateExpression(ExpressionParserScope parserScope)
        {
            while (parserScope.OperatorTokenStack.Count > 0)
            {
                var topOperator = parserScope.OperatorTokenStack.Pop();
                parserScope.NodeStack.Push(BuildExpressionNode(parserScope, topOperator));
            }
                    
            var topNode = parserScope.NodeStack.Pop();
            if (parserScope.NodeStack.Count != 0)
                throw new Exception($"node stack is not empty with {parserScope.NodeStack.Count} elements.");
            return topNode;
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
                        return new ObjectValueAstNode();
                    return new VariableAstNode(token.Value);
                }
            }
            throw new ArgumentException($"Invalid token type as value: {token.TokenType}");
        }

        private ASTNode BuildExpressionNode(ExpressionParserScope parserScope, Token operatorToken)
        {
            var operatorInfo = _opCodes[operatorToken.TokenType];
            var operands = new ASTNode[operatorInfo.OperandCount];
            for (int i = operatorInfo.OperandCount - 1; i >= 0; i--)
                operands[i] = parserScope.NodeStack.Pop();
            return new ExpressionAstNode(operatorInfo.OpCode, operands);
        }
        #endregion
        
        #region ExpressionParserScope

        private ExpressionParserScope GetExpressionParserScope()
        {
            var lastIndex = _scopePool.Count - 1;
            if (lastIndex < 0) return new ExpressionParserScope();
            var scope = _scopePool[lastIndex];
            _scopePool.RemoveAt(lastIndex);
            return scope;
        }

        private void ReturnExpressionParserScope(ExpressionParserScope scope)
        {
            scope.Clear();
            _scopePool.Add(scope);
        }
        
        #endregion

        private static class PredefinedFunctions
        {
            public static ValueInfo Print(TContext context, ASTNode[] parameters)
            {
                var param0Str = parameters[0].GetAnyValue(context.Evaluator);
                System.Console.WriteLine(param0Str?.ToString() ?? "null");
                return ValueInfo.Void;
            }

            public static ValueInfo Evaluate(TContext context, ASTNode[] parameters)
            {
                return parameters[0].Evaluate(context.Evaluator);
            }
        }
    }
}