using Compiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Services
{
    public class ExpressionExpanderService
    {
        private List<Token> Tokens = new List<Token>();
        private Stack<Token> OutputStack = new Stack<Token>();
        private Stack<Token> Operators = new Stack<Token>();
        private Stack<bool> OpenParenNegation = new Stack<bool>();
        private int Count;

        public void Push(Token token)
        {
            // var lastTOken = Tokens.Last();

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
                    OutputStack.Push(token);
                }
                else if (token.Type == TokenType.BooleanNot)
                {
                    if (OutputStack.Any() && OutputStack.Peek().Type == TokenType.BooleanNot)
                    {
                        // !! => cancels out
                        OutputStack.Pop();
                    }
                    else
                    {
                        OutputStack.Push(token);
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

        private string GenerateName()
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
                if (IsNumberOrVariable(item))
                {
                    var entry = new object[SIZE];

                    if (item.Type == TokenType.Identifier)
                    {
                        entry[TEMP_IDENTIFIER] = item.Lexeme;
                    }
                    else
                    {
                        entry[TEMP_IDENTIFIER] = GenerateName();
                    }

                    entry[OPERAND1] = item;

                    expressionStack.Push(entry);
                    expressionList.Add(entry);
                }
                else if (item == Token.UNARY_MINUS)
                {
                    // Don't add to the list, since the effect will be handled on te stack
                    var entry = new object[SIZE];

                    entry[TEMP_IDENTIFIER] = Token.UNARY_MINUS.Lexeme;
                    entry[OPERAND1] = Token.UNARY_MINUS;

                    expressionStack.Push(entry);
                }
                else
                {
                    var entry = new object[SIZE];
                    var operand2 = expressionStack.Pop();

                    if (expressionStack.Any())
                    {
                        if ((string)expressionStack.Peek()[TEMP_IDENTIFIER] == Token.UNARY_MINUS.Lexeme)
                        {
                            entry[NEGATE_OPERAND2] = true;
                            expressionStack.Pop();
                        }
                    }

                    var operand1 = expressionStack.Pop();

                    if (expressionStack.Any() && expressionStack.Count == 1)
                    {
                        // For the operand1 only use check the unary minus when only one item is left
                        // If last item not unary, then there must be another operator
                        if ((string)expressionStack.Peek()[TEMP_IDENTIFIER] == Token.UNARY_MINUS.Lexeme)
                        {
                            entry[NEGATE_OPERAND1] = true;
                            expressionStack.Pop();
                        }
                    }

                    entry[TEMP_IDENTIFIER] = GenerateName();
                    entry[OPERAND1] = operand1[TEMP_IDENTIFIER];
                    entry[OPERATOR] = item;
                    entry[OPERAND2] = operand2[TEMP_IDENTIFIER];

                    expressionStack.Push(entry);
                    expressionList.Add(entry);
                }
            }

            // Check whether to negate final expression

            var lastExpression = expressionStack.Pop();

            if (expressionStack.Any())
            {
                if ((string)expressionStack.Peek()[TEMP_IDENTIFIER] == Token.UNARY_MINUS.Lexeme)
                {
                    lastExpression[NEGATE_OPERAND1] = true;
                    expressionStack.Pop();

                    // Add the negated final expression
                    expressionList.Add(lastExpression);
                }
                else
                {
                    throw new Exception("Invalid expression");
                }
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
                    operandStr = $"{"[" + operandStr + "]", 15 }";
                }
                else
                {
                    operandStr = $"{operandStr, 15 }";
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

        private bool IsAddSub(TokenType type)
        {
            return type == TokenType.Plus || type == TokenType.Minus;
        }

        private bool IsMultDiv(TokenType type)
        {
            return type == TokenType.Multiplication || type == TokenType.Divide;
        }

        private bool IsArithmeticOperator(TokenType type)
        {
            return
                type == TokenType.Multiplication ||
                type == TokenType.Divide ||
                type == TokenType.Plus ||
                type == TokenType.Minus;
        }

        public override string ToString()
        {
            var tab = "    ";
            Evaluate();
            return $"{tab} # {OutputStack.Peek().LineNumber,-6} =>   {string.Join($" ", OutputStack.Reverse().Select(tok => tok.Lexeme))}\n";
            // return "\n\n" + tab + string.Join($"\n{tab}", OutputStack.Reverse());
        }
    }
}