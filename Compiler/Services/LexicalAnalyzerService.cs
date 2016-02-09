using Compiler.Models;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
///
/// </summary>
namespace Compiler.Services
{
    /// <summary>
    ///
    /// </summary>
    public class LexicalAnalyzerService
    {
        /// <summary>
        /// The EOF
        /// </summary>
        private const char EOF = char.MaxValue;

        /// <summary>
        /// The eol
        /// </summary>
        private const char EOL = '\n';

        /// <summary>
        /// Gets or sets the source code reader of this LexicalAnalyzerService.
        /// </summary>
        private StreamReader SourceCodeStream { get; set; }

        /// <summary>
        /// The _ known token types
        /// </summary>
        private static KnownTokenTypes _KnownTokenTypes = KnownTokenTypes.Instance;

        /// <summary>
        /// The line number
        /// </summary>
        private int LineNumber = 1;

        /// <summary>
        /// Gets the next character.
        /// </summary>
        private char NextChar
        {
            get
            {
                if (this.SourceCodeStream.EndOfStream)
                {
                    this.EOFReached = true;
                    return EOF;
                }

                if ((char)this.SourceCodeStream.Peek() == '\r')
                {
                    // Ignore \r
                    // The assumption is that the next character is \n
                    this.SourceCodeStream.Read();
                }

                var ch = (char)this.SourceCodeStream.Read();

                if (ch == EOL)
                {
                    this.LineNumber++;
                }

                return ch;
            }
        }

        /// <summary>
        /// Gets the next non white space character.
        /// </summary>
        private char NextNonWhiteSpaceChar
        {
            get
            {
                var currentChar = this.NextChar;

                // Skip all white spaces
                while (currentChar != EOF && char.IsWhiteSpace(currentChar))
                {
                    currentChar = this.NextChar;
                }

                return currentChar;
            }
        }

        /// <summary>
        /// Gets the peek next.
        /// </summary>
        private char PeekNext
        {
            get
            {
                if (this.SourceCodeStream.EndOfStream)
                {
                    return EOF;
                }

                return (char)this.SourceCodeStream.Peek();
            }
        }

