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

            if (Environment.GetEnvironmentVariable("dumm", EnvironmentVariableTarget.User) == null)
            {
                var compiler = new CompilerService();
                compiler.Compile(args[0]);
            }
            else
            {
                const string FILE_PATH = @"..\..\..\Compiler.Test\test.files\";

                var testCodeFiles = new string[] {
                    $@"{FILE_PATH}\8\test0.java",
                    $@"{FILE_PATH}\8\test1.java",
                    $@"{FILE_PATH}\8\test2.java",
                    $@"{FILE_PATH}\8\test3.java",
                    $@"{FILE_PATH}\8\test4.java",
                    $@"{FILE_PATH}\7\test1.java",
                    $@"{FILE_PATH}\7\test2.java",
                    $@"{FILE_PATH}\7\test3.java",
                    $@"{FILE_PATH}\7\test4.java",
                    $@"{FILE_PATH}\7\test5.java",
                    $@"{FILE_PATH}\7\test6.java",
                    $@"{FILE_PATH}\7\test7.java",
                    $@"{FILE_PATH}\7\test8.java"
                };

                foreach (var codeFile in testCodeFiles)
                {
                    var sourceCode = System.IO.File.ReadAllText(codeFile);
                    CompilerService.CompileFile(codeFile);
                    Console.WriteLine($"\n\nPlease press enter to continue ... ");
                    Console.ReadLine();
                    Console.Clear();
                }
            }

            Console.WriteLine($"\n\nAll done.\nPlease press enter to continue ... ");
            Console.ReadLine();
        }
    }
}