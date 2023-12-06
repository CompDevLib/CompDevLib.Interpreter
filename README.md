# CompDevLib.Interpreter

A simple interpreter that can evaluate expressions and execute custom instructions.

### Evaluate Expression

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
}

```

### Execute Instruction
`
FuncIdentifier: arg0, arg1
`

each argument is an expression.

```csharp

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

private static float TestFunc2(int param0, float param1, bool param2)
{
	return param2
        ? param0 + param1
        : param0 * param1;
}

[Test]
public void TestInstruction()
{
    var interpreter = new CompInterpreter<BasicContext>();
    var context = new BasicContext();

	interpreter.AddFunctionDefinition("TestFunc1", TestFunc1);
	interpreter.AddFunctionDefinition("TestFunc2", TestFunc2);

	interpreter.Execute<float>(context, "TestFunc1: 12, 12.0, false");
	Assert.That((int)retD == 144);

	interpreter.Execute<float>(context, "TestFunc2: 12, 12.0, false");
	Assert.That((int)retD == 144);
}

```