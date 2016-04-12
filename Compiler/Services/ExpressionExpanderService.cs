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
        private Stack<bool> OpenParenNegation = new Stack<bool>();

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
            return "\n\n" + tab + string.Join($"\n{tab}", OutputStack.Reverse());
        }
    }
}