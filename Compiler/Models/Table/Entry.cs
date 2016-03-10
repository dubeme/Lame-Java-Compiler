namespace Compiler.Models.Table
{
    public class Entry
    {
        public Token Token { get; set; }
        public int Depth { get; set; }
        public EntryType Type { get; set; }
        public IContent Content { get; set; }
    }
}