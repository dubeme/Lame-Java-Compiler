using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Compiler.Services
{
    public class Intelx86GeneratorService
    {
        private const int TAB_INDENT = -33;
        private const string PROC = "proc";
        private const string ENDP = "endp";
        private const string CALL = "call";
        private const string PUSH = "push";

        public static void Generate(
            string[] tacCodeLines, 
            Dictionary<string, string> globalStrings,
            Dictionary<string, int> methodLocalBytes,
            Dictionary<string, int> methodParameterBytes,
            Action<string> printer)
        {
            PrintTopMetadata(globalStrings, printer);
            ThreeAdressCodeTox86(tacCodeLines, methodLocalBytes, methodParameterBytes, printer);
        }

        private static void PrintTopMetadata(Dictionary<string, string> globalStrings, Action<string> printer)
        {
            printer($@"{"", TAB_INDENT}.model small");
            printer($@"{"", TAB_INDENT}.586");
            printer($@"{"", TAB_INDENT}.stack 100h");
            printer($@"{"", TAB_INDENT}.data");

            if (globalStrings.Any())
            {
                foreach (var item in globalStrings)
                {
                    printer($@"{item.Key,TAB_INDENT}DB {item.Value}, ""$""");
                }
            }

            printer($@"{"", TAB_INDENT}.code");
            printer($@"{"", TAB_INDENT}include io.asm");
        }

        private static void ThreeAdressCodeTox86(
            string[] tacCodeLines,
            Dictionary<string, int> methodLocalBytes,
            Dictionary<string, int> methodParameterBytes,
            Action<string> printer)
        {
            foreach (var line in tacCodeLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var temp = Regex.Split(line.Trim(), "[ ]+");

                if (temp[0] == PROC)
                {
                    printer($@"{temp[1],TAB_INDENT}proc");
                    printer($@"{"",TAB_INDENT}push bp");
                    printer($@"{"",TAB_INDENT}mov bp, sp");
                    printer($@"{"",TAB_INDENT}sub sp, {methodLocalBytes[temp[1]]}");
                }
                else if (temp[0] == ENDP)
                {
                    printer($@"{"",TAB_INDENT}add sp, {methodLocalBytes[temp[1]]}");
                    printer($@"{"",TAB_INDENT}pop bp");
                    printer($@"{"",TAB_INDENT}ret {methodParameterBytes[temp[1]]}");
                    printer($@"{temp[1],TAB_INDENT}endp");
                }
                else if (temp[0] == CALL)
                {
                    printer($@"{"",TAB_INDENT}call {temp[1]}");
                }
                else if (temp[0] == PUSH)
                {
                    printer($@"{"",TAB_INDENT}push {temp[1]}");
                }

            }
        }
    }
}
