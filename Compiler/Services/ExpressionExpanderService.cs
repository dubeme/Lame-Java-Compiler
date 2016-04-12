using Compiler.Models;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Services
{
    public class ExpressionExpanderService
    {
        private List<Token> Tokens = new List<Token>();
        private Queue<Token> OutputQueue = new Queue<Token>();
        private Stack<Token> Operators = new Stack<Token>();
        private Stack<bool> OpenParenNegation = new Stack<bool>();
        private Token NegativeOne = Token.CreateToken("-1", -1);

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
            OutputQueue.Clear();
            Operators.Clear();
        }

        public void Evaluate()
        {
            var lastTokenWasArithmethicOperator = false;

            foreach (var token in Tokens.Skip(2))
            {
                // https://en.wikipedia.org/wiki/Shunting-yard_algorithm
                if (IsNumberOrVariable(token))
                {
                    OutputQueue.Enqueue(token);
                }
                else if (IsArithmeticOperator(token.Type))
                {
                    if (!Operators.Any())
                    {
                        // If first operator is -, and nothing in the OutputQueue
                        if (token.Type == TokenType.Minus && !OutputQueue.Any())
                        {
                            // Negate, NegativeOne will tell the parser to negate the next number
                            OutputQueue.Enqueue(NegativeOne);
                        }
                        else
                        {
                            Operators.Push(token);
                        }
                    }
                    else if (token.Type == TokenType.Minus && lastTokenWasArithmethicOperator)
                    {
                        // Negate, NegativeOne will tell the parser to negate the next number
                        OutputQueue.Enqueue(NegativeOne);
                    }
                    else if (token.Type == TokenType.Plus && lastTokenWasArithmethicOperator)
                    {
                        // Ignore
                    }
                    else
                    {
                        // TODO: Add support for ++, --

                        // - + has lesser precedence than * /
                        while (IsMultDiv(Operators.Peek().Type) && IsAddSub(token.Type))
                        {
                            OutputQueue.Enqueue(Operators.Pop());
                        }

                        Operators.Push(token);
                    }
                }
                else if (token.Type == TokenType.OpenParen)
                {
                    Operators.Push(token);
                }
                else if (token.Type == TokenType.CloseParen)
                {
                    while (Operators.Peek().Type != TokenType.OpenParen)
                    {
                        OutputQueue.Enqueue(Operators.Pop());
                    }

                    Operators.Pop();
                }

                lastTokenWasArithmethicOperator = IsArithmeticOperator(token.Type);
            }

            // Add remaining operators onto output queue
            while (Operators.Any())
            {
                OutputQueue.Enqueue(Operators.Pop());
            }
        }

        private static bool IsNumberOrVariable(Token currentToken)
        {
            return
                currentToken.Type == TokenType.LiteralInteger ||
                currentToken.Type == TokenType.LiteralReal ||
                currentToken.Type == TokenType.Identifier;
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
            return "\n\n" + tab + string.Join($"\n{tab}", OutputQueue);
        }
    }
}