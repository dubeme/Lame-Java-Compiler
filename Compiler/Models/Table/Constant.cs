namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    public class Constant : IContent
    {
        /// <summary>
        /// Gets or sets the type of this Constant.
        /// </summary>
        public VariableType Type { get; set; }
        /// <summary>
        /// Gets or sets the offset of this Constant.
        /// </summary>
        public int Offset { get; set; }
    }
}