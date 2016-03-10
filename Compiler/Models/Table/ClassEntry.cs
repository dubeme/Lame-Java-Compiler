using System.Collections.Generic;

namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    internal class ClassEntry : IContent
    {
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