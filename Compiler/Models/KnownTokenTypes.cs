using Compiler.Helpers;
using Compiler.Models.Attributes;
using System;
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
        /// The known token types
        /// </summary>
        private static Dictionary<string, TokenType> _KnownTokenTypes;

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
            var validateTokenType =  new Func<TokenType, KeyValuePair<string, TokenType>>((tokenType) =>
            {
                var attr = AttributeHelper.GetAttribute<TokenTypeMetadataAttribute, TokenType>(tokenType);

                if (string.IsNullOrWhiteSpace(attr.Lexeme))
                {
                    return default(KeyValuePair<string, TokenType>);
                }

                return  new KeyValuePair<string, TokenType>(attr.Lexeme, tokenType);
            });

            var tokenTypesWithValidLexemes = Enum.GetValues(typeof(TokenType))
                .Cast<TokenType>()
                .Select(validateTokenType)
                .Where(v => !v.Equals(default(KeyValuePair<string, TokenType>)));

            TokenTypeStringValue = tokenTypesWithValidLexemes.ToDictionary(curr => curr.Value, curr => curr.Key);
            _KnownTokenTypes = tokenTypesWithValidLexemes.ToDictionary(curr => curr.Key, curr => curr.Value);
        }
   }
}