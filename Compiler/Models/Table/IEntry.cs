namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEntry
    {
        /// <summary>
        /// Gets or sets the token of this IEntry.
        /// </summary>
        Token Token { get; set; }
        /// <summary>
        /// Gets or sets the depth of this IEntry.
        /// </summary>
        int Depth { get; set; }
        /// <summary>
        /// Gets or sets the type of this IEntry.
        /// </summary>
        EntryType Type { get; set; }
    }
}