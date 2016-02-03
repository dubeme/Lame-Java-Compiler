using Compiler.Models;
using Compiler.Services;
using System;

namespace Compiler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid usage - Usage EXECUTABLE JAVA_FILE");
                return;
            }

            var lexAnalyzer = new LexicalAnalyzerService(args[0]);
            var count = 1L;
            var maxCount = 20;

            Console.WriteLine(Token.PRINT_HEADER);

            while (true)
            {
                try
                {
                    var token = lexAnalyzer.GetNextToken();

                    if (count % maxCount == 0)
                    {
                        Console.WriteLine($"\nPage #{count / maxCount}\nPlease press enter to continue ... ");
                        Console.ReadLine();
                    }

                    if (token.HasError)
                    {
                        var cc = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(token);
                        Console.ForegroundColor = cc;
                    }
                    else if (token.Type == TokenType.Unknown)
                    {
                        var cc = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(token);
                        Console.ForegroundColor = cc;
                    }
                    else
                    {
                        Console.WriteLine(token);
                    }
                    
                    count++;

                    if (token.Type == TokenType.EndOfFile)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Console.WriteLine($"\n\nAll done.\nPlease press enter to continue ... ");
            Console.ReadLine();
        }
    }
}