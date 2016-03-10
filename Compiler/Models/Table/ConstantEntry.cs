namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    public class ConstantEntry : IContent
    {
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