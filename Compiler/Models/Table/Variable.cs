namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    public class Variable : IContent
    {
        /// <summary>
        /// Gets or sets the type of this Variable.
        /// </summary>
        public VariableType Type { get; set; }
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