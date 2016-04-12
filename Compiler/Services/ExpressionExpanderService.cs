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
            OutputQueue.Clear();
            Operators.Clear();
        }

        public void Evaluate()
        {
            var lastTokenWasArithmethicOperator = false;
            var negateNextNumber = false;

            foreach (var token in Tokens.Skip(2))
            {
                // https://en.wikipedia.org/wiki/Shunting-yard_algorithm
                if (IsNumberOrVariable(token))
                {
                    if (negateNextNumber)
                    {
                        negateNextNumber = false;
                    }
                    OutputQueue.Enqueue(token);
                }
                else if (IsArithmeticOperator(token.Type))
                {
                    if (!Operators.Any())
                    {
                        Operators.Push(token);
                    }
                    else if (token.Type == TokenType.Minus && lastTokenWasArithmethicOperator)
                    {
                        // Negate
                        negateNextNumber = true;
                    }
                    else
                    {
                        // - + has lesser precedence than * /
                        // TODO: Add support for ++, --
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