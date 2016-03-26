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
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="printer">The printer.</param>
        public void Print(string lexeme, Action<object> printer)
        {
            var str = new StringBuilder($"class {lexeme} {{}}\n");
            var tab = "    ";

            if (this.Fields == null)
            {
                str.AppendLine($"{tab}Contains no fields");
            }
            else
            {
                str.AppendLine($"{tab}Total size of the class fields - {this.SizeOfLocal}");
                str.AppendLine($"{tab}Fields");
                var fields = this.Fields;
                while (fields != null)
                {
                    str.AppendLine($"{tab}{tab}{fields.Value}");
                    fields = fields.Next;
                }
            }

            if (this.MethodNames == null)
            {
                str.Append($"{tab}Contains no methods");
            }
            else
            {
                str.AppendLine($"{tab}Methods");
                var methodNames = this.MethodNames;
                while (methodNames != null)
                {
                    str.Append($"{tab}{tab}{methodNames.Value}");
                    methodNames = methodNames.Next;
                }
            }

            printer(str);
        }
    }
}