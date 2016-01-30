using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Compiler.Models
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
        public TokenGroup Group { get; set; }

        private Token()
        {
        }

        public static Token CreateToken(string lexeme)
        {
            TokenType type = TokenType.Unknown;
            var intRegex = @"[+|-]?[\d]+";
            var realRegex = @"[+|-]?[\d]*(.)[\d]+";
            var identifierRegex = @"[a-zA-Z][\w]{0, 30}";
            var booleanRegex = @"(true|false)";
            var stringRegex = @"""[.]*""";

            if (KnownTokenTypes.ContainsKey(lexeme))
            {
                type = KnownTokenTypes[lexeme];
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

        private static Dictionary<string, TokenType> KnownTokenTypes = new Dictionary<string, TokenType>
        {
            { "!", TokenType.BooleanNot },
            { "!=", TokenType.NotEqual },
            { "%", TokenType.Modulo },
            { "%=", TokenType.ModuloEqual },
            { "&", TokenType.LogicalAnd },
            { "&&", TokenType.BooleanAnd },
            { "&=", TokenType.LogicalAndEqual },
            { "(", TokenType.OpenParen },
            { ")", TokenType.CloseParen },
            { "*", TokenType.Multiplication },
            { "*=", TokenType.MultiplicationEqual },
            { "+", TokenType.Plus },
            { "++", TokenType.PlusPlus },
            { "+=", TokenType.PlusEqual },
            { ",", TokenType.Comma },
            { "-", TokenType.Minus },
            { "--", TokenType.MinusMinus },
            { "-=", TokenType.MinusEqual },
            { ".", TokenType.Dot },
            { "/", TokenType.Divide },
            { "/=", TokenType.DivideEqual },
            { ":", TokenType.Colon },
            { ";", TokenType.Semicolon },
            { "<", TokenType.LessThan },
            { "<<", TokenType.BitwiseLeftShift },
            { "<<=", TokenType.BitwiseLeftShiftEqual },
            { "<=", TokenType.LessThanOrEqual },
            { "=", TokenType.Assignment },
            { "==", TokenType.BooleanEqual },
            { ">", TokenType.GreaterThan },
            { ">=", TokenType.GreaterThanOrEqual },
            { ">>", TokenType.BitwiseRightShift },
            { ">>=", TokenType.BitwiseRightShiftEqual },
            { ">>>", TokenType.UnsignedLeftShift },
            { ">>>=", TokenType.UnsignedLeftShiftEqual },
            { "?", TokenType.QuestionMark },
            { "[", TokenType.OpenSquareBracket },
            { "\"", TokenType.DoubleQuote },
            { "]", TokenType.CloseSquareBracket },
            { "^", TokenType.LogicalExclusiveOr },
            { "^=", TokenType.LogicalExclusiveOrEqual },
            { "{", TokenType.OpenCurlyBrace },
            { "|", TokenType.LogicalOr },
            { "|=", TokenType.LogicalOrEqual },
            { "||", TokenType.BooleanOr },
            { "}", TokenType.CloseCurlyBrace },
            { "~", TokenType.LogicalNot },
            { "abstract", TokenType.Abstract },
            { "assert", TokenType.Assert },
            { "boolean", TokenType.Boolean },
            { "break", TokenType.Break },
            { "byte", TokenType.Byte },
            { "case", TokenType.Case },
            { "catch", TokenType.Catch },
            { "char", TokenType.Char },
            { "class", TokenType.Class },
            { "const", TokenType.Const },
            { "continue", TokenType.Continue },
            { "default", TokenType.Default },
            { "do", TokenType.Do },
            { "double", TokenType.Double },
            { "else", TokenType.Else },
            { "enum", TokenType.Enum },
            { "extends", TokenType.Extends },
            { "false", TokenType.False },
            { "final", TokenType.Final },
            { "finally", TokenType.Finally },
            { "float", TokenType.Float },
            { "for", TokenType.For },
            { "goto", TokenType.Goto },
            { "if", TokenType.If },
            { "implements", TokenType.Implements },
            { "import", TokenType.Import },
            { "instanceof", TokenType.Instanceof },
            { "int", TokenType.Int },
            { "interface", TokenType.Interface },
            { "length", TokenType.Length },
            { "long", TokenType.Long },
            { "main", TokenType.Main },
            { "native", TokenType.Native },
            { "new", TokenType.New },
            { "package", TokenType.Package },
            { "private", TokenType.Private },
            { "protected", TokenType.Protected },
            { "public", TokenType.Public },
            { "return", TokenType.Return },
            { "short", TokenType.Short },
            { "static", TokenType.Static },
            { "strictfp", TokenType.Strictfp },
            { "super", TokenType.Super },
            { "switch", TokenType.Switch },
            { "synchronized", TokenType.Synchronized },
            { "this", TokenType.This },
            { "throw", TokenType.Throw },
            { "throws", TokenType.Throws },
            { "transient", TokenType.Transient },
            { "true", TokenType.True },
            { "try", TokenType.Try },
            { "void", TokenType.Void },
            { "volatile", TokenType.Volatile },
            { "while", TokenType.While },
            { "String", TokenType.String },
            { "System.out.println", TokenType.SystemOutPrintln },
        };
    }
}