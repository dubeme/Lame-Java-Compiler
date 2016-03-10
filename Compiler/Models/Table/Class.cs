using System.Collections.Generic;

namespace Compiler.Models.Table
{
    internal class Class : IContent
    {
        public int SizeOfLocal { get; set; }
        public LinkedListNode<string> MethodNames { get; set; }
        public LinkedListNode<string> VariableNames { get; set; }
    }
}