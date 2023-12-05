namespace CompDevLib.Interpreter.Tokenization
{
    public enum ETokenType
    {
        #region Logical

        AND,
        OR,
        NOT,

        #endregion

        #region Comparison

        EQ,
        NE,
        LT,
        GT,
        LE,
        GE,

        #endregion

        #region Arithmetic

        ADD,
        SUB,
        MULT,
        DIV,
        MOD,

        #endregion

        #region Bitwise

        B_OR,
        B_AND,

        #endregion

        #region

        INT,
        BOOL,
        FLOAT,
        STR,
        IDENTIFIER,

        #endregion

        ASSIGN,
        TYPE_MEMBER,
        PTR_MEMBER,
        COMMA,
        OPEN_PR,
        CLOSE_PR,
    }
}