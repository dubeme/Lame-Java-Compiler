using Compiler.Models.Table;
using System;

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
                this._Table[index].Previous = newEntry;
            }

            this._Table[index] = newEntry;
        }

        public void WriteTable(int depth)
        {
            var printer = new Action<object>((obj) => { });

            for (int index = 0; index < PRIME_TABLE_SIZE; index++)
            {
                var item = this._Table[index];
                if (item != null)
                {
                    while (item != null && item.Value.Depth >= depth)
                    {
                        if (item.Value.Depth == depth)
                        {
                            item.Value.Content.Print(printer);
                        }

                        item = item.Next;
                    }
                }
            }
        }

        public void DeleteDepth(int depth)
        {
            for (int index = 0; index < PRIME_TABLE_SIZE; index++)
            {
                if (this._Table[index] != null)
                {
                    this._Table[index] = Remove(depth, this._Table[index]);
                }
            }
        }

        public Entry Lookup(string lexeme)
        {
            var index = (int)Hash(lexeme) % PRIME_TABLE_SIZE;
            var item = this._Table[index];

            if (item != null)
            {
                while (item != null)
                {
                    if (item.Value.Token.Lexeme == lexeme)
                    {
                        return item.Value;
                    }

                    item = item.Next;
                }
            }

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

        private LinkedListNode<Entry> Remove(int depth, LinkedListNode<Entry> src)
        {
            var result = src;

            if (src == null)
            {
                return null;
            }

            if (src.Value.Depth == depth)
            {
                // If the first item is the same depth
                while (src != null && src.Value.Depth == depth)
                {
                    src = src.Next;
                }

                return src;
            }
            else
            {
                while (src != null && src.Value.Depth >= depth)
                {
                    if (src.Value.Depth == depth)
                    {
                        //   A <--> B <--> C
                        //   A <--> C
                        // item = B

                        if (src.Previous != null)
                        {
                            //   A --> C
                            src.Previous.Next = src.Next;
                        }

                        if (src.Next != null)
                        {
                            //   A <-- C
                            src.Next.Previous = src.Previous;
                        }
                    }

                    src = src.Next;
                }
            }

            return result;
        }
    }
}