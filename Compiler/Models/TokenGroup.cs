namespace Compiler.Models
{
    public enum TokenGroup
    {
        Unknown,
        ReservedWord,
        Identifier,
        Number,
        Literal,
        Relational,
        Operator,
        SpecialCharacter,
    }
}