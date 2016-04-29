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
        private const string COMMENT = ";";
        private static string MOV = "mov";

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

            PrintNewLine(printer);
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
                        printer($@"{"",TAB_INDENT}mov AX, @data");
                        printer($@"{"",TAB_INDENT}mov DS, AX");
                    }
                    else
                    {
                        printer($@"{methodName,TAB_INDENT}proc");
                        printer($@"{"",TAB_INDENT}push BP");
                        printer($@"{"",TAB_INDENT}mov BP, SP");
                        printer($@"{"",TAB_INDENT}sub SP, {methodLocalBytes[methodName]}");
                    }
                    PrintNewLine(printer);
                }
                else if (temp[0] == ENDP)
                {
                    var methodName = temp[1];

                    if (methodName == MAIN)
                    {
                        printer($@"{"",TAB_INDENT}mov AH, 04ch");
                        printer($@"{"",TAB_INDENT}int 21h");
                        printer($@"{"start",TAB_INDENT}endp");
                        printer($@"{"",TAB_INDENT}end start");
                    }
                    else
                    {
                        printer($@"{"",TAB_INDENT}add SP, {methodLocalBytes[methodName]}");
                        printer($@"{"",TAB_INDENT}pop BP");

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
                    PrintNewLine(printer);
                }
                else if (temp[0] == PUSH)
                {
                    if (temp[1].StartsWith(IntermediateCodeGeneratorService.BP_REGISTER))
                    {
                        printer($@"{"",TAB_INDENT}push [BP{GetBPOffset(temp[1])}]");
                    }
                    else
                    {
                        printer($@"{"",TAB_INDENT}push {temp[1]}");
                    }
                }
                else if (temp[0] == MOV)
                {
                    //var dst = temp[1];
                    //var src = temp[2];

                    //if (src.StartsWith(IntermediateCodeGeneratorService.BP_REGISTER))
                    //{
                    //    src = $"[BP{GetBPOffset(src)}]";
                    //}

                    //printer($@"{"",TAB_INDENT}mov {dst}, {src}");

                    printer($@"{"",TAB_INDENT}{line.Trim()}");
                }
                else if (temp.Length == JUST_ASSIGNMENT)
                {
                    var leftHand = temp[0];
                    var operand1 = temp[2].TrimStart('-');
                    var negateOperand1 = temp[2].StartsWith("-");

                    printer($"{"",TAB_INDENT}{COMMENT}{leftHand} = {operand1}");
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

                    if (!leftHand.StartsWith(IntermediateCodeGeneratorService.RETURN_REGISTER))
                    {
                        // If not return statement
                        printer($@"{"",TAB_INDENT}mov [BP{GetBPOffset(leftHand)}], AX");
                    }

                    PrintNewLine(printer);
                }
                else if (temp.Length == OPERATION)
                {
                    var lh = temp[0];
                    var oper1 = temp[2];
                    var @operator = temp[3];
                    var oper2 = temp[4];

                    printer($"{"",TAB_INDENT}{COMMENT}{lh} = {oper1} {@operator} {oper2}");
                    
                    if (@operator == "/")
                    {
                        printer($@"{"",TAB_INDENT}mov DX, 0    ; Clear high dividend");
                    }

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
                        printer($@"{"",TAB_INDENT}mov BX, {oper2}");
                        printer($@"{"",TAB_INDENT}imul BX");
                    }
                    else if (@operator == "/")
                    {
                        printer($@"{"",TAB_INDENT}mov BX, {oper2}");
                        printer($@"{"",TAB_INDENT}idiv BX");
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

                    if (!lh.StartsWith(IntermediateCodeGeneratorService.RETURN_REGISTER))
                    {
                        // If not return statement
                        printer($@"{"",TAB_INDENT}mov [BP{GetBPOffset(lh)}], AX");
                    }

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