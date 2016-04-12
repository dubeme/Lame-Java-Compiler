using Compiler.Models;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Services
{
    public class ExpressionExpanderService
    {
        private List<Token> Tokens = new List<Token>();
        private Queue<KeyValuePair<Token, bool>> OutputQueue = new Queue<KeyValuePair<Token, bool>>();
        private Stack<Token> Operators = new Stack<Token>();
        private Stack<bool> OpenParenNegation = new Stack<bool>();
        Token NE = Token.CreateToken("-1", -1);

        public void Push(Token token)
        {
            var lastTOken = Tokens.Last();

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
                    OutputQueue.Enqueue(new KeyValuePair<Token, bool>(token, negateNextNumber));

                    if (negateNextNumber)
                    {
                        negateNextNumber = false;
                    }
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
                        OutputQueue.Enqueue(new KeyValuePair<Token, bool>(token, negateNextNumber));
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
                            OutputQueue.Enqueue(new KeyValuePair<Token, bool>(Operators.Pop(), false));
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
                        OutputQueue.Enqueue(new KeyValuePair<Token, bool>(Operators.Pop(), false));
                    }

                    Operators.Pop();
                }

                lastTokenWasArithmethicOperator = IsArithmeticOperator(token.Type);
            }

            // Add remaining operators onto output queue
            while (Operators.Any())
            {
                OutputQueue.Enqueue(new KeyValuePair<Token, bool>(Operators.Pop(), false));
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