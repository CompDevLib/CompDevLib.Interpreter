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
    public void Test1()
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
        var retA = interpreter.Execute(testContext, "TestFunc 12, 12.0, true, \"This is a string.\"");
        Console.WriteLine($"TestFuncWithRet 12, 12.0, true == {retA.ValueType}");
        var retB = interpreter.Execute<float>(testContext, "TestFuncWithRet 12, 12.0, true");
        Console.WriteLine($"TestFuncWithRet 12, 12.0, true == {retB}");
        var retC = interpreter.Execute<float>(testContext, "TestFuncWithRet 12, 12.0, false");
        Console.WriteLine($"TestFuncWithRet 12, 12.0, false == {retC}");
        //var retC = interpreter.Execute<int>(testContext, "TestFuncWithRet 12, 12, false");
    }
}