using System.Collections.Generic;

namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    public class Method : IContent
    {
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
        public VariableType ReturnType { get; set; }
        /// <summary>
        /// Gets or sets the parameters of this Method.
        /// </summary>
        public LinkedListNode<VariableType> ParameterTypes { get; set; }
    }
}