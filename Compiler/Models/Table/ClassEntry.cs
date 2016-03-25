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
        public LinkedListNode<string> Fields { get; set; }

        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="printer">The printer.</param>
        public void Print(Action<object> printer)
        {
            var str = new StringBuilder();
            var tab = "    ";

            if (this.Fields == null)
            {
                str.AppendLine("Class contains no fields");
            }
            else
            {
                str.AppendLine($"Total size of the class fields - {this.SizeOfLocal}");
                str.AppendLine("Fields");
                var fields = this.Fields;
                while (fields != null)
                {
                    str.AppendLine($"{tab}{fields.Value}");
                    fields = fields.Next;
                }
            }

            if (this.MethodNames == null)
            {
                str.AppendLine("Class contains no methods");
            }
            else
            {
                str.AppendLine("Methods");
                var methodNames = this.MethodNames;
                while (methodNames != null)
                {
                    str.AppendLine($"{tab}{methodNames.Value}");
                    methodNames = methodNames.Next;
                }
            }

            printer(str);
        }
    }
}