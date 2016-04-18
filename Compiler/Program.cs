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
                    $@"{FILE_PATH}\more\test1.java",
                    $@"{FILE_PATH}\more\test2.java",
                    $@"{FILE_PATH}\more\test3.java",
                    $@"{FILE_PATH}\more\test4.java",
                    $@"{FILE_PATH}\more\test5.java",
                    $@"{FILE_PATH}\more\test6.java",
                    $@"{FILE_PATH}\more\test7.java",
                    $@"{FILE_PATH}\more\test8.java",
                    $@"{FILE_PATH}justMain.java",
                    $@"{FILE_PATH}main1Class.java",
                    $@"{FILE_PATH}main2Classes.java",
                    $@"{FILE_PATH}classWithFields.java",
                    $@"{FILE_PATH}classWithFieldsAndMethod.java",
                    $@"{FILE_PATH}classWithFieldsAndMethods.java",
                    $@"{FILE_PATH}classWithFieldsMethodsParameters.java",
                    $@"{FILE_PATH}classWithFieldsMethodsParametersAndLocals.java",
                    $@"{FILE_PATH}classWithConstant.java",
                    $@"{FILE_PATH}classWithExpression.java",
                    $@"{FILE_PATH}classWithExpressionBasic.java",
                    $@"{FILE_PATH}classWithExpressionSimpleArithmetic.java",
                    $@"{FILE_PATH}classWithExpressionBoolean.java",
                    $@"{FILE_PATH}classWithExpressionPemdas.java",
                    $@"{FILE_PATH}classWithExpressionUndeclared1.java",
                    $@"{FILE_PATH}classWithExpressionUndeclared2.java"
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