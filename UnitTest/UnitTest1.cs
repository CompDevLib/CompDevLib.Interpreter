using CompDevLib.Interpreter;
using CompDevLib.Interpreter.Parse;

namespace UnitTest;

public class Tests
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
            Console.WriteLine($"Testing first param (int): {parameters[0].GetIntValue(context.ASTContext)}");
            Console.WriteLine($"Testing first param (float): {parameters[1].GetFloatValue(context.ASTContext)}");
            Console.WriteLine($"Testing first param (bool): {parameters[2].GetBoolValue(context.ASTContext)}");
            Console.WriteLine($"Testing first param (string): {parameters[3].GetStringValue(context.ASTContext)}");
            return EValueType.Void;
        });
        interpreter.Execute(testContext, "TestFunc 12, 11.0, true, \"This is a string\"");
        Assert.Pass();
    }
}