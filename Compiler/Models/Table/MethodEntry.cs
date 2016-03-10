namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    public class MethodEntry : IContent
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