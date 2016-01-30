using System.Collections.Generic;
using System.Linq;

namespace Compiler.Models
{
    /// <summary>
    ///
    /// </summary>
    public class KnownTokenTypes
    {
        private static KnownTokenTypes _instance;

        public static KnownTokenTypes Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                        {
                            _instance = new KnownTokenTypes();
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// The token type string value
        /// </summary>
        private static Dictionary<TokenType, string> TokenTypeStringValue;

        /// <summary>
        /// Gets the <see cref="System.String"/>(Lexeme) with the specified token type.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <returns></returns>
        public string this[TokenType tokenType]
        {
            get
            {
                if (TokenTypeStringValue.ContainsKey(tokenType))
                {
                    return TokenTypeStringValue[tokenType];
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the <see cref="TokenType"/> with the specified lexeme.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <returns></returns>
        public TokenType this[string lexeme]
        {
            get
            {
                if (_KnownTokenTypes.ContainsKey(lexeme))
                {
                    return _KnownTokenTypes[lexeme];
                }

                return TokenType.Unknown;
            }
        }

        /// <summary>
        /// Initializes the <see cref="KnownTokenTypes"/> class.
        /// </summary>
        static KnownTokenTypes()
        {
            TokenTypeStringValue = _KnownTokenTypes.ToDictionary(curr => curr.Value, curr => curr.Key);
        }

        /// <summary>
        /// The known token types
        /// </summary>
        private static Dictionary<string, TokenType> _KnownTokenTypes = new Dictionary<string, TokenType>
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