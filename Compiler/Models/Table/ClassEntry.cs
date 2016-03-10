using System.Collections.Generic;

namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IEntry" />
    internal class ClassEntry : IEntry
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
        /// Gets or sets the size of local of this ClassEntry.
        /// </summary>
        public int SizeOfLocal { get; set; }

        /// <summary>
        /// Gets or sets the method names of this ClassEntry.
        /// </summary>
        public LinkedListNode<string> MethodNames { get; set; }

        /// <summary>
        /// Gets or sets the variable names of this ClassEntry.
        /// </summary>
        public LinkedListNode<string> VariableNames { get; set; }
    }
}