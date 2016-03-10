namespace Compiler.Models.Table
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedListNode<T>
    {
        /// <summary>
        /// Gets or sets the value of this LinkedListNode{T}.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the next node of this LinkedListNode{T}.
        /// </summary>
        public LinkedListNode<T> Next { get; set; }
    }
}