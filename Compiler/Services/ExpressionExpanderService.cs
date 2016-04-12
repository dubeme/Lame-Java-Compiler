using Compiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Services
{
    public class ExpressionExpanderService
    {
        const int NAME = 0;
        const int OPERAND1 = 1;
        const int OPERATOR = 2;
        const int OPERAND2 = 3;
        const int NEGATE_OPERAND1 = 4;
        const int NEGATE_OPERAND2 = 5;
        const int SIZE = 6;

        private List<Token> Tokens = new List<Token>();
        private Stack<Token> PostfixStack = new Stack<Token>();
        private Stack<Token> Operators = new Stack<Token>();
        private int Count;
        private static string GENERATED_NAME_PREFIX = "___t";

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
        }

        public void Reset()
        {
            Clear();
            Count = 0;
        }

        public void Evaluate()
        {
            var variable = Tokens[0];
            var assignment = Tokens[1];

            foreach (var token in Tokens.Skip(2))
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

            var result = SimplifyExpressionList(ParsePostfixStack());

            var entry = CreateEntry(
                name: variable.Lexeme,
                operand1: result.Last()[NAME]);

            result.Add(entry);

            PrintExpressionList(result);
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
            return $"{GENERATED_NAME_PREFIX}{++Count}";
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

        private static void PrintExpressionList(IList<object[]> expressionList)
        {
            System.Console.WriteLine();
            foreach (var item in expressionList)
            {
                var str = $"{item[NAME],10}";
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

                System.Console.WriteLine(str);
            }
            System.Console.WriteLine();
        }

        private static IList<object[]> SimplifyExpressionList(IList<object[]> expressionList)
        {
            return expressionList.Where(exp =>
            {
                if (exp != null)
                {
                    if (exp[NAME].ToString().StartsWith(GENERATED_NAME_PREFIX))
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
            Evaluate();
            var tab = "    ";
            var lineNumber = !PostfixStack.Any() ? -1 :
                PostfixStack.First(t => t.LineNumber != Token.UNARY_MINUS.LineNumber).LineNumber;
            return $"{tab} # {lineNumber} =>   {string.Join($" ", PostfixStack.Reverse().Select(tok => tok.Lexeme))}\n";
            // return "\n\n" + tab + string.Join($"\n{tab}", OutputStack.Reverse());
        }
    }
}