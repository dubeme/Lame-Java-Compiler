namespace Compiler.Services
{
    [System.Serializable]
    public class UndeclaredVariableException : System.Exception
    {
        public UndeclaredVariableException(string identifier) : base($"{identifier} isn't declared in current scope.")
        {
        }

        public UndeclaredVariableException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected UndeclaredVariableException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}