using Compiler.Models;
using Compiler.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class Class1
    {
        public static void Main()
        {
            var lexAnalyzer = new LexicalAnalyzerService("test.txt");

            while (true)
            {
                var token = lexAnalyzer.GetNextToken();

                if (token == null)
                {
                    break;
                }

                Console.WriteLine(token);
            }
        }
    }
}
