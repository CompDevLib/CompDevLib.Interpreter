# CompDevLib.Interpreter

A simple interpreter that can evaluate expressions and execute custom instructions.

This interpreter is intended for embedding in other application's sub-systems like commandline tools, game console, visual scripting, etc.
It is extremely light weight, doesn't not have much predefined functions but is easily expandable.

### Evaluate Expression

Example:
```csharp

[Test]
public void TestExpression()
{
    var interpreter = new CompInterpreter<BasicContext>();
    var context = new BasicContext();
    
    var expressionA = "(1 + 2 + 3 + 2 ^ 2) / 2 * 2 ^ 2 % 100";
    int resultA = interpreter.EvaluateExpression<int>(context, expressionA);
    Assert.That(resultA == 20);
    
    var expressionB = "((1 + 2 + 3 + 2 ^ 2) / 2) ^ 2 ^ 2 % 100";
    int resultB = interpreter.EvaluateExpression<int>(context, expressionB);
    Assert.That(resultB == 25);
    
    var expressionC = $"{expressionA} + 5 == {expressionB}";
    bool resultC = interpreter.EvaluateExpression<bool>(context, expressionC);
    Assert.That(resultC);
}

```

### Execute Instruction
`
FuncIdentifier: arg0, arg1
`

each argument is an expression.

Example:
```csharp

[Test]
public void TestInstruction()
{
    var interpreter = new CompInterpreter<BasicContext>();
    var context = new BasicContext();

    // standard functions should be defined in a specific format
	interpreter.AddFunctionDefinition("TestFunc1", TestFunc1);
	interpreter.Execute<float>(context, "TestFunc1: 12, 12.0, false");
	Assert.That((int)retD == 144);

    // you can define converted function with any return value or parameters, as long as the data type is supported.
	interpreter.AddFunctionDefinition("TestFunc2", TestFunc2);
	interpreter.Execute<float>(context, "TestFunc2: 12, 12.0, false");
	Assert.That((int)retD == 144);
	
        
    var instruction = interpreter.BuildInstruction(instructionD); // build instruction to an object.
    instruction.Optimize(context); // optimize the instruction by computing expressions that will result in constant values first

    // you can build the instruction and store it somewhere first, and execute when needed, which is a lot faster.
    instruction.Execute(context);
}

/// <summary>
/// Standard function definition
/// </summary>
private static ValueInfo TestFunc1(BasicContext context, ASTNode[] parameters)
{
	// get parameters
    var param0 = parameters[0].GetIntValue(ctx.Environment);
    var param1 = parameters[1].GetFloatValue(ctx.Environment);
    var param2 = parameters[2].GetBoolValue(ctx.Environment);

    // calculation
	var result = param2
        ? param0 + param1
        : param0 * param1;

    // push return value
    return ctx.Environment.PushEvaluationResult(result);
}

/// <summary>
/// Converted function definition
/// </summary>
private static float TestFunc2(int param0, float param1, bool param2)
{
	return param2
        ? param0 + param1
        : param0 * param1;
}

```