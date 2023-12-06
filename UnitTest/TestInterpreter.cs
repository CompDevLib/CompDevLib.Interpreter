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
        var interpreter = new CompInterpreter<Context>();
        var context = new Context();
        var expressionA = "(1 + 2 + 3 + 2 ^ 2) / 2 * 2 ^ 2 % 100";
        int resultA = interpreter.EvaluateExpression<int>(context, expressionA);
        Console.WriteLine($"{expressionA} == {resultA}");
        
        var expressionB = "((1 + 2 + 3 + 2 ^ 2) / 2) ^ 2 ^ 2 % 100";
        int resultB = interpreter.EvaluateExpression<int>(context, expressionB);
        Console.WriteLine($"{expressionB} == {resultB}");
    }
    
    [Test]
    public void TestInstruction()
    {
        var interpreter = new CompInterpreter<Context>();
        var testContext = new Context();
        interpreter.AddFunctionDefinition("TestFunc", (context, parameters) =>
        {
            Console.WriteLine("TestFunc is called.");
            Console.WriteLine($"Testing first param (int): {parameters[0].GetIntValue(context.Environment)}");
            Console.WriteLine($"Testing first param (float): {parameters[1].GetFloatValue(context.Environment)}");
            Console.WriteLine($"Testing first param (bool): {parameters[2].GetBoolValue(context.Environment)}");
            Console.WriteLine($"Testing first param (string): {parameters[3].GetStringValue(context.Environment)}");
            return ValueInfo.Void;
        });
        interpreter.AddFunctionDefinition("TestFuncWithRet", (context, parameters) =>
        {
            Console.WriteLine("TestFuncWithRet is called.");
            var param0 = parameters[0].GetIntValue(context.Environment);
            var param1 = parameters[1].GetFloatValue(context.Environment);
            var param2 = parameters[2].GetBoolValue(context.Environment);
            Console.WriteLine($"Testing first param (int): {param0}");
            Console.WriteLine($"Testing first param (float): {param1}");
            Console.WriteLine($"Testing first param (bool): {param2}");
            if (param2)
                return context.Environment.AppendEvaluationResult(param0 + param1);
            else
                return context.Environment.AppendEvaluationResult(param0 * param1);
        });
        var expressionA = "TestFunc: 12, 12.0, true, \"This is a string.\"";
        var retA = interpreter.Execute(testContext, expressionA);
        Console.WriteLine($"{expressionA} == {retA.ValueType}\n");

        var expressionB = "TestFuncWithRet: 12, 12.0, true";
        var retB = interpreter.Execute<float>(testContext, expressionB);
        Console.WriteLine($"{expressionB} == {retB}\n");

        var expressionC = "TestFuncWithRet: (2 + 1) * 2 ^ 2, 12.0, false";
        var retC = interpreter.Execute<float>(testContext, expressionC);
        Console.WriteLine($"{expressionC} == {retC}\n");
        
        var expressionD = "TestFuncWithRet: (2 * 3 + 3) * 2 / 3 + (1 + 2) * 2, 2 ^ 3 + 4.0, false";
        var retD = interpreter.Execute<float>(testContext, expressionD);
        Console.WriteLine($"{expressionD} == {retD}\n");
    }
}