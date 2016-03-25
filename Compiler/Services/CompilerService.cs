﻿using Compiler.Models;
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
                var symbolTable = new SymbolTable
                {
                    Printer = (val) =>
                    {
                        Console.WriteLine(val);
                    }
                };

                var syntaxParser = new SyntaxParserService(lexAnalyzer, symbolTable);

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

        public static void Test()
        {
            var vals = new String[]{
                @"final class Main {public static void main(String[] args){}}",

                @"class tim { int x, y, z;}
                final class Main {public static void main(String[] args){}}",

                @"class tim {
                    int x, y, z;
                    public void ass(int a, int b, int c){ return;}
                }
                final class Main {public static void main(String[] args){}}",

                @"class one {} class two{}
                final class Main
                {
                    public static void main(String[] args)
                    {
                    }
                }",

                @"class One
                {
                    int erer;
                    final int kl = 2;
                    public void func(){ return ;}
                }
                class Two
                {
                    int a;
                    boolean b;
                } final class Main {
                    public static void main(String[] args)
                    {
                    }
                }"
            };

            foreach (var str in vals)
            {
                try
                {
                    var memStream = new MemoryStream(Encoding.UTF8.GetBytes(str));
                    var streamReader = new StreamReader(memStream);
                    var lexAnalyzer = new LexicalAnalyzerService(streamReader);
                    var symbolTable = new SymbolTable
                    {
                        Printer = Console.WriteLine
                    };

                    var syntaxParser = new SyntaxParserService(lexAnalyzer, symbolTable);

                    syntaxParser.Parse();
                }
                catch (Exception ex)
                {
                    Print("Oops, there seems to be something wrong.\n\n", ErrorColor);
                    Print(ex.Message, ErrorColor);
                }
            }
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