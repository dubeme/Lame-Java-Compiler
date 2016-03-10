namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IEntry" />
    public class VariableEntry : IEntry
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
        /// Gets or sets the type of this Variable.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public VariableType DataType { get; set; }

        /// <summary>
        /// Gets or sets the offset of this Variable.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the size of this Variable.
        /// </summary>
        public int Size { get; set; }
    }
}