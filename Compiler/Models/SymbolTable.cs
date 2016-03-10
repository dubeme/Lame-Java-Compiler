using Compiler.Models.Table;

namespace Compiler.Models
{
    public class SymbolTable
    {
        private const int PRIME_TABLE_SIZE = 211;
        private readonly LinkedListNode<Entry>[] _Table;

        public SymbolTable()
        {
            _Table = new LinkedListNode<Entry>[PRIME_TABLE_SIZE];
        }

        public void Insert(Token token, int depth)
        {
            var index = (int)Hash(token.Lexeme) % PRIME_TABLE_SIZE;
            var newEntry = new LinkedListNode<Entry>
            {
                Value = new Entry
                {
                    Token = token,
                    Depth = depth
                }
            };

            if (this._Table[index] != null)
            {
                newEntry.Next = this._Table[index];
            }

            this._Table[index] = newEntry;
        }

        public void WriteTable(int depth)
        {

        }

        public void DeleteDepth(int depth)
        {
        }

        public Entry Lookup(string lexeme)
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