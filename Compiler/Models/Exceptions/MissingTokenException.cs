using System;

namespace Compiler.Models.Exceptions
{
    [Serializable]
    public class MissingTokenException : Exception
    {
        public MissingTokenException(string expectedToken, string production) : base($"({expectedToken}) Token missing in ({production}) Production")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingTokenException"/> class.
        /// </summary>
        /// <param name="expectedGroup">The expected group.</param>
        /// <param name="production">The production.</param>
        public MissingTokenException(TokenGroup expectedGroup, string production) : base($"({expectedGroup}) Token missing in ({production}) Production")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingTokenException"/> class.
        /// </summary>
        /// <param name="expectedType">The expected token type.</param>
        /// <param name="production">The production.</param>
        public MissingTokenException(TokenType expectedType, string production) : base($"({expectedType}) Token missing in ({production}) Production")
        {
        }

        protected MissingTokenException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}