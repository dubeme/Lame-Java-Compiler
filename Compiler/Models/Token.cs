using Compiler.Helpers;
using Compiler.Models.Attributes;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Compiler.Models
{
    /// <summary>
    ///
    /// </summary>
    public class Token
    {
        private static Token _Empty;

        public static Token PRINT_HEADER
        {
            get
            {
                if (_Empty == null)
                {
                    lock (new object())
                    {
                        if (_Empty == null)
                        {
                            _Empty = new Token();
                        }
                    }
                }

                return _Empty;
            }
        }

        public static readonly Token UNARY_MINUS = new Token
        {
            Lexeme = "unary_minus",
            LineNumber = -1
        };

        /// <summary>
        /// Gets or sets the type of this Token.
        /// </summary>
        public TokenType Type { get; set; }

        /// <summary>
        /// Gets or sets the lexeme of this Token.
        /// </summary>
        public string Lexeme { get; set; }

        /// <summary>
        /// Gets or sets the group of this Token.
        /// </summary>
        public TokenGroup Group
        {
            get
            {
                var attr = AttributeHelper.GetAttribute<TokenTypeMetadataAttribute, TokenType>(this.Type);
                return attr.BaseTokenGroup;
            }
        }

        /// <summary>
        /// Gets the line numbe of this Tokenr.
        /// </summary>
        public int LineNumber { get; set; }

        public bool HasError { get; set; }
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// The known token types
        /// </summary>
        private static KnownTokenTypes _KnownTokenTypes = KnownTokenTypes.Instance;

        /// <summary>
        /// Prevents a default instance of the <see cref="Token"/> class from being created.
        /// </summary>
        private Token()
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            const int LNW = 10;
            const int LXW = 32;
            const int TKW = 24;
            const int VW = 16;
            const int VRW = 10;
            const int WIDTH = LNW + LXW + TKW + VW + VRW;

            var formatString = $"{{0,{LNW}}} {{1,-{LXW}}} {{2,-{TKW}}} {{3,-{VW}}} {{4,-{VRW}}}";

            if (this == PRINT_HEADER)
            {
                var sb = new StringBuilder(string.Format(formatString, "Line #", "Lexeme", "Token", "Value", "Valuer"));
                sb.AppendLine($"\n{new string('-', WIDTH)}");

                return sb.ToString();
            }
            else
            {
                var value = "";
                var valueR = "";

                if (this.HasError)
                {
                    return $"{this.LineNumber,LNW} {this.ErrorMessage,-(WIDTH - LNW)}";
                }

                if (this.Type == TokenType.LiteralInteger)
                {
                    value = this.Lexeme;
                }
                else if (this.Type == TokenType.LiteralReal)
                {
                    valueR = this.Lexeme;
                }

                return string.Format(formatString, this.LineNumber, Contain(this.Lexeme, 30), this.Type, value, valueR);
            }
        }

        private static string Contain(string str, int max)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            if (str.Length <= max || max < 5)
            {
                return str;
            }

            var join = "...";
            var len = max % 2 == 0 ? max / 2 : max / 2 + 1;

            return $"{str.Substring(0, len - 2)}{join}{str.Substring(str.Length - len + 1)}";
        }

        public static Token CreateErrorToken(int lineNumber, string errMessage)
        {
            return new Token
            {
                HasError = true,
                ErrorMessage = errMessage,
                LineNumber = lineNumber
            };
        }

        public static Token CreateEOFToken(int lineNumber)
        {
            return new Token
            {
                LineNumber = lineNumber,
                Type = TokenType.EndOfFile
            };
        }

        /// <summary>
        /// Creates a token.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>A <see cref="Compiler.Models.Token" /> object</returns>
        public static Token CreateToken(string lexeme, int lineNumber)
        {
            TokenType type = TokenType.Unknown;
            var intRegex = @"^[+|-]?[\d]+$";
            var realRegex = @"^[+|-]?[\d]*(.)[\d]+$";
            var identifierRegex = @"^[a-zA-Z][\w]{0,}$";
            var booleanRegex = @"^(true|false)$";
            var stringRegex = @"^""(.)*""$";

            if (_KnownTokenTypes[lexeme] != TokenType.Unknown)
            {
                type = _KnownTokenTypes[lexeme];
            }
            else if (Regex.IsMatch(lexeme, intRegex))
            {
                type = TokenType.LiteralInteger;
            }
            else if (Regex.IsMatch(lexeme, realRegex))
            {
                type = TokenType.LiteralReal;
            }
            else if (Regex.IsMatch(lexeme, identifierRegex))
            {
                int MAX_LENGTH = 31;
                if (lexeme.Length > MAX_LENGTH)
                {
                    throw new Exception($"Identifier {{{lexeme}}}, excedds max length of {MAX_LENGTH}");
                }

                type = TokenType.Identifier;
            }
            else if (Regex.IsMatch(lexeme, booleanRegex))
            {
                type = TokenType.LiteralBoolean;
            }
            else if (Regex.IsMatch(lexeme, stringRegex))
            {
                type = TokenType.LiteralString;
            }

            return new Token
            {
                Type = type,
                Lexeme = lexeme,
                LineNumber = lineNumber
            };
        }
    }
}