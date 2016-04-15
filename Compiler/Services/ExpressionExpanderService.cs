using Compiler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Compiler.Services
{
    public class ExpressionExpanderService
    {
        private const int NAME = 0;
        private const int OPERAND1 = 1;
        private const int OPERATOR = 2;
        private const int OPERAND2 = 3;
        private const int NEGATE_OPERAND1 = 4;
        private const int NEGATE_OPERAND2 = 5;
        private const int SIZE = 6;

        private const int INVALID_MODE = -1;
        public const int RETURN_EXPRESSION = 0;
        public const int ASSIGNMENT = 1;
        public const int ASSIGNMENT_VIA_METHOD_CALL = 2;

        private List<Token> Tokens = new List<Token>();
        private Stack<Token> PostfixStack = new Stack<Token>();
        private Stack<Token> Operators = new Stack<Token>();
        private int VariableNameCount;

        private static string PREFIX = "_";
        private static string GENERATED_NAME_PREFIX = $"{PREFIX}t";
        private static string RETURN_REGISTER = $"{PREFIX}AX";

        public int Mode { get; set; }

        public void Push(Token token)
        {
            Tokens.Add(token);
        }

        public void Pop()
        {
            Tokens.RemoveAt(Tokens.Count - 1);
        }

        public void Clear()
        {
            Tokens.Clear();
            PostfixStack.Clear();
            Operators.Clear();
            Mode = INVALID_MODE;
        }

        public void Reset()
        {
            Clear();
            VariableNameCount = 0;
        }

        public void DumpIntermediateCode(Action<object> printer)
        {
            printer(Evaluate());
        }

        private string Evaluate()
        {
            if (Mode == RETURN_EXPRESSION)
            {
                ShuntYardToPostFix(Tokens);
                var res = ParsePostfixStack();

                res.Last()[NAME] = RETURN_REGISTER;
                return StringifyExpressionList(SimplifyExpressionList(res));
            }
            else if (Mode == ASSIGNMENT_VIA_METHOD_CALL)
            {
                var variableToken = Tokens[0];
                var assignmentToken = Tokens[1];
                var classToken = Tokens[2];
                var methodToken = Tokens[3];
                var parameters = Tokens.Skip(4).Reverse();
                var str = new StringBuilder();

                foreach (var parameter in parameters)
                {
                    str.AppendLine($"push {parameter.Lexeme}");
                }

                str.AppendLine($"call {methodToken.Lexeme}");
                str.Append($"{variableToken.Lexeme} = _AX");

                return str.ToString();

            }
            else if (Mode == ASSIGNMENT)
            {
                var variableToken = Tokens[0];
                var assignmentToken = Tokens[1];

                ShuntYardToPostFix(Tokens.Skip(2));

                var result = SimplifyExpressionList(ParsePostfixStack());
                var entry = CreateEntry(
                    name: variableToken.Lexeme,
                    operand1: result.Last()[NAME]);

                result.Add(entry);
                return StringifyExpressionList(result);
            }
            else
            {
                throw new Exception($"Invalid Mode - {Mode}");
            }
        }

        private void ShuntYardToPostFix(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                // https://en.wikipedia.org/wiki/Shunting-yard_algorithm
                if (IsNumberOrVariable(token))
                {
                    PostfixStack.Push(token);
                }
                else if (token == Token.UNARY_MINUS)
                {
                    // Unary minus (Negation)
                    if (Operators.Any() && Operators.Peek() == Token.UNARY_MINUS)
                    {
                        // -- => cancels out
                        Operators.Pop();
                    }
                    else
                    {
                        Operators.Push(token);
                    }
                }
                else if (token.Type == TokenType.BooleanNot)
                {
                    if (Operators.Any() && Operators.Peek().Type == TokenType.BooleanNot)
                    {
                        // !! => cancels out
                        Operators.Pop();
                    }
                    else
                    {
                        Operators.Push(token);
                    }
                }
                else if (token.Type == TokenType.True || token.Type == TokenType.False)
                {
                    PostfixStack.Push(token);
                }
                else if (token.Type == TokenType.OpenParen)
                {
                    Operators.Push(token);
                }
                else if (token.Type == TokenType.CloseParen)
                {
                    while (Operators.Any() && Operators.Peek().Type != TokenType.OpenParen)
                    {
                        PostfixStack.Push(Operators.Pop());
                    }

                    Operators.Pop();
                }
                else if (IsArithmeticOperator(token.Type))
                {
                    if (!Operators.Any())
                    {
                        Operators.Push(token);
                    }
                    else
                    {
                        // TODO: Add support for ++, --

                        // - + has lesser precedence than * /
                        while (Operators.Any() && IsMultDiv(Operators.Peek().Type) && IsAddSub(token.Type))
                        {
                            PostfixStack.Push(Operators.Pop());
                        }

                        Operators.Push(token);
                    }
                }
            }

            // Add remaining operators onto output queue
            while (Operators.Any())
            {
                PostfixStack.Push(Operators.Pop());
            }
        }

        private IList<object[]> ParsePostfixStack()
        {
            var reverseStack = this.PostfixStack.Reverse();
            var expressionStack = new Stack<object[]>();
            var expressionList = new List<object[]>();

            foreach (var item in reverseStack)
            {
                var entry = CreateEntry();

                if (IsNumberOrVariable(item) || IsBoolean(item))
                {
                    entry[OPERAND1] = item;

                    if (item.Type == TokenType.Identifier)
                    {
                        entry[NAME] = item.Lexeme;
                    }
                    else
                    {
                        entry[NAME] = GenerateVariableName();
                    }
                }
                else if (item == Token.UNARY_MINUS || item.Type == TokenType.BooleanNot)
                {
                    // Negation central
                    var top = expressionStack.Pop();

                    // Create a new temporary to hold the negated value
                    entry[NAME] = GenerateVariableName();
                    entry[OPERAND1] = top[NAME];
                    entry[NEGATE_OPERAND1] = true;
                }
                else
                {
                    var operand2 = expressionStack.Pop();
                    var operand1 = expressionStack.Pop();

                    entry[NAME] = GenerateVariableName();
                    entry[OPERAND1] = operand1[NAME];
                    entry[OPERATOR] = item;
                    entry[OPERAND2] = operand2[NAME];
                }

                expressionStack.Push(entry);
                expressionList.Add(entry);
            }

            if (expressionStack.Count != 1)
            {
                throw new Exception("Invalid expression");
            }

            return expressionList;
        }

        private string GenerateVariableName()
        {
            return $"{GENERATED_NAME_PREFIX}{++VariableNameCount}";
        }

        private static object[] CreateEntry(
            object name = null,
            object operand1 = null,
            object @operator = null,
            object operand2 = null,
            object negateOperand1 = null,
            object negateOperand2 = null)
        {
            var entry = new object[SIZE];

            entry[NAME] = name;
            entry[OPERAND1] = operand1;
            entry[OPERATOR] = @operator;
            entry[OPERAND2] = operand2;
            entry[NEGATE_OPERAND1] = negateOperand1;
            entry[NEGATE_OPERAND2] = negateOperand2;

            return entry;
        }

        private static string StringifyExpressionList(IList<object[]> expressionList)
        {
            var res = new StringBuilder();

            foreach (var item in expressionList)
            {
                var str = $"{item[NAME],16} = ";
                var operandStr = "";

                if (item[OPERAND1] is Token)
                {
                    operandStr = $"{((Token)item[OPERAND1]).Lexeme}";
                }
                else
                {
                    operandStr = $"{item[OPERAND1]}";
                }

                // Add visual indicator for negation
                if (item[NEGATE_OPERAND1] != null && (bool)item[NEGATE_OPERAND1])
                {
                    operandStr = $"{"[" + operandStr + "]",15 }";
                }
                else
                {
                    operandStr = $"{operandStr,15 }";
                }

                str = $"{str}{operandStr}";

                if (item[OPERATOR] != null)
                {
                    str = $"{str}  {((Token)item[OPERATOR]).Lexeme}  ";

                    if (item[OPERAND2] is string)
                    {
                        operandStr = $"{item[OPERAND2]}";
                    }
                    else
                    {
                        operandStr = $"{((Token)item[OPERAND2]).Lexeme}";
                    }

                    // Add visual indicator for negation
                    if (item[NEGATE_OPERAND2] != null && (bool)item[NEGATE_OPERAND2])
                    {
                        operandStr = $"[{operandStr}]";
                    }

                    str = $"{str}{operandStr}";
                }

                res.AppendLine(str);
            }

            return res.ToString().TrimEnd();
        }

        private static IList<object[]> SimplifyExpressionList(IList<object[]> expressionList)
        {
            return expressionList.Where(exp =>
            {
                if (exp != null)
                {
                    if (exp[NAME].ToString().StartsWith(PREFIX))
                    {
                        return true;
                    }
                }
                return false;
            }).ToList();
        }

        private static bool IsNumberOrVariable(Token token)
        {
            return
                token.Type == TokenType.LiteralInteger ||
                token.Type == TokenType.LiteralReal ||
                token.Type == TokenType.Identifier;
        }

        private static bool IsBoolean(Token item)
        {
            return item.Type == TokenType.True || item.Type == TokenType.False;
        }

        private static bool IsAddSub(TokenType type)
        {
            return type == TokenType.Plus || type == TokenType.Minus;
        }

        private static bool IsMultDiv(TokenType type)
        {
            return type == TokenType.Multiplication || type == TokenType.Divide;
        }

        private static bool IsArithmeticOperator(TokenType type)
        {
            return
                type == TokenType.Multiplication ||
                type == TokenType.Divide ||
                type == TokenType.Plus ||
                type == TokenType.Minus;
        }

        public override string ToString()
        {
            if (!PostfixStack.Any())
            {
                return string.Empty;
            }

            var tab = "    ";
            var lineNumber = PostfixStack.First(t => t.LineNumber != Token.UNARY_MINUS.LineNumber).LineNumber;
            return $"{tab} # {lineNumber} =>   {string.Join($" ", PostfixStack.Reverse().Select(tok => tok.Lexeme))}\n";
            // return "\n\n" + tab + string.Join($"\n{tab}", OutputStack.Reverse());
        }
    }
}