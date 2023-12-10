// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using CompDevLib.Interpreter;
using CompDevLib.Interpreter.Parse;

TestInstruction2();

void TestInstruction2()
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
        
    var interpreter = new CompInterpreter<BasicContext>(false);
    var context = new BasicContext();

    interpreter.AddFunctionDefinition("TestFunc1", TestFunc1);
    interpreter.AddFunctionDefinition("TestFunc2", TestFunc2);

    var stopwatch = Stopwatch.StartNew();
    const int executionCount = 1000000;
    
    for (int i = 0; i < executionCount; i++)
    {
        var retA = interpreter.Execute<float>(context, "TestFunc1: (2 * 3 + 3) * 2 / 3 + (1 + 2) * 2, 2 ^ 3 + 4.0, false");
    }
    Console.WriteLine($"Standard function call time: {stopwatch.ElapsedMilliseconds}\n");

    stopwatch.Restart();
    for (int i = 0; i < executionCount; i++)
    {
        var retB = interpreter.Execute<float>(context, "TestFunc2: (2 * 3 + 3) * 2 / 3 + (1 + 2) * 2, 2 ^ 3 + 4.0, false");
    }
    stopwatch.Stop();
    Console.WriteLine($"Converted function call time: {stopwatch.ElapsedMilliseconds}");
}