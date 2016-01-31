using Compiler.Models;
using System;
using System.IO;
using System.Text;

namespace Compiler.Services
{
    public class LexicalAnalyzerService
    {
        private const char EOF = char.MaxValue;
        private const char EOL = '\n';

        public StreamReader SourceCodeReader { get; set; }

        private static KnownTokenTypes _KnownTokenTypes = KnownTokenTypes.Instance;
        private int LineNumber = 0;

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

        public LexicalAnalyzerService(string sourceCodeFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceCodeFilePath))
            {
                return;
            }

            this.SourceCodeReader = new StreamReader(sourceCodeFilePath);
        }

        public Token GetNextToken()
        {
            try
            {
                if (this.PeekNext != EOF)
                {
                    return Token.CreateToken(NextLexeme());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{LineNumber} {ex.Message}"); ;
            }

            return null;
        }

        private string NextLexeme()
        {
            var currentChar = this.NextChar;
            var lexeme = new StringBuilder();

            // Skip all white spaces
            while (currentChar != EOF && char.IsWhiteSpace(currentChar))
            {
                currentChar = this.NextChar;
            }

            // When end of file
            if (currentChar == EOF)
            {
                return string.Empty;
            }

            switch (currentChar)
            {
                case '(':
                case ')':
                case ',':
                case '.':
                case ':':
                case ';':
                case '?':
                case '[':
                case ']':
                case '{':
                case '}':
                case '~': lexeme.Append(currentChar); break;
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
                case '|': lexeme.Append(ExtractOperator(currentChar)); break;
                case '"': lexeme.Append(ExtractLiteralString()); break;
                default:
                    if (char.IsLetterOrDigit(this.PeekNext))
                    {
                        lexeme.Append(ExtractString(currentChar));
                    }
                    else
                    {
                        throw new Exception($"{currentChar} is not a recognized symbol");
                    }

                    break;
            }

            return lexeme.ToString();
        }

        private string ExtractString(char currentChar)
        {
            var literalString = new StringBuilder();
            var previousChar = currentChar;

            var hasDot = false;

            if (char.IsDigit(currentChar))
            {
                // Number mode
                do
                {
                    literalString.Append(currentChar);
                    hasDot = hasDot || currentChar == '.';

                    if (!char.IsDigit(this.PeekNext) || (hasDot && this.PeekNext == '.'))
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

                    if (!char.IsLetterOrDigit(this.PeekNext) || this.PeekNext != '_')
                    {
                        break;
                    }

                    currentChar = this.NextChar;
                } while (currentChar != EOF && currentChar != EOL);
            }

            return literalString.ToString();
        }

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
                    return literalString.ToString();
                }
            }

            throw new Exception("Invalid string literal, EOF reached before closing \"");
        }

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