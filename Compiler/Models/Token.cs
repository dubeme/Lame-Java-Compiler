namespace Compiler.Models
{
    public class Token
    {
        public TokenType Type { get; set; }
        public Lexeme Lexeme { get; set; }
        public TokenGroup Group { get; set; }

    }
}