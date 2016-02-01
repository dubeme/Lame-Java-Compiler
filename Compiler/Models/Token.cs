using System.Text.RegularExpressions;

namespace Compiler.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Token
    {
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
        public TokenGroup Group { get; set; }
        /// <summary>
        /// Gets the line numbe of this Tokenr.
        /// </summary>
        public int LineNumber { get; set; }

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
            return $"{this.LineNumber, -10} {this.Lexeme,-20} {this.Type}";
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
            var intRegex = @"[+|-]?[\d]+";
            var realRegex = @"[+|-]?[\d]*(.)[\d]+";
            var identifierRegex = @"[a-zA-Z][\w]{0,30}";
            var booleanRegex = @"(true|false)";
            var stringRegex = @"""[.]*""";

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