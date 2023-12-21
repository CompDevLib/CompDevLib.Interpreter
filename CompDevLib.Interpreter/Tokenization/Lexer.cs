using System;
using System.Collections.Generic;
using System.Text;

namespace CompDevLib.Interpreter.Tokenization
{
	/// <summary>
	/// A simple lexer that can tokenizes a given string.
	/// </summary>
	public class Lexer
	{
		private StringBuilder _stringBuilder;
		private List<Token> _tokens;

		public Lexer()
		{
			_stringBuilder = new StringBuilder();
			_tokens = new List<Token>();
		}

		public IReadOnlyList<Token> GetTokens()
		{
			return _tokens;
		}

		/// <summary>
		/// Process and tokenize a string. The result can be obtained by <see cref="GetTokens"/>
		/// </summary>
		/// <param name="str">The string to be tokenized.</param>
		public unsafe void Process(string str)
		{
			_tokens.Clear();
			var length = str.Length;

			fixed(char *pData = str)
			{
				var pCurrData = pData;
				var pEndData = pData + length - 1;
				while(pCurrData <= pEndData)
				{
					if (char.IsWhiteSpace(*pCurrData)) {
						pCurrData++;
						continue;
					};

					pCurrData += Process(pCurrData, pEndData - pCurrData + 1);
				}
			}
		}

		private unsafe int Process(char* pData, long length)
		{
			var chr = pData[0];
			switch (chr)
			{
				case '+':
					_tokens.Add(new Token { TokenType = ETokenType.ADD });
					return 1;
				case '-':
					if(length > 1 && pData[1] == '>')
					{
						_tokens.Add(new Token { TokenType = ETokenType.PTR_MEMBER });
						return 2;
					}
                    _tokens.Add(new Token { TokenType = ETokenType.SUB });
                    return 1;
				case '*':
                    _tokens.Add(new Token { TokenType = ETokenType.MULT });
                    return 1;
				case '/':
                    _tokens.Add(new Token { TokenType = ETokenType.DIV });
                    return 1;
				case '%':
                    _tokens.Add(new Token { TokenType = ETokenType.MOD });
                    return 1;
				case '^':
					_tokens.Add(new Token { TokenType = ETokenType.POW });
					return 1;
				case '|':
					if(length > 1 && pData[1] == '|')
					{
						_tokens.Add(new Token { TokenType = ETokenType.OR });
						return 2;
					}
					_tokens.Add(new Token { TokenType = ETokenType.B_OR });
					return 1;
				case '&':
                    if (length > 1 && pData[1] == '&')
                    {
                        _tokens.Add(new Token { TokenType = ETokenType.AND });
                        return 2;
                    }
                    _tokens.Add(new Token { TokenType = ETokenType.B_AND });
                    return 1;
                case '>':
                    if (length > 1 && pData[1] == '=')
                    {
                        _tokens.Add(new Token { TokenType = ETokenType.GE });
                        return 2;
                    }
                    _tokens.Add(new Token { TokenType = ETokenType.GT });
                    return 1;
                case '<':
                    if (length > 1 && pData[1] == '=')
                    {
                        _tokens.Add(new Token { TokenType = ETokenType.LE });
                        return 2;
                    }
                    _tokens.Add(new Token { TokenType = ETokenType.LT });
                    return 1;
                case '=':
                    if (length > 1 && pData[1] == '=')
                    {
                        _tokens.Add(new Token { TokenType = ETokenType.EQ });
                        return 2;
                    }
                    _tokens.Add(new Token { TokenType = ETokenType.ASSIGN });
                    return 1;
                case '!':
	                if (length > 1 && pData[1] == '=')
	                {
		                _tokens.Add(new Token {TokenType = ETokenType.NE });
		                return 2;
	                }
	                _tokens.Add(new Token {TokenType = ETokenType.NOT });
	                return 1;
				case '.':
					_tokens.Add(new Token { TokenType = ETokenType.TYPE_MEMBER });
					return 1;
				case ',':
					_tokens.Add(new Token { TokenType = ETokenType.COMMA });
					return 1;
				case ':':
					_tokens.Add(new Token { TokenType = ETokenType.COLON });
					return 1;
				case '?':
					_tokens.Add(new Token {TokenType = ETokenType.QUESTION_MARK});
					return 1;
				case '(':
					_tokens.Add(new Token { TokenType = ETokenType.OPEN_PR });
					return 1;
				case ')':
					_tokens.Add(new Token { TokenType = ETokenType.CLOSE_PR });
					return 1;
			}
			if (chr == '\'' || chr == '"') return ReadString(pData, length);
			return ReadWord(pData, length);
		}

		private unsafe int ReadString(char* pData, long length)
		{
			_stringBuilder.Clear();
			var quotationMark = pData[0];
			var pCurrData = pData;
			var index = 1;
			while(index < length)
			{
				// skip
				if (pData[index] == '\\')
				{
					if(index == length - 1)
						throw new Exception("Invalid \\ at the end of string.");

					_stringBuilder.Append(pData[index + 1]);
					index += 2;
					continue;
				}

				if (pData[index] == quotationMark)
				{
					index++;
					break;
				}

				_stringBuilder.Append(pData[index]);
				index++;
			}

            _tokens.Add(new Token
			{
				TokenType = ETokenType.STR,
				Value = _stringBuilder.ToString(),
			});

            return index;
		}

		/// <summary>
		/// A number or an identifier is considered a word.
		/// </summary>
		/// <param name="pData"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private unsafe int ReadWord(char* pData, long length)
		{
			_stringBuilder.Clear();

			var chr = *pData;
			var index = 1;
			if (char.IsDigit(chr))
			{
                bool isFraction = false;
                _stringBuilder.Append(chr);
				while(index < length)
				{
					chr = *(pData + index);
					if(chr == '.')
					{
						if (isFraction)
							throw new Exception("A second '.' is found in a number.");
						isFraction = true;
					}
					else if (!char.IsDigit(chr))
					{
						break;
					}
					_stringBuilder.Append(chr);
					index++;
				}
                _tokens.Add(new Token
                {
                    TokenType = isFraction ? ETokenType.FLOAT : ETokenType.INT,
                    Value = _stringBuilder.ToString(),
                });
            }
            else if(char.IsLetter(chr) || chr == '_')
			{
				_stringBuilder.Append(chr);
                while (index < length)
                {
                    chr = *(pData + index);
                    if (!char.IsLetterOrDigit(chr) && chr != '_')
                    {
						break;
                    }
					_stringBuilder.Append(chr);
					index++;
                }

                var str = _stringBuilder.ToString();
                // TODO: add keyword detection
                if(str == "true" || str == "false")
	                _tokens.Add(new Token
	                {
		                TokenType = ETokenType.BOOL,
		                Value = str,
	                });
                else
	                _tokens.Add(new Token
	                {
	                    TokenType = ETokenType.IDENTIFIER,
	                    Value = str,
	                });
            }
			else
			{
				throw new Exception($"Unrecognized word starting with '{chr}'.");
			}

            return index;
        }
    }
}