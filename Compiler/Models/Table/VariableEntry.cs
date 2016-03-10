using System;

namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    /// <seealso cref="Compiler.Models.Table.Entry" />
    public class VariableEntry : IContent
    {
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
        
        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="printer">The printer.</param>
        public void Print(Action<object> printer)
        {
            printer($"{this.DataType, -10} {this.Offset,-10} {this.Size,-10}");
        }
    }
}