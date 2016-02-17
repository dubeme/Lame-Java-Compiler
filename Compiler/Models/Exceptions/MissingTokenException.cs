using System;

namespace Compiler.Models.Exceptions
{
    [Serializable]
    public class MissingTokenException : Exception
    {
        public MissingTokenException(TokenType tokenType, string production) : base($"({tokenType}) Token missing in ({production}) Production")
        {
        }

        protected MissingTokenException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}