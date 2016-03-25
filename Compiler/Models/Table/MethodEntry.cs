using System;
using System.Text;

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

        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="printer">The printer.</param>
        public void Print(Action<object> printer)
        {
            var str = new StringBuilder();
            var tab = "    ";

            str.AppendLine($"Return type - {this.ReturnType}");
            str.AppendLine($"Number of parameters - {this.NumberOfParameters}");
            str.AppendLine($"Size of local variables - {this.SizeOfLocal}");

            if (this.ParameterTypes != null)
            {
                var paramsTypes = this.ParameterTypes;
                while (paramsTypes != null)
                {
                    str.AppendLine($"{tab}{paramsTypes.Value}");
                    paramsTypes = paramsTypes.Next;
                }
            }
            
            printer(str);
        }
    }
}