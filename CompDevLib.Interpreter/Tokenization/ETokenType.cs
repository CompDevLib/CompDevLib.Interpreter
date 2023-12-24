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
        POW,

        #endregion

        #region Bitwise

        B_OR,
        B_AND,

        #endregion

        #region Value

        INT,
        BOOL,
        FLOAT,
        STR,
        IDENTIFIER,

        #endregion

        ASSIGN,
        TYPE_MEMBER,
        PTR_MEMBER,
        QUESTION_MARK,
        COMMA,
        COLON,
        OPEN_PR,
        CLOSE_PR,
        OPEN_BRACKET,
        CLOSE_BRACKET,
        OPEN_BRACE,
        CLOSE_BRACE,
    }
}