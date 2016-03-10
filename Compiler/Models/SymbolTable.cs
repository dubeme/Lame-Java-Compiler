using Compiler.Models.Table;
using System.Collections.Generic;

namespace Compiler.Models
{
    public class SymbolTable
    {
        private const int PRIME_TABLE_SIZE = 211;
        private readonly LinkedListNode<IEntry>[] _Table;

        public SymbolTable()
        {
            _Table = new LinkedListNode<IEntry>[PRIME_TABLE_SIZE];
        }

        public void Insert(Token token, int depth)
        {
            var index = (int)Hash(token.Lexeme) % PRIME_TABLE_SIZE;
            
        }

        public void WriteTable(int depth)
        {
        }

        public void DeleteDepth(int depth)
        {
        }

        public IEntry Lookup(string lexeme)
        {
            return null;
        }

        /// <summary>
        /// Hashes the specified lexeme, using the hashpjw from P.J Weinberger's C compiler.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <returns>The hash of the lexeme</returns>
        private uint Hash(string lexeme)
        {
            uint hash = 0, temp;

            foreach (var ch in lexeme)
            {
                hash = (hash << 4) + ch;
                temp = hash & 0xF0000000;

                if (temp != 0)
                {
                    hash = hash ^ (temp >> 24);
                    hash = hash ^ temp;
                }
            }

            return hash;
        }
    }
}