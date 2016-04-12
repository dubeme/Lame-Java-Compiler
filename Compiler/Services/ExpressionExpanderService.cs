using Compiler.Models;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Services
{
    public class ExpressionExpanderService
    {
        private List<Token> Tokens = new List<Token>();
        private Stack<Token> OutputStack = new Stack<Token>();
        private Stack<Token> Operators = new Stack<Token>();
        private int Count;

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
            OutputStack.Clear();
            Operators.Clear();
            Count = 0;
        }

        public void Evaluate()
        {
            foreach (var token in Tokens.Skip(2))
            {
                // https://en.wikipedia.org/wiki/Shunting-yard_algorithm
                if (IsNumberOrVariable(token))
                {
                    OutputStack.Push(token);
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
                    OutputStack.Push(token);
                }
                else if (token.Type == TokenType.OpenParen)
                {
                    Operators.Push(token);
                }
                else if (token.Type == TokenType.CloseParen)
                {
                    while (Operators.Any() && Operators.Peek().Type != TokenType.OpenParen)
                    {
                        OutputStack.Push(Operators.Pop());
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
                            OutputStack.Push(Operators.Pop());
                        }

                        Operators.Push(token);
                    }
                }
            }

            // Add remaining operators onto output queue
            while (Operators.Any())
            {
                OutputStack.Push(Operators.Pop());
            }

            Parse();
        }

        private string GenerateVariableName()
        {
            return $"_t{++Count}";
        }

        private void Parse()
        {
            const int TEMP_IDENTIFIER = 0;
            const int OPERAND1 = 1;
            const int OPERATOR = 2;
            const int OPERAND2 = 3;
            const int NEGATE_OPERAND1 = 4;
            const int NEGATE_OPERAND2 = 5;
            const int SIZE = 6;

            var reverseStack = this.OutputStack.Reverse();
            var expressionStack = new Stack<object[]>();
            var expressionList = new List<object[]>();

            foreach (var item in reverseStack)
            {
                var entry = new object[SIZE];
                if (IsNumberOrVariable(item) || IsBoolean(item))
                {
                    entry[OPERAND1] = item;

                    if (item.Type == TokenType.Identifier)
                    {
                        entry[TEMP_IDENTIFIER] = item.Lexeme;
                    }
                    else
                    {
                        entry[TEMP_IDENTIFIER] = GenerateVariableName();
                    }
                }
                else if (item == Token.UNARY_MINUS || item.Type == TokenType.BooleanNot)
                {
                    // Negation central
                    var top = expressionStack.Pop();

                    // Create a new temporary to hold the negated value
                    entry[TEMP_IDENTIFIER] = GenerateVariableName();
                    entry[OPERAND1] = top[TEMP_IDENTIFIER];
                    entry[NEGATE_OPERAND1] = true;
                }
                else
                {
                    var operand2 = expressionStack.Pop();
                    var operand1 = expressionStack.Pop();

                    entry[TEMP_IDENTIFIER] = GenerateVariableName();
                    entry[OPERAND1] = operand1[TEMP_IDENTIFIER];
                    entry[OPERATOR] = item;
                    entry[OPERAND2] = operand2[TEMP_IDENTIFIER];
                }

                expressionStack.Push(entry);
                expressionList.Add(entry);
            }

            PrintExpressionList(TEMP_IDENTIFIER, OPERAND1, OPERATOR, OPERAND2, NEGATE_OPERAND1, NEGATE_OPERAND2, expressionList);
        }

        private static void PrintExpressionList(int TEMP_IDENTIFIER, int OPERAND1, int OPERATOR, int OPERAND2, int NEGATE_OPERAND1, int NEGATE_OPERAND2, List<object[]> expressionList)
        {
            System.Console.WriteLine();
            foreach (var item in expressionList)
            {
                var str = $"{item[TEMP_IDENTIFIER],10}";
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
            var lineNumber = !OutputStack.Any() ? -1 :
                OutputStack.First(t => t.LineNumber != Token.UNARY_MINUS.LineNumber).LineNumber;
            return $"{tab} # {lineNumber} =>   {string.Join($" ", OutputStack.Reverse().Select(tok => tok.Lexeme))}\n";
            // return "\n\n" + tab + string.Join($"\n{tab}", OutputStack.Reverse());
        }
    }
}