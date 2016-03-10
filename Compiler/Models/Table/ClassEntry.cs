using System;
using System.Text;

namespace Compiler.Models.Table
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="Compiler.Models.Table.IContent" />
    internal class ClassEntry : IContent
    {
        /// <summary>
        /// Gets or sets the size of local of this ClassEntry.
        /// </summary>
        public int SizeOfLocal { get; set; }

        /// <summary>
        /// Gets or sets the method names of this ClassEntry.
        /// </summary>
        public LinkedListNode<string> MethodNames { get; set; }

        /// <summary>
        /// Gets or sets the variable names of this ClassEntry.
        /// </summary>
        public LinkedListNode<string> VariableNames { get; set; }

        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="printer">The printer.</param>
        public void Print(Action<object> printer)
        {
            var str = new StringBuilder();
            var tab = "\t";

            str.AppendLine(this.SizeOfLocal.ToString());

            str.AppendLine("Methods");
            var methodNames = this.MethodNames;
            while (methodNames != null)
            {
                str.AppendLine($"{tab}{methodNames.Value}");
                methodNames = methodNames.Next;
            }

            str.AppendLine("Variables");
            var variableNames = this.VariableNames;
            while (variableNames != null)
            {
                str.AppendLine($"{tab}{variableNames.Value}");
                variableNames = variableNames.Next;
            }

            printer(str);
        }
    }
}