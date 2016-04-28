using Compiler.Models;
using System;
using System.IO;

namespace Compiler.Services
{
    public class CompilerService
    {
        private static ConsoleColor NormalColor = Console.ForegroundColor;
        private static ConsoleColor ErrorColor = ConsoleColor.Red;
        private static ConsoleColor InfoColor = ConsoleColor.Yellow;

        private static TextWriter FileIn;

        public void Compile(string fileName)
        {
            var parent = Directory.GetParent(fileName);
            var dir = Directory.CreateDirectory($"output_{parent.Name}");

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var tacFilePath = $"{dir.FullName}/{fileNameWithoutExtension}.tac";
            var asmFilePath = $"{dir.FullName}/{fileNameWithoutExtension}.asm";
            try
            {
                FileIn = File.CreateText(tacFilePath);
                var streamReader = new StreamReader(fileName);
                var lexAnalyzer = new LexicalAnalyzerService(streamReader);
                var symbolTable = new SymbolTable
                {
                    Printer = (val) =>
                    {
                        Console.WriteLine(val);
                    }
                };

                var syntaxParser = new SyntaxParserService(lexAnalyzer, symbolTable);

                PrintSourceCode(File.ReadAllText(fileName));
                syntaxParser.Parse();
                FileIn.Close();

                Intelx86GeneratorService.Generate(
                    File.ReadAllLines(tacFilePath),
                    syntaxParser.GlobalStrings,
                    syntaxParser.MethodLocalSize,
                    syntaxParser.MethodParamSize,
                    (str) => {
                        if (File.Exists(asmFilePath))
                        {
                            File.Delete(asmFilePath);
                        }

                        File.AppendAllText(asmFilePath, str);
                        Console.WriteLine(str);
                    });

            }
            catch (Exception ex)
            {
                // Delete out file
                if (File.Exists(tacFilePath))
                {
                    FileIn.Close();
                    File.Delete(tacFilePath);
                }

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

        public static void CompileFile(string fileName)
        {
            var compiler = new CompilerService();
            compiler.Compile(fileName);
        }

        public static void PrintSourceCode(string sourceCode)
        {
            if (sourceCode == null)
            {
                return;
            }

            var lines = sourceCode.Split('\n');
            var lineNumber = 1;

            foreach (var line in lines)
            {
                Print($"{lineNumber,6}| {line}", ConsoleColor.Cyan);
                lineNumber++;
            }

            Print("\n", ConsoleColor.White);
        }

        private static void Print(object obj, ConsoleColor color, bool newLine = true)
        {
            var cc = Console.ForegroundColor;
            Console.ForegroundColor = color;

            if (newLine)
            {
                Console.WriteLine(obj);
            }
            else
            {
                Console.Write(obj);
            }
            Console.ForegroundColor = cc;
        }

        public static void PrintToFile(object obj, bool newLine = true)
        {
            if (obj != null && FileIn != null)
            {
                if (newLine)
                {
                    FileIn.WriteLine(obj);
                }
                else
                {
                    FileIn.Write(obj);
                }
            }
        }
    }
}