using System;

namespace Compiler.Models.Table
{
    public interface IContent
    {
        /// <summary>
        /// Prints the content using the specified printer.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <param name="printer">The printer.</param>
        void Print(string lexeme, Action<object> printer);
    }
}