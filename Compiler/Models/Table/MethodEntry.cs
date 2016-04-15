using System;
using System.Collections.Generic;
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
        public LinkedListNode<KeyValuePair<string, VariableType>> Parameters { get; set; }

        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="printer">The printer.</param>
        public void Print(string lexeme, Action<object> printer)
        {
            var str = new StringBuilder();
            var tab = "    ";

            str.Append($"{this.ReturnType} {lexeme}");

            if (this.Parameters != null)
            {
                var paramsTypes = this.Parameters;

                str.Append($"(");
                while (paramsTypes != null)
                {
                    str.Append($"{paramsTypes.Value}");
                    paramsTypes = paramsTypes.Next;

                    if (paramsTypes != null)
                    {
                        str.Append(", ");
                    }
                }
                str.AppendLine($")");
            }
            else
            {
                str.AppendLine("()");
            }

            str.AppendLine($"{tab}Number of parameters - {this.NumberOfParameters}");
            str.Append($"{tab}Size of local variables - {this.SizeOfLocal}");

            printer(str);
        }
    }
}