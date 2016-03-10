using System;

namespace Compiler.Models.Table
{
    public interface IContent
    {
        /// <summary>
        /// Prints the content using thspecified printer.
        /// </summary>
        /// <param name="printer">The printer.</param>
        void Print(Action<object> printer);
    }
}