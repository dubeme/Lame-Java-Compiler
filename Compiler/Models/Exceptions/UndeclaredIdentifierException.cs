namespace Compiler.Services
{
    [System.Serializable]
    public class UndeclaredIdentifierException : System.Exception
    {
        public UndeclaredIdentifierException(string identifier) : base($"{identifier} isn't declared in current scope.")
        {
        }

        public UndeclaredIdentifierException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected UndeclaredIdentifierException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}