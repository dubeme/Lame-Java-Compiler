using Compiler.Models;
using Compiler.Services;
using System;
using System.IO;

namespace Compiler
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var normalColor = Console.ForegroundColor;
            var errorColor = ConsoleColor.Red;
            var infoColor = ConsoleColor.Yellow;

            if (args.Length != 1)
            {
                Console.WriteLine("Invalid usage - Usage EXECUTABLE JAVA_FILE");
                return;
            }

            try
            {
                var streamReader = new StreamReader(args[0] + "");
                var lexAnalyzer = new LexicalAnalyzerService(streamReader);
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
                            Print(token, errorColor);
                        }
                        else if (token.Type == TokenType.Unknown)
                        {
                            Print(token, infoColor);
                        }
                        else
                        {
                            Print(token, normalColor);
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
            }
            catch (Exception ex)
            {
                Print("Oops, there seems to be something wrong.\n\n", errorColor);
                Print(ex.Message, errorColor);
            }

            Console.WriteLine($"\n\nAll done.\nPlease press enter to continue ... ");
            Console.ReadLine();
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