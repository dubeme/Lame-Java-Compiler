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

            var compiler = new CompilerService();
            compiler.Compile(args[0]);

            Console.WriteLine($"\n\nAll done.\nPlease press enter to continue ... ");
            Console.ReadLine();
        }
    }
}