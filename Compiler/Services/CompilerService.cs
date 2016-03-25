using Compiler.Models;
using System;
using System.IO;
using System.Text;

namespace Compiler.Services
{
    public class CompilerService
    {
        private static ConsoleColor NormalColor = Console.ForegroundColor;
        private static ConsoleColor ErrorColor = ConsoleColor.Red;
        private static ConsoleColor InfoColor = ConsoleColor.Yellow;

        public void Compile(string fileName)
        {
            try
            {
                var streamReader = new StreamReader(fileName);
                var lexAnalyzer = new LexicalAnalyzerService(streamReader);
                var syntaxParser = new SyntaxParserService(lexAnalyzer);

                syntaxParser.Parse();
            }
            catch (Exception ex)
            {
                Print("Oops, there seems to be something wrong.\n\n", ErrorColor);
                Print(ex.Message, ErrorColor);
            }
        }

        private void TraverseAllTokens(LexicalAnalyzerService lexAnalyzer)
        {
            var count = 1L;
            var maxCount = 20;

            Console.WriteLine(Token.PRINT_HEADER);

            while (true)
            {
                var token = lexAnalyzer.GetNextToken();

                if (count % maxCount == 0)
                {
                    Console.WriteLine($"\nPage #{count / maxCount}\nPlease press enter to continue ... ");
                    Console.ReadLine();
                }

                if (token.HasError)
                {
                    Print(token, ErrorColor);
                }
                else if (token.Type == TokenType.Unknown)
                {
                    Print(token, InfoColor);
                }
                else
                {
                    Print(token, NormalColor);
                }

                count++;

                if (token.Type == TokenType.EndOfFile)
                {
                    break;
                }
            }
        }

        // 
        public static StreamReader CreateStreamReaderWith(string content)
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }

        private static void Print(object obj, ConsoleColor color)
        {
            var cc = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(obj);
            Console.ForegroundColor = cc;
        }
    }
}