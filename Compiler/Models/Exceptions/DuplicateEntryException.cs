using System;

namespace Compiler.Models.Exceptions
{
    [Serializable]
    public class DuplicateEntryException : Exception
    {
        public DuplicateEntryException(string name) : base($"{name} already exists in current scope.")
        {
        }

        public DuplicateEntryException(string name, Exception inner) : base($"{name} already exists in current scope.", inner)
        {
        }

        protected DuplicateEntryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }
}