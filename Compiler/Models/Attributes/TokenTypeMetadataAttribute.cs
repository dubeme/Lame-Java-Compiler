using System;

namespace Compiler.Models.Attributes
{
    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class TokenTypeMetadataAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenTypeMetadataAttribute"/> class.
        /// </summary>
        public TokenTypeMetadataAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the lexeme of this TokenTypeMetadataAttribute.
        /// </summary>
        public string Lexeme { get; set; }

        /// <summary>
        /// Gets or sets the base token group of this TokenTypeMetadataAttribute.
        /// </summary>
        public TokenGroup BaseTokenGroup { get; set; }
    }
}