        /// <summary>
        /// Gets and Sets a value indicating whether [EOF reached].
        /// </summary>
        private bool EOFReached { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [comment found].
        /// </summary>
        private bool CommentFound { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show stopping error occured].
        /// </summary>
        private bool ShowStoppingErrorOccured { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalAnalyzerService" /> class.
        /// </summary>
        /// <param name="sourceFile">The source file.</param>
        public LexicalAnalyzerService(StreamReader sourceFile)
        {
            this.SourceCodeStream = sourceFile;
        }

        /// <summary>
        /// Gets the next token.
        /// </summary>
        /// <returns>
        /// A token if a proper lexeme is found.
        /// Error token, if error occured while processing lexeme.
        /// Null, if the streamreader is null
        /// </returns>
        /// <exception cref="System.Exception">${LineNumber} {ex.Message}</exception>
        public Token GetNextToken()
        {
            try
            {
                if (this.SourceCodeStream != null && !this.ShowStoppingErrorOccured)
                {
                    var lexeme = NextLexeme();

                    if (EOFReached)
                    {
                        return Token.CreateEOFToken(LineNumber);
                    }
                    return Token.CreateToken(lexeme, LineNumber);
                }
            }
            catch (Exception ex)
            {
                return Token.CreateErrorToken(LineNumber, ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Nexts the lexeme.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">${currentChar} is not a recognized symbol</exception>
        private string NextLexeme()
        {
            var lexeme = string.Empty;

            do
            {
                var currentChar = this.NextNonWhiteSpaceChar;

                // When end of file
                if (this.EOFReached)
                {
                    return lexeme;
                }

                // Set that no comment is found
                this.CommentFound = false;

                switch (currentChar)
                {
                    case '(':
                    case ')':
                    case ',':
                    case ':':
                    case ';':
                    case '?':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '~': lexeme = $"{currentChar}"; break;
                    case '!':
                    case '%':
                    case '&':
                    case '*':
                    case '+':
                    case '-':
                    case '/':
                    case '<':
                    case '=':
                    case '>':
                    case '^':
                    case '|': lexeme = ExtractOperator(currentChar); break;
                    case '"': lexeme = ExtractLiteralString(); break;
                    case '.':
                    default:
                        if (currentChar == '.' && !char.IsDigit(this.PeekNext))
                        {
                            lexeme = $"{currentChar}"; break;
                        }
                        else if (currentChar == '.' || char.IsLetterOrDigit(currentChar))
                        {
                            lexeme = ExtractString(currentChar); break;
                        }
                        else
                        {
                            throw new Exception($"{currentChar} is not a recognized symbol");
                        }
                }
                // Ignore all the comments before the next valid token
            } while (this.CommentFound && !this.EOFReached);

            return lexeme;
        }

        /// <summary>
        /// Extracts the string.
        /// </summary>
        /// <param name="currentChar">The current character.</param>
        /// <returns></returns>
        private string ExtractString(char currentChar)
        {
            var literalString = new StringBuilder();
            var previousChar = currentChar;

            var containsDecimalPoint = currentChar == '.';
            var canAcceptDecimalPoint = true;

            if (char.IsDigit(currentChar) || currentChar == '.')
            {
                // Number mode
                do
                {
                    literalString.Append(currentChar);
                    containsDecimalPoint = containsDecimalPoint || currentChar == '.';
                    canAcceptDecimalPoint = this.PeekNext == '.' && !containsDecimalPoint;

                    if (!char.IsDigit(this.PeekNext) && !canAcceptDecimalPoint)
                    {
                        break;
                    }

                    currentChar = this.NextChar;
                } while (currentChar != EOF && currentChar != EOL);
            }
            else if (char.IsLetter(currentChar))
            {
                // Identifier mode
                do
                {
                    literalString.Append(currentChar);

                    if (!char.IsLetterOrDigit(this.PeekNext) && this.PeekNext != '_')
                    {
                        // TODO: Remove later, these are here for the course duration
                        // NOTE: This allows anything in the form System.out.*
                        if (!(Regex.IsMatch(literalString.ToString(), "^(System|System.out)$") && this.PeekNext == '.'))
                        {
                            break;
                        }
                    }

                    currentChar = this.NextChar;
                } while (currentChar != EOF && currentChar != EOL);
            }

            return literalString.ToString();
        }

        /// <summary>
        /// Extracts the literal string.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Invalid string literal, EOF reached before closing \</exception>
        private string ExtractLiteralString()
        {
            var literalString = new StringBuilder();
            var currentChar = this.NextChar;
            var previousChar = currentChar;

            while (currentChar != EOF && currentChar != EOL)
            {
                literalString.Append(currentChar);

                previousChar = currentChar;
                currentChar = this.NextChar;

                if (currentChar == '"' && previousChar != '\\')
                {
                    return $"\"{literalString.ToString()}\"";
                }
            }

            throw new Exception("Invalid string literal, EOF reached before closing \"");
        }

        /// <summary>
        /// Extracts the literal character.
        /// </summary>
        /// <returns></returns>
        private string ExtractLiteralChar()
        {
            return string.Empty;
        }

        /// <summary>
        /// Extracts the operator.
        /// </summary>
        /// <param name="operatorChar">The operator character.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">EOF reached while trying to find closing tag for multiline comment.</exception>
        private string ExtractOperator(char operatorChar)
        {
            var lexeme = new StringBuilder();

            lexeme.Append(operatorChar);

            if (this.PeekNext == '=')
            {
                // Operators in the form OPERATOR_EQUAL, <=, +=, !=
                lexeme.Append(this.NextChar);
            }
            else if (operatorChar == this.PeekNext && this.PeekNext != '/')
            {
                // Operators in the form OPERATOR_OPERATOR, ++, --, &&, <<, >>
                // NOTE: == will be caught above
                // NOTE: This also avoids capturing comments
                lexeme.Append(this.NextChar);

                if (lexeme.ToString() == _KnownTokenTypes[TokenType.BitwiseLeftShift] && this.PeekNext == '=')
                {
                    // Check for <<=
                    lexeme.Append(this.NextChar);
                }
                else if (lexeme.ToString() == _KnownTokenTypes[TokenType.BitwiseRightShift])
                {
                    if (this.PeekNext == '=')
                    {
                        // Check for >>=
                        lexeme.Append(this.NextChar);
                    }
                    else if (this.PeekNext == '>')
                    {
                        // Check for >>>
                        lexeme.Append(this.NextChar);

                        if (this.PeekNext == '=')
                        {
                            // Check for >>>=
                            lexeme.Append(this.NextChar);
                        }
                    }
                }
            }
            else if (operatorChar == '/')
            {
                ExtractComment(lexeme);
            }

            return lexeme.ToString();
        }

        /// <summary>
        /// Extracts the comment.
        /// </summary>
        /// <param name="lexeme">The lexeme.</param>
        /// <exception cref="System.Exception">EOF reached while trying to find closing tag for multiline comment.</exception>
        private void ExtractComment(StringBuilder lexeme)
        {
            if (this.PeekNext == '/')
            {
                // Extract single line comment
                while (this.NextChar != EOL) ;

                lexeme.Clear();
                this.CommentFound = true;
            }
            else if (this.PeekNext == '*')
            {
                // Extract multi line comment
                var currentChar = this.NextChar;

                do
                {
                    currentChar = this.NextChar;

                    if (currentChar == '/' && this.PeekNext == '*')
                    {
                        // Nested comment [Not allowed]
                        this.ShowStoppingErrorOccured = true;
                        throw new Exception("Nested comments not allowed.");
                    }

                    if (currentChar == '*' && this.PeekNext == '/')
                    {
                        // Ending slash found
                        currentChar = this.NextChar;
                        break;
                    }

                    if (this.PeekNext == EOF)
                    {
                        throw new Exception("EOF reached while trying to find closing tag for multiline comment.");
                    }
                } while (this.PeekNext != EOF);

                lexeme.Clear();
                this.CommentFound = true;
            }
        }
    }
}