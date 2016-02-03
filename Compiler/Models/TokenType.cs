using Compiler.Models.Attributes;

namespace Compiler.Models
{
    public enum TokenType
    {
        [TokenTypeMetadata(Lexeme = "", BaseTokenGroup = TokenGroup.Unknown)]
        Unknown = 0,

        [TokenTypeMetadata(Lexeme = "abstract", BaseTokenGroup = TokenGroup.ReservedWord)]
        Abstract,

        [TokenTypeMetadata(Lexeme = "assert", BaseTokenGroup = TokenGroup.ReservedWord)]
        Assert,

        [TokenTypeMetadata(Lexeme = "=", BaseTokenGroup = TokenGroup.Operator)]
        Assignment,

        [TokenTypeMetadata(Lexeme = "<<", BaseTokenGroup = TokenGroup.Operator)]
        BitwiseLeftShift,

        [TokenTypeMetadata(Lexeme = "<<=", BaseTokenGroup = TokenGroup.Operator)]
        BitwiseLeftShiftEqual,

        [TokenTypeMetadata(Lexeme = ">>", BaseTokenGroup = TokenGroup.Operator)]
        BitwiseRightShift,

        [TokenTypeMetadata(Lexeme = ">>=", BaseTokenGroup = TokenGroup.Operator)]
        BitwiseRightShiftEqual,

        [TokenTypeMetadata(Lexeme = "boolean", BaseTokenGroup = TokenGroup.ReservedWord)]
        Boolean,

        [TokenTypeMetadata(Lexeme = "&&", BaseTokenGroup = TokenGroup.Operator)]
        BooleanAnd,

        [TokenTypeMetadata(Lexeme = "==", BaseTokenGroup = TokenGroup.Operator)]
        BooleanEqual,

        [TokenTypeMetadata(Lexeme = "!", BaseTokenGroup = TokenGroup.Operator)]
        BooleanNot,

        [TokenTypeMetadata(Lexeme = "||", BaseTokenGroup = TokenGroup.Operator)]
        BooleanOr,

        [TokenTypeMetadata(Lexeme = "break", BaseTokenGroup = TokenGroup.ReservedWord)]
        Break,

        [TokenTypeMetadata(Lexeme = "byte", BaseTokenGroup = TokenGroup.ReservedWord)]
        Byte,

        [TokenTypeMetadata(Lexeme = "case", BaseTokenGroup = TokenGroup.ReservedWord)]
        Case,

        [TokenTypeMetadata(Lexeme = "catch", BaseTokenGroup = TokenGroup.ReservedWord)]
        Catch,

        [TokenTypeMetadata(Lexeme = "char", BaseTokenGroup = TokenGroup.ReservedWord)]
        Char,

        [TokenTypeMetadata(Lexeme = "class", BaseTokenGroup = TokenGroup.ReservedWord)]
        Class,

