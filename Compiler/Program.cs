using Compiler.Models;
using Compiler.Services;
using System;

namespace Compiler
{
    internal class Program
    {
        public static void Main()
        {
            var lexAnalyzer = new LexicalAnalyzerService("test.txt");
            var count = 1l;
            var maxCount = 20;

            Console.WriteLine(Token.PRINT_HEADER);

            while (true)
            {
                try
                {
                    var token = lexAnalyzer.GetNextToken();

                    if (token == null)
                    {
                        break;
                    }

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
                    else
                    {
                        Console.WriteLine(token);
                    }
                    
                    count++;
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