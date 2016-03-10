namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    public class Entry
    {
        /// <summary>
        /// Gets or sets the token of this Entry.
        /// </summary>
        Token Token { get; set; }

        /// <summary>
        /// Gets or sets the depth of this Entry.
        /// </summary>
        int Depth { get; set; }

        /// <summary>
        /// Gets or sets the type of this Entry.
        /// </summary>
        EntryType Type { get; set; }

        /// <summary>
        /// Gets or sets the content of this Entry.
        /// </summary>
        public IContent Content { get; set; }
    }
}