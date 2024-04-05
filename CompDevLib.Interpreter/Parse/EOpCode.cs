namespace CompDevLib.Interpreter.Parse
{
    public enum EOpCode
    {
        Eq, Ne, Lt, Gt, Le, Ge, 
        Add, Sub, Mult, Div, Mod, Pow,
        Neg, Pos, Inc, Dec, 
        And, Or, Not, 
        Ternary,
        Member,
    }
}