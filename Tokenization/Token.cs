namespace CompDevLib.Interpreter.Tokenization
{
    public struct Token
    {
        public ETokenType TokenType;
        public string Value;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Value)) return $"TokenType={TokenType}";
            return $"TokenType={TokenType}, Value={Value}";
        }
    }
}