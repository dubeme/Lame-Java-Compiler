using Compiler.Models;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
        public StreamReader SourceCodeReader { get; set; }

        /// <summary>
        /// The _ known token types
        /// </summary>
        private static KnownTokenTypes _KnownTokenTypes = KnownTokenTypes.Instance;

        /// <summary>
        /// The line number
        /// </summary>
        private int LineNumber = 0;

        /// <summary>
        /// Gets the next character.
        /// </summary>
        private char NextChar
        {
            get
            {
                if (this.SourceCodeReader.EndOfStream)
                {
                    return EOF;
                }

                if ((char)this.SourceCodeReader.Peek() == '\r')
                {
                    // Ignore \r
                    // The assumption is that the next character is \n
                    this.SourceCodeReader.Read();
                    this.LineNumber++;
                }

                return (char)this.SourceCodeReader.Read();
            }
        }

        /// <summary>
        /// Gets the peek next.
        /// </summary>
        private char PeekNext
        {
            get
            {
                if (this.SourceCodeReader.EndOfStream)
                {
                    return EOF;
                }

                return (char)this.SourceCodeReader.Peek();
            }
        }

        public bool EOFReached { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LexicalAnalyzerService"/> class.
        /// </summary>
        /// <param name="sourceCodeFilePath">The source code file path.</param>
        public LexicalAnalyzerService(string sourceCodeFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceCodeFilePath))
            {
                return;
            }

            this.SourceCodeReader = new StreamReader(sourceCodeFilePath);
            this.LineNumber = 1;
        }

        /// <summary>
        /// Gets the next token.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">${LineNumber} {ex.Message}</exception>
        public Token GetNextToken()
        {
            try
            {
                if (this.PeekNext != EOF)
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
            var currentChar = this.NextChar;
            var lexeme = string.Empty;

            // Skip all white spaces
            while (currentChar != EOF && char.IsWhiteSpace(currentChar))
            {
                currentChar = this.NextChar;
            }

            // When end of file
            if (currentChar == EOF)
            {
                this.EOFReached = true;
                return string.Empty;
            }

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
                        lexeme = $"{currentChar}";
                    }
                    else if (currentChar == '.' || char.IsLetterOrDigit(currentChar))
                    {
                        lexeme = ExtractString(currentChar);
                    }
                    else
                    {
                        throw new Exception($"{currentChar} is not a recognized symbol");
                    }

                    break;
            }

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
                        if (!(Regex.IsMatch(literalString.ToString(), "(System|System.out)") && this.PeekNext == '.'))
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

        private string ExtractLiteralChar()
        {
            return string.Empty;
        }

        /// <summary>
        /// Extracts the operator.
        /// </summary>
        /// <param name="operatorChar">The operator character.</param>
        /// <returns></returns>
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
                // Extract comment
            }

            return lexeme.ToString();
        }
    }
}