using Compiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Services
{
    public class IntermediateCodeGeneratorService
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

        private List<Token> _Tokens = new List<Token>();
        private Stack<Token> _PostfixStack = new Stack<Token>();
        private Stack<Token> _Operators = new Stack<Token>();
        private Dictionary<string, string> _VariableLocations = new Dictionary<string, string>();


        private int _TotalTempVariableSize = 0;
        private int _VariableNameCount = 0;
        private int _BPOffset = 0;


        private static string PREFIX = "_";
        private static string GENERATED_NAME_PREFIX = $"{PREFIX}t";
        private static string RETURN_REGISTER = $"{PREFIX}AX";
        private static string BP_REGISTER = $"{PREFIX}BP";

        public int Mode { get; set; }

        public void Push(Token token)
        {
            _Tokens.Add(token);
        }

        public void Pop()
        {
            _Tokens.RemoveAt(_Tokens.Count - 1);
        }

        public void Clear()
        {
            _Tokens.Clear();
            _PostfixStack.Clear();
            _Operators.Clear();
            _VariableLocations.Clear();
            Mode = INVALID_MODE;
        }

        public void Reset()
        {
            Clear();
            _VariableNameCount = 0;
            _TotalTempVariableSize = 0;
        }

        public void GenerateIntermediateCode(Action<object> printer, Dictionary<string, string> variableLocations, int bpOffset)
        {
            _BPOffset = bpOffset;
            printer(Evaluate(variableLocations));
            // Uncomment to print postfix expresion
            // printer($"\n\n{this.ToString()}\n\n");
        }

        private string Evaluate(Dictionary<string, string> variableLocations)
        {
            if (Mode == RETURN_EXPRESSION)
            {
                ShuntYardToPostFix(_Tokens);
                var res = ParsePostfixStack();

                res.Last()[NAME] = RETURN_REGISTER;

                foreach (var item in _VariableLocations)
                {
                    variableLocations.Add(item.Key, item.Value);
                }

                return StringifyExpressionList(SimplifyExpressionList(res), variableLocations);
            }
            else if (Mode == ASSIGNMENT_VIA_METHOD_CALL)
            {
                var variableToken = _Tokens[0];
                var assignmentToken = _Tokens[1];
                var classToken = _Tokens[2];
                var methodToken = _Tokens[3];
                var parameters = _Tokens.Skip(4).Reverse();
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
                var variableToken = _Tokens[0];
                var assignmentToken = _Tokens[1];

                ShuntYardToPostFix(_Tokens.Skip(2));

                var result = SimplifyExpressionList(ParsePostfixStack());
                var entry = CreateEntry(
                    name: variableToken.Lexeme,
                    operand1: result.Last()[NAME]);

                result.Add(entry);

                foreach (var item in _VariableLocations)
                {
                    variableLocations.Add(item.Key, item.Value);
                }

                return StringifyExpressionList(result, variableLocations);
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
                    _PostfixStack.Push(token);
                }
                else if (token == Token.UNARY_MINUS)
                {
                    // Unary minus (Negation)
                    if (_Operators.Any() && _Operators.Peek() == Token.UNARY_MINUS)
                    {
                        // -- => cancels out
                        _Operators.Pop();
                    }
                    else
                    {
                        _Operators.Push(token);
                    }
                }
                else if (token.Type == TokenType.BooleanNot)
                {
                    if (_Operators.Any() && _Operators.Peek().Type == TokenType.BooleanNot)
                    {
                        // !! => cancels out
                        _Operators.Pop();
                    }
                    else
                    {
                        _Operators.Push(token);
                    }
                }
                else if (token.Type == TokenType.True || token.Type == TokenType.False)
                {
                    _PostfixStack.Push(token);
                }
                else if (token.Type == TokenType.OpenParen)
                {
                    _Operators.Push(token);
                }
                else if (token.Type == TokenType.CloseParen)
                {
                    while (_Operators.Any() && _Operators.Peek().Type != TokenType.OpenParen)
                    {
                        _PostfixStack.Push(_Operators.Pop());
                    }

                    _Operators.Pop();
                }
                else if (IsArithmeticOperator(token.Type))
                {
                    if (!_Operators.Any())
                    {
                        _Operators.Push(token);
                    }
                    else
                    {
                        if (_Operators.Any() && _Operators.Peek() == Token.UNARY_MINUS)
                        {
                            // Push unary minus onto postfix stack
                            _PostfixStack.Push(_Operators.Pop());
                        }

                        // TODO: Add support for ++, --

                        // - + has lesser precedence than * /
                        while (_Operators.Any() && IsMultDiv(_Operators.Peek().Type) && IsAddSub(token.Type))
                        {
                            _PostfixStack.Push(_Operators.Pop());
                        }

                        _Operators.Push(token);
                    }
                }
            }

            // Add remaining operators onto output queue
            while (_Operators.Any())
            {
                _PostfixStack.Push(_Operators.Pop());
            }
        }

        private IList<object[]> ParsePostfixStack()
        {
            var reverseStack = this._PostfixStack.Reverse();
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
            var name = $"{GENERATED_NAME_PREFIX}{++_VariableNameCount}";

            // Assume only 2 byte data size
            _TotalTempVariableSize += 2;
            _VariableLocations.Add(name, $"{BP_REGISTER}-{_BPOffset + _TotalTempVariableSize}");

            return name;
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

        private static string StringifyExpressionList(IList<object[]> expressionList, Dictionary<string, string> variableLocations)
        {
            var res = new StringBuilder();

            foreach (var item in expressionList)
            {
                var str = $"{GetStackBasedLocation(item[NAME], variableLocations)} = ";
                var operandStr = $"{GetStackBasedLocation(item[OPERAND1], variableLocations)}";

                // Add visual indicator for negation
                if (item[NEGATE_OPERAND1] != null && (bool)item[NEGATE_OPERAND1])
                {
                    operandStr = $"{"[" + operandStr + "]",15 }";
                }

                str = $"{str}{operandStr, 15}";

                if (item[OPERATOR] != null)
                {
                    str = $"{str}  {((Token)item[OPERATOR]).Lexeme}  ";

                    operandStr = $"{GetStackBasedLocation(item[OPERAND2], variableLocations)}";
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

        private static string GetStackBasedLocation(object obj, Dictionary<string, string> variableLocations)
        {
            var str = string.Empty;

            // If token, then it's either a number or an identifier
            if (obj is Token)
            {
                var token = (Token)obj;

                if (token.Type == TokenType.Identifier)
                {
                    str = $"{variableLocations[token.Lexeme]}";
                }
                else
                {
                    str = $"{token.Lexeme}";
                }
            }
            else if (obj is string)
            {
                var val = obj.ToString();
                var key = variableLocations.ContainsKey(val) ? variableLocations[val] : val;
                str = $"{key}";
            }

            return str;
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
            if (!_PostfixStack.Any())
            {
                return string.Empty;
            }

            var tab = "    ";
            var lineNumber = _PostfixStack.First(t => t.LineNumber != Token.UNARY_MINUS.LineNumber).LineNumber;
            return $"{tab} # {lineNumber} =>   {string.Join($" ", _PostfixStack.Reverse().Select(tok => tok.Lexeme))}\n";
            // return "\n\n" + tab + string.Join($"\n{tab}", OutputStack.Reverse());
        }
    }
}