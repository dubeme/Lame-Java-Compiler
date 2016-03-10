namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IEntry" />
    public class ConstantEntry : IEntry
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
        /// Gets or sets the type of this Constant.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public VariableType DataType { get; set; }

        /// <summary>
        /// Gets or sets the offset of this Constant.
        /// </summary>
        public int Offset { get; set; }
    }
}