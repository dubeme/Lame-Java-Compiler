using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Compiler.Services
{
    public class Intelx86GeneratorService
    {
        private const int TAB_INDENT = -33;
        private const string PROC = "proc";
        private const string ENDP = "endp";
        private const string CALL = "call";
        private const string PUSH = "push";
        private const string MAIN = "main";
        private static int OPERATION = 5;
        private static int JUST_ASSIGNMENT = 3;

        public static void Generate(
            string[] tacCodeLines,
            Dictionary<string, string> globalStrings,
            Dictionary<string, int> methodLocalBytes,
            Dictionary<string, int> methodParameterBytes,
            Action<string> printer)
        {
            StringBuilder buffer = new StringBuilder();
            Action<string> bufferer = (str) =>
            {
                buffer.AppendLine(str);
            };

            PrintTopMetadata(globalStrings, bufferer);
            ThreeAdressCodeTox86(tacCodeLines, methodLocalBytes, methodParameterBytes, bufferer);

            printer(buffer.ToString());
        }

        private static void PrintTopMetadata(Dictionary<string, string> globalStrings, Action<string> printer)
        {
            printer($@"{"",TAB_INDENT}.model small");
            printer($@"{"",TAB_INDENT}.586");
            printer($@"{"",TAB_INDENT}.stack 100h");
            printer($@"{"",TAB_INDENT}.data");

            if (globalStrings.Any())
            {
                foreach (var item in globalStrings)
                {
                    printer($@"{item.Key,TAB_INDENT}DB {item.Value}, ""$""");
                }
            }

            printer($@"{"",TAB_INDENT}.code");
            printer($@"{"",TAB_INDENT}include io.asm");
        }

        private static void ThreeAdressCodeTox86(
            string[] tacCodeLines,
            Dictionary<string, int> methodLocalBytes,
            Dictionary<string, int> methodParameterBytes,
            Action<string> printer)
        {
            Func<string, string> GetBPOffset = (str) =>
            {
                return Regex.Replace(str, $"^({IntermediateCodeGeneratorService.BP_REGISTER})", "");
            };

            foreach (var line in tacCodeLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var temp = Regex.Split(line.Trim(), "[ ]+");

                if (temp[0] == PROC)
                {
                    var methodName = temp[1];

                    if (methodName == MAIN)
                    {
                        printer($@"{"start",TAB_INDENT}proc");
                        printer($@"{"",TAB_INDENT}mov ax, @data");
                        printer($@"{"",TAB_INDENT}mov bs, ax");
                    }
                    else
                    {
                        printer($@"{methodName,TAB_INDENT}proc");
                        printer($@"{"",TAB_INDENT}push bp");
                        printer($@"{"",TAB_INDENT}mov bp, sp");
                        printer($@"{"",TAB_INDENT}sub sp, {methodLocalBytes[methodName]}");
                    }
                    PrintNewLine(printer);
                }
                else if (temp[0] == ENDP)
                {
                    var methodName = temp[1];

                    if (methodName == MAIN)
                    {
                        printer($@"{"",TAB_INDENT}mov ah, 04ch");
                        printer($@"{"",TAB_INDENT}int 21h");
                        printer($@"{"start",TAB_INDENT}endp");
                        printer($@"{"",TAB_INDENT}end start");
                    }
                    else
                    {
                        printer($@"{"",TAB_INDENT}add sp, {methodLocalBytes[methodName]}");
                        printer($@"{"",TAB_INDENT}pop bp");

                        if (methodParameterBytes[methodName] == 0)
                        {
                            printer($@"{"",TAB_INDENT}ret");
                        }
                        else
                        {
                            printer($@"{"",TAB_INDENT}ret {methodParameterBytes[methodName]}");
                        }

                        printer($@"{methodName,TAB_INDENT}endp");
                    }

                    PrintNewLine(printer);
                }
                else if (temp[0] == CALL)
                {
                    printer($@"{"",TAB_INDENT}call {temp[1]}");
                }
                else if (temp[0] == PUSH)
                {
                    printer($@"{"",TAB_INDENT}push {temp[1]}");
                }
                else if (temp.Length == JUST_ASSIGNMENT)
                {
                    var leftHand = temp[0];
                    var operand1 = temp[2].TrimStart('-');
                    var negateOperand1 = temp[2].StartsWith("-");

                    if (operand1.StartsWith(IntermediateCodeGeneratorService.BP_REGISTER))
                    {
                        printer($@"{"",TAB_INDENT}mov AX, [BP{GetBPOffset(operand1)}]");

                        if (negateOperand1)
                        {
                            printer($@"{"",TAB_INDENT}neg AX");
                        }
                    }
                    else
                    {
                        printer($@"{"",TAB_INDENT}mov AX, {operand1}");
                    }

                    printer($@"{"",TAB_INDENT}mov [BP{GetBPOffset(leftHand)}], AX    #{leftHand} = {operand1}");
                    PrintNewLine(printer);
                }
                else if (temp.Length == OPERATION)
                {
                    var lh = temp[0];
                    var oper1 = temp[2];
                    var @operator = temp[3];
                    var oper2 = temp[4];

                    if (oper1.StartsWith(IntermediateCodeGeneratorService.BP_REGISTER))
                    {
                        printer($@"{"",TAB_INDENT}mov AX, [BP{GetBPOffset(oper1)}]");
                    }
                    else
                    {
                        printer($@"{"",TAB_INDENT}mov AX, {oper1}");
                    }

                    if (oper2.StartsWith(IntermediateCodeGeneratorService.BP_REGISTER))
                    {
                        oper2 = $"[BP{GetBPOffset(oper2)}]";
                    }

                    if (@operator == "*")
                    {
                        printer($@"{"",TAB_INDENT}mul AX, {oper2}");
                    }
                    else if (@operator == "/")
                    {
                        printer($@"{"",TAB_INDENT}div AX, {oper2}");
                    }
                    else if (@operator == "+")
                    {
                        printer($@"{"",TAB_INDENT}add AX, {oper2}");
                    }
                    else if (@operator == "-")
                    {
                        printer($@"{"",TAB_INDENT}sub AX, {oper2}");
                    }
                    else
                    {
                        throw new Exception($"Invalid operation {@operator}");
                    }

                    printer($@"{"",TAB_INDENT}mov [BP{GetBPOffset(lh)}], AX    #{lh} = {oper1} {@operator} {temp[4]}");
                    PrintNewLine(printer);
                }
                else
                {
                    throw new Exception("Invalid Three address code format");
                }
            }
        }

        private static void PrintNewLine(Action<string> printer)
        {
            printer("");
        }
    }
}