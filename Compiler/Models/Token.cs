using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compiler.Models
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public TokenGroup Group { get; set; }

        private static KnownTokenTypes _KnownTokenTypes = KnownTokenTypes.Instance;


        private Token()
        {
        }

        public override string ToString()
        {
            return $"{this.Lexeme:20} {this.Type}";
        }

        public static Token CreateToken(string lexeme)
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
                Lexeme = lexeme
            };
        }
    }
}