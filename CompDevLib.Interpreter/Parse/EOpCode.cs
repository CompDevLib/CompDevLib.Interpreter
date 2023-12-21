namespace CompDevLib.Interpreter.Parse
{
    public enum EOpCode
    {
        Eq, Ne, Lt, Gt, Le, Ge, 
        Add, Sub, Mult, Div, Mod, Pow,
        Neg, Inc, Dec, 
        And, Or, Not, 
        // TODO: Add support for conditional operator
        Member,
    }
}