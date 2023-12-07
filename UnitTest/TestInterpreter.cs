using System.Diagnostics;
using CompDevLib.Interpreter;
using CompDevLib.Interpreter.Parse;

namespace UnitTest;

public class TestInterpreter
{
    [SetUp]
    public void Setup()
    {
    }

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
    
    [Test]
    public void TestInstruction()
    {
        var interpreter = new CompInterpreter<BasicContext>();
        var context = new BasicContext();
        interpreter.AddFunctionDefinition("TestFunc", (ctx, parameters) =>
        {
            Console.WriteLine("TestFunc is called.");
            Console.WriteLine($"Testing first param (int): {parameters[0].GetIntValue(ctx.Environment)}");
            Console.WriteLine($"Testing first param (float): {parameters[1].GetFloatValue(ctx.Environment)}");
            Console.WriteLine($"Testing first param (bool): {parameters[2].GetBoolValue(ctx.Environment)}");
            Console.WriteLine($"Testing first param (string): {parameters[3].GetStringValue(ctx.Environment)}");
            return ValueInfo.Void;
        });
        interpreter.AddFunctionDefinition("TestFuncWithRet", (ctx, parameters) =>
        {
            Console.WriteLine("TestFuncWithRet is called.");
            var param0 = parameters[0].GetIntValue(ctx.Environment);
            var param1 = parameters[1].GetFloatValue(ctx.Environment);
            var param2 = parameters[2].GetBoolValue(ctx.Environment);
            Console.WriteLine($"Testing first param (int): {param0}");
            Console.WriteLine($"Testing first param (float): {param1}");
            Console.WriteLine($"Testing first param (bool): {param2}");
            return param2
                ? ctx.Environment.PushEvaluationResult(param0 + param1)
                : ctx.Environment.PushEvaluationResult(param0 * param1);
        });
        var expressionA = "TestFunc: 12, 12.0, true, \"This is a string.\"";
        var retA = interpreter.Execute(context, expressionA);
        Console.WriteLine($"{expressionA} == {retA.ValueType}\n");
        Assert.That(retA.ValueType == EValueType.Void);

        var expressionB = "TestFuncWithRet: 12, 12.0, true";
        var retB = interpreter.Execute<float>(context, expressionB);
        Console.WriteLine($"{expressionB} == {retB}\n");
        Assert.That((int)retB == 24);

        var expressionC = "TestFuncWithRet: (2 + 1) * 2 ^ 2, 12.0, false";
        var retC = interpreter.Execute<float>(context, expressionC);
        Console.WriteLine($"{expressionC} == {retC}\n");
        Assert.That((int)retC == 144);
        
        var expressionD = "TestFuncWithRet: (2 * 3 + 3) * 2 / 3 + (1 + 2) * 2, 2 ^ 3 + 4.0, false";
        var retD = interpreter.Execute<float>(context, expressionD);
        Console.WriteLine($"{expressionD} == {retD}\n");
        Assert.That((int)retD == 144);
    }

    [Test]
    public void TestInstruction2()
    {
        static ValueInfo TestFunc1(BasicContext context, ASTNode[] parameters)
        {
            // get parameters
            var param0 = parameters[0].GetIntValue(context.Environment);
            var param1 = parameters[1].GetFloatValue(context.Environment);
            var param2 = parameters[2].GetBoolValue(context.Environment);

            // calculation
            var result = param2
                ? param0 + param1
                : param0 * param1;

            // push return value
            return context.Environment.PushEvaluationResult(result);
        }

        static float TestFunc2(int param0, float param1, bool param2)
        {
            return param2
                ? param0 + param1
                : param0 * param1;
        }
        
        var interpreter = new CompInterpreter<BasicContext>();
        var context = new BasicContext();

        interpreter.AddFunctionDefinition("TestFunc1", TestFunc1);
        interpreter.AddFunctionDefinition("TestFunc2", TestFunc2);

        var stopwatch = Stopwatch.StartNew();
        const int executionCount = 1000000;
        for (int i = 0; i < executionCount; i++)
        {
            var retA = interpreter.Execute<float>(context, "TestFunc1: 12, 12.0, false");
            Assert.That((int) retA == 144);
        }
        Console.WriteLine($"Standard function call time: {stopwatch.ElapsedMilliseconds}\n");

        stopwatch.Restart();
        for (int i = 0; i < executionCount; i++)
        {
            var retB = interpreter.Execute<float>(context, "TestFunc2: 12, 12.0, false");
            Assert.That((int) retB == 144);
        }
        stopwatch.Stop();
        Console.WriteLine($"Converted function call time: {stopwatch.ElapsedMilliseconds}");
    }

    [Test]
    public void TestOptimization()
    {
        static float TestFunc(int param0, float param1, bool param2)
        {
            return param2
                ? param0 + param1
                : param0 * param1;
        }
        var interpreter = new CompInterpreter<BasicContext>();
        var context = new BasicContext();
        const string instructionStr = "TestFunc: (2 * 3 + 3) * 2 / 3 + (1 + 2) * 2, 2 ^ 3 + 4.0, false";

        interpreter.AddFunctionDefinition("TestFunc", TestFunc);
        var instruction = interpreter.BuildInstruction(instructionStr);
        
        var stopwatch = Stopwatch.StartNew();
        const int executionCount = 1000000;
        for (int i = 0; i < executionCount; i++)
        {
            var retA = interpreter.Execute<float>(context, instructionStr);
            Assert.That((int) retA == 144);
        }
        Console.WriteLine($"Execute instruction string directly time cost: {stopwatch.ElapsedMilliseconds}\n");

        stopwatch.Restart();
        for (int i = 0; i < executionCount; i++)
        {
            var retB = interpreter.Execute<float>(context, instructionStr, instruction);
            Assert.That((int) retB == 144);
        }
        stopwatch.Stop();
        Console.WriteLine($"Execute pre-built instruction time cost: {stopwatch.ElapsedMilliseconds}\n");
        
        stopwatch.Restart();
        instruction.Optimize(context);
        for (int i = 0; i < executionCount; i++)
        {
            var retB = interpreter.Execute<float>(context, instructionStr, instruction);
            Assert.That((int) retB == 144);
        }
        stopwatch.Stop();
        Console.WriteLine($"Execute pre-built and optimized instruction time cost: {stopwatch.ElapsedMilliseconds}\n");
    }
}