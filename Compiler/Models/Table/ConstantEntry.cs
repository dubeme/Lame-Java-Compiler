using System;

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

        /// <summary>
        /// Gets or sets the value of this ConstantEntry.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="printer">The printer.</param>
        public void Print(string lexeme, Action<object> printer)
        {
            var _offset = $"Offset({this.Offset})";
            printer($"final {this.DataType, -12} {lexeme,-10} {_offset,-15} {this.Value}");
        }
    }
}