        [TokenTypeMetadata(Lexeme = "}", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        CloseCurlyBrace,

        [TokenTypeMetadata(Lexeme = ")", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        CloseParen,

        [TokenTypeMetadata(Lexeme = "]", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        CloseSquareBracket,

        [TokenTypeMetadata(Lexeme = ":", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        Colon,

        [TokenTypeMetadata(Lexeme = ",", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        Comma,

        [TokenTypeMetadata(Lexeme = "const", BaseTokenGroup = TokenGroup.ReservedWord)]
        Const,

        [TokenTypeMetadata(Lexeme = "continue", BaseTokenGroup = TokenGroup.ReservedWord)]
        Continue,

        [TokenTypeMetadata(Lexeme = "default", BaseTokenGroup = TokenGroup.ReservedWord)]
        Default,

        [TokenTypeMetadata(Lexeme = "/", BaseTokenGroup = TokenGroup.Operator)]
        Divide,

        [TokenTypeMetadata(Lexeme = "/=", BaseTokenGroup = TokenGroup.Operator)]
        DivideEqual,

        [TokenTypeMetadata(Lexeme = "do", BaseTokenGroup = TokenGroup.ReservedWord)]
        Do,

        [TokenTypeMetadata(Lexeme = ".", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        Dot,

        [TokenTypeMetadata(Lexeme = "double", BaseTokenGroup = TokenGroup.ReservedWord)]
        Double,

        [TokenTypeMetadata(Lexeme = "\"", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        DoubleQuote,

        [TokenTypeMetadata(Lexeme = "else", BaseTokenGroup = TokenGroup.ReservedWord)]
        Else,

        [TokenTypeMetadata(Lexeme = "enum", BaseTokenGroup = TokenGroup.ReservedWord)]
        Enum,

        [TokenTypeMetadata(Lexeme = "extends", BaseTokenGroup = TokenGroup.ReservedWord)]
        Extends,

        [TokenTypeMetadata(Lexeme = "false", BaseTokenGroup = TokenGroup.ReservedWord)]
        False,

        [TokenTypeMetadata(Lexeme = "final", BaseTokenGroup = TokenGroup.ReservedWord)]
        Final,

        [TokenTypeMetadata(Lexeme = "finally", BaseTokenGroup = TokenGroup.ReservedWord)]
        Finally,

        [TokenTypeMetadata(Lexeme = "float", BaseTokenGroup = TokenGroup.ReservedWord)]
        Float,

        [TokenTypeMetadata(Lexeme = "for", BaseTokenGroup = TokenGroup.ReservedWord)]
        For,

        [TokenTypeMetadata(Lexeme = "goto", BaseTokenGroup = TokenGroup.ReservedWord)]
        Goto,

        [TokenTypeMetadata(Lexeme = ">", BaseTokenGroup = TokenGroup.Operator)]
        GreaterThan,

        [TokenTypeMetadata(Lexeme = ">=", BaseTokenGroup = TokenGroup.Operator)]
        GreaterThanOrEqual,

        [TokenTypeMetadata(Lexeme = "", BaseTokenGroup = TokenGroup.Identifier)]
        Identifier,

        [TokenTypeMetadata(Lexeme = "if", BaseTokenGroup = TokenGroup.ReservedWord)]
        If,

        [TokenTypeMetadata(Lexeme = "implements", BaseTokenGroup = TokenGroup.ReservedWord)]
        Implements,

        [TokenTypeMetadata(Lexeme = "import", BaseTokenGroup = TokenGroup.ReservedWord)]
        Import,

        [TokenTypeMetadata(Lexeme = "instanceof", BaseTokenGroup = TokenGroup.ReservedWord)]
        Instanceof,

        [TokenTypeMetadata(Lexeme = "int", BaseTokenGroup = TokenGroup.ReservedWord)]
        Int,

        [TokenTypeMetadata(Lexeme = "interface", BaseTokenGroup = TokenGroup.ReservedWord)]
        Interface,

        [TokenTypeMetadata(Lexeme = "<", BaseTokenGroup = TokenGroup.Operator)]
        LessThan,

        [TokenTypeMetadata(Lexeme = "<=", BaseTokenGroup = TokenGroup.Operator)]
        LessThanOrEqual,

        [TokenTypeMetadata(Lexeme = "", BaseTokenGroup = TokenGroup.Literal)]
        LiteralBoolean,

        [TokenTypeMetadata(Lexeme = "", BaseTokenGroup = TokenGroup.Literal)]
        LiteralInteger,

        [TokenTypeMetadata(Lexeme = "", BaseTokenGroup = TokenGroup.Literal)]
        LiteralReal,

        [TokenTypeMetadata(Lexeme = "", BaseTokenGroup = TokenGroup.Literal)]
        LiteralString,

        [TokenTypeMetadata(Lexeme = "&", BaseTokenGroup = TokenGroup.Operator)]
        LogicalAnd,

        [TokenTypeMetadata(Lexeme = "&=", BaseTokenGroup = TokenGroup.Operator)]
        LogicalAndEqual,

        [TokenTypeMetadata(Lexeme = "^", BaseTokenGroup = TokenGroup.Operator)]
        LogicalExclusiveOr,

        [TokenTypeMetadata(Lexeme = "^=", BaseTokenGroup = TokenGroup.Operator)]
        LogicalExclusiveOrEqual,

        [TokenTypeMetadata(Lexeme = "~", BaseTokenGroup = TokenGroup.Operator)]
        LogicalNot,

        [TokenTypeMetadata(Lexeme = "|", BaseTokenGroup = TokenGroup.Operator)]
        LogicalOr,

        [TokenTypeMetadata(Lexeme = "|=", BaseTokenGroup = TokenGroup.Operator)]
        LogicalOrEqual,

        [TokenTypeMetadata(Lexeme = "long", BaseTokenGroup = TokenGroup.ReservedWord)]
        Long,

        [TokenTypeMetadata(Lexeme = "-", BaseTokenGroup = TokenGroup.Operator)]
        Minus,

        [TokenTypeMetadata(Lexeme = "-=", BaseTokenGroup = TokenGroup.Operator)]
        MinusEqual,

        [TokenTypeMetadata(Lexeme = "--", BaseTokenGroup = TokenGroup.Operator)]
        MinusMinus,

        [TokenTypeMetadata(Lexeme = "%", BaseTokenGroup = TokenGroup.Operator)]
        Modulo,

        [TokenTypeMetadata(Lexeme = "%=", BaseTokenGroup = TokenGroup.Operator)]
        ModuloEqual,

        [TokenTypeMetadata(Lexeme = "*", BaseTokenGroup = TokenGroup.Operator)]
        Multiplication,

        [TokenTypeMetadata(Lexeme = "*=", BaseTokenGroup = TokenGroup.Operator)]
        MultiplicationEqual,

        [TokenTypeMetadata(Lexeme = "native", BaseTokenGroup = TokenGroup.ReservedWord)]
        Native,

        [TokenTypeMetadata(Lexeme = "new", BaseTokenGroup = TokenGroup.ReservedWord)]
        New,

        [TokenTypeMetadata(Lexeme = "!=", BaseTokenGroup = TokenGroup.Operator)]
        NotEqual,

        [TokenTypeMetadata(Lexeme = "{", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        OpenCurlyBrace,

        [TokenTypeMetadata(Lexeme = "(", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        OpenParen,

        [TokenTypeMetadata(Lexeme = "[", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        OpenSquareBracket,

        [TokenTypeMetadata(Lexeme = "package", BaseTokenGroup = TokenGroup.ReservedWord)]
        Package,

        [TokenTypeMetadata(Lexeme = "+", BaseTokenGroup = TokenGroup.Operator)]
        Plus,

        [TokenTypeMetadata(Lexeme = "+=", BaseTokenGroup = TokenGroup.Operator)]
        PlusEqual,

        [TokenTypeMetadata(Lexeme = "++", BaseTokenGroup = TokenGroup.Operator)]
        PlusPlus,

        [TokenTypeMetadata(Lexeme = "private", BaseTokenGroup = TokenGroup.ReservedWord)]
        Private,

        [TokenTypeMetadata(Lexeme = "protected", BaseTokenGroup = TokenGroup.ReservedWord)]
        Protected,

        [TokenTypeMetadata(Lexeme = "public", BaseTokenGroup = TokenGroup.ReservedWord)]
        Public,

        [TokenTypeMetadata(Lexeme = "?", BaseTokenGroup = TokenGroup.Operator)]
        QuestionMark,

        [TokenTypeMetadata(Lexeme = "return", BaseTokenGroup = TokenGroup.ReservedWord)]
        Return,

        [TokenTypeMetadata(Lexeme = ";", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        Semicolon,

        [TokenTypeMetadata(Lexeme = "short", BaseTokenGroup = TokenGroup.ReservedWord)]
        Short,

        [TokenTypeMetadata(Lexeme = "'", BaseTokenGroup = TokenGroup.SpecialCharacter)]
        SingleQuote,

        [TokenTypeMetadata(Lexeme = "static", BaseTokenGroup = TokenGroup.ReservedWord)]
        Static,

        [TokenTypeMetadata(Lexeme = "strictfp", BaseTokenGroup = TokenGroup.ReservedWord)]
        Strictfp,

        [TokenTypeMetadata(Lexeme = "super", BaseTokenGroup = TokenGroup.ReservedWord)]
        Super,

        [TokenTypeMetadata(Lexeme = "switch", BaseTokenGroup = TokenGroup.ReservedWord)]
        Switch,

        [TokenTypeMetadata(Lexeme = "synchronized", BaseTokenGroup = TokenGroup.ReservedWord)]
        Synchronized,

        [TokenTypeMetadata(Lexeme = "this", BaseTokenGroup = TokenGroup.ReservedWord)]
        This,

        [TokenTypeMetadata(Lexeme = "throw", BaseTokenGroup = TokenGroup.ReservedWord)]
        Throw,

        [TokenTypeMetadata(Lexeme = "throws", BaseTokenGroup = TokenGroup.ReservedWord)]
        Throws,

        [TokenTypeMetadata(Lexeme = "transient", BaseTokenGroup = TokenGroup.ReservedWord)]
        Transient,

        [TokenTypeMetadata(Lexeme = "true", BaseTokenGroup = TokenGroup.ReservedWord)]
        True,

        [TokenTypeMetadata(Lexeme = "try", BaseTokenGroup = TokenGroup.ReservedWord)]
        Try,

        [TokenTypeMetadata(Lexeme = ">>>", BaseTokenGroup = TokenGroup.Operator)]
        UnsignedLeftShift,

        [TokenTypeMetadata(Lexeme = ">>>=", BaseTokenGroup = TokenGroup.Operator)]
        UnsignedLeftShiftEqual,

        [TokenTypeMetadata(Lexeme = "void", BaseTokenGroup = TokenGroup.ReservedWord)]
        Void,

        [TokenTypeMetadata(Lexeme = "volatile", BaseTokenGroup = TokenGroup.ReservedWord)]
        Volatile,

        [TokenTypeMetadata(Lexeme = "while", BaseTokenGroup = TokenGroup.ReservedWord)]
        While,

        //TODO: Remove later, these are here for the course duration

        [TokenTypeMetadata(Lexeme = "main", BaseTokenGroup = TokenGroup.ReservedWord)]
        Main,

        [TokenTypeMetadata(Lexeme = "length", BaseTokenGroup = TokenGroup.ReservedWord)]
        Length,

        [TokenTypeMetadata(Lexeme = "System.out.println", BaseTokenGroup = TokenGroup.ReservedWord)]
        SystemOutPrintln,

        [TokenTypeMetadata(Lexeme = "String", BaseTokenGroup = TokenGroup.ReservedWord)]
        String,

        [TokenTypeMetadata(Lexeme = "", BaseTokenGroup = TokenGroup.Unknown)]
        EndOfFile,
    }
}