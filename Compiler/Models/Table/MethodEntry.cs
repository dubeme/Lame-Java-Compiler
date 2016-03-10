using System.Collections.Generic;

namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IEntry" />
    public class MethodEntry : IEntry
    {
        /// <summary>
        /// Gets or sets the token of this IEntry.
        /// </summary>
        public Token Token { get; set; }

        /// <summary>
        /// Gets or sets the depth of this IEntry.
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Gets or sets the type of this IEntry.
        /// </summary>
        public EntryType Type { get; set; }

        /// <summary>
        /// Gets or sets the total size of the local variables in this Method.
        /// </summary>
        public int SizeOfLocal { get; set; }

        /// <summary>
        /// Gets or sets the number of parameters passed to this Method.
        /// </summary>
        public int NumberOfParameters { get; set; }

        /// <summary>
        /// Gets or sets the return type for this function.
        /// </summary>
        /// <value>
        /// The type of the return.
        /// </value>
        public VariableType ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the parameters of this Method.
        /// </summary>
        public LinkedListNode<VariableType> ParameterTypes { get; set; }
    }
}