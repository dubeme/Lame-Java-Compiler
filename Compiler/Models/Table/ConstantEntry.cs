using System;

namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    public class ConstantEntry<T> : IContent
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
        /// Gets or sets the value of this ConstantEntry{T}.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="printer">The printer.</param>
        public void Print(Action<object> printer)
        {
            printer($"{this.DataType,-10} {this.Offset,-10}");
        }
    }
}