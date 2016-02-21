using System;

namespace Compiler.Models.Exceptions
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class MissingTokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingTokenException" /> class.
        /// </summary>
        /// <param name="expectedGroup">The expected group.</param>
        /// <param name="foundGroup">The found group.</param>
        /// <param name="production">The production.</param>
        public MissingTokenException(TokenGroup expectedGroup, TokenGroup foundGroup, string production)
            : this(expectedGroup.ToString(), foundGroup.ToString(), production)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingTokenException" /> class.
        /// </summary>
        /// <param name="expectedType">The expected token type.</param>
        /// <param name="foundType">Type of the found.</param>
        /// <param name="production">The production.</param>
        public MissingTokenException(TokenType expectedType, TokenType foundType, string production)
            : this(expectedType.ToString(), foundType.ToString(), production)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingTokenException"/> class.
        /// </summary>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="foundType">Type of the found.</param>
        /// <param name="production">The production.</param>
        public MissingTokenException(string expectedType, string foundType, string production) 
            : base ($"Expected ({expectedType}) Token, but found ({foundType}) Token in ({production}) Production.")
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingTokenException"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected MissingTokenException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}