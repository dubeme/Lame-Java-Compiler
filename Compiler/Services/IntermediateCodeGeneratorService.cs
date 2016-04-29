using Compiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Compiler.Services
{
    /// <summary>
    ///
    /// </summary>
    public class IntermediateCodeGeneratorService
    {
        /// <summary>
        /// The name
        /// </summary>
        private const int NAME = 0;

        /// <summary>
        /// The operan d1
        /// </summary>
        private const int OPERAND1 = 1;

        /// <summary>
        /// The operator
        /// </summary>
        private const int OPERATOR = 2;

        /// <summary>
        /// The operan d2
        /// </summary>
        private const int OPERAND2 = 3;

        /// <summary>
        /// The negat e_ operan d1
        /// </summary>
        private const int NEGATE_OPERAND1 = 4;

        /// <summary>
        /// The negat e_ operan d2
        /// </summary>
        private const int NEGATE_OPERAND2 = 5;

        /// <summary>
        /// The size
        /// </summary>
        private const int SIZE = 6;

        /// <summary>
        /// The invali d_ mode
        /// </summary>
        private const int INVALID_MODE = -1;

        /// <summary>
        /// The retur n_ expression
        /// </summary>
        public const int RETURN_EXPRESSION = 0;

        /// <summary>
        /// The assignment
        /// </summary>
        public const int ASSIGNMENT = 1;

        /// <summary>
        /// The assignmen t_ vi a_ metho d_ call
        /// </summary>
        public const int ASSIGNMENT_VIA_METHOD_CALL = 2;

        /// <summary>
        /// The metho d_ call
        /// </summary>
        public const int METHOD_CALL = 3;

        /// <summary>
        /// The IO method call
        /// </summary>
        public const int IO_METHOD_CALL = 4;

        /// <summary>
        /// The _ tokens
        /// </summary>
        private List<Token> _Tokens = new List<Token>();

        /// <summary>
        /// The _ postfix stack
        /// </summary>
        private Stack<Token> _PostfixStack = new Stack<Token>();

        /// <summary>
        /// The _ operators
        /// </summary>
        private Stack<Token> _Operators = new Stack<Token>();

        /// <summary>
        /// The _ variable locations
        /// </summary>
        private Dictionary<string, string> _VariableLocations = new Dictionary<string, string>();

        /// <summary>
        /// Gets the total size of the temporary variable.
        /// </summary>
        public int TotalTempVariableSize { get; private set; }

        /// <summary>
        /// The _ variable name count
        /// </summary>
        private int _VariableNameCount = 0;

        /// <summary>
        /// The _ bp offset
        /// </summary>
        private int _BPOffset = 0;

        private const string READ = "read";
        private const string WRITE = "write";
        private const string WRITE_LN = "writeln";
        private const string WRITE_INT = "writeint";
        private const string WRITE_STR = "writestr";
        private const string READ_INT = "readint";

        /// <summary>
        /// The prefix
        /// </summary>
        private static string PREFIX = "_";

        /// <summary>
        /// The generate d_ nam e_ prefix
        /// </summary>
        private static string GENERATED_NAME_PREFIX = $"{PREFIX}t";

        /// <summary>
        /// The retur n_ register
        /// </summary>
        public readonly static string RETURN_REGISTER = $"{PREFIX}AX";

        /// <summary>
        /// The b p_ register
        /// </summary>
        public readonly static string BP_REGISTER = $"{PREFIX}BP";

        /// <summary>
        /// Gets or sets the mode of this IntermediateCodeGeneratorService.
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        /// Pushes the specified token.
        /// </summary>
        /// <param name="token">The token.</param>
        public void Push(Token token)
        {
            _Tokens.Add(token);
        }

        /// <summary>
        /// Pops this instance.
        /// </summary>
        public void Pop()
        {
            _Tokens.RemoveAt(_Tokens.Count - 1);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _Tokens.Clear();
            _PostfixStack.Clear();
            _Operators.Clear();
            _VariableLocations.Clear();
            Mode = INVALID_MODE;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            Clear();
            _VariableNameCount = 0;
            TotalTempVariableSize = 0;
        }

        /// <summary>
        /// Generates the intermediate code.
        /// </summary>
        /// <param name="printer">The printer.</param>
        /// <param name="variableLocations">The variable locations.</param>
        /// <param name="bpOffset">The bp offset.</param>
        public void GenerateIntermediateCode(Action<object> printer, Dictionary<string, string> variableLocations, int bpOffset)
        {
            _BPOffset = bpOffset;
            printer(Evaluate(variableLocations));
            // Uncomment to print postfix expresion
            // printer($"\n\n{this.ToString()}\n\n
        }

        /// <summary>
        /// Evaluates the specified variable locations.
        /// </summary>
        /// <param name="variableLocations">The variable locations.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private string Evaluate(Dictionary<string, string> variableLocations)
        {
            Func<string, string> getStackAddress = (string variableName) =>
            {
                if (variableLocations.ContainsKey(variableName))
                {
                    return variableLocations[variableName];
                }

                throw new Exception($"{variableName} isn't allocated on the stack");
            };

            Func<string, string> GetBPOffset = (str) =>
            {
                var bpOffset = Regex.Replace(getStackAddress(str), $"^({BP_REGISTER})", "");
                return $"[BP{bpOffset}]";
            };

            if (Mode == RETURN_EXPRESSION)
            {
                ShuntYardToPostFix(_Tokens);
                var res = ParsePostfixStack();

                // Last item holds result
                // it always result in VAR = VAR
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
                    if (parameter.Type == TokenType.Identifier)
                    {
                        str.AppendLine($"push {GetBPOffset(parameter.Lexeme)}");
                    }
                    else
                    {
                        str.AppendLine($"push {parameter.Lexeme}");
                    }
                }

                str.AppendLine($"call {methodToken.Lexeme}");
                str.Append($"{getStackAddress(variableToken.Lexeme)} = AX");

                return str.ToString();
            }
            else if (Mode == METHOD_CALL)
            {
                var classToken = _Tokens[0];
                var methodToken = _Tokens[1];
                var parameters = _Tokens.Skip(2).Reverse();
                var str = new StringBuilder();

                foreach (var parameter in parameters)
                {
                    if (parameter.Type == TokenType.Identifier)
                    {
                        str.AppendLine($"push {GetBPOffset(parameter.Lexeme)}");
                    }
                    else
                    {
                        str.AppendLine($"push {parameter.Lexeme}");
                    }
                }

                str.Append($"call {methodToken.Lexeme}");

                return str.ToString();
            }
            else if (Mode == IO_METHOD_CALL)
            {
                var methodToken = _Tokens[0];
                var parameter = _Tokens[1];
                var output = new StringBuilder();

                if (parameter.Type == TokenType.LiteralString)
                {
                    if (methodToken.Lexeme == READ)
                    {
                        // Not supported yet: Read string
                        // str.Append($"call {methodToken.Lexeme}");
                    }
                    else
                    {
                        // Strings are in the data section
                        output.AppendLine($"mov DX, offset {getStackAddress(parameter.Lexeme)}");

                        if (methodToken.Lexeme == WRITE)
                        {
                            output.Append($"call {WRITE_STR}");
                        }
                        else
                        {
                            output.AppendLine($"call {WRITE_STR}");
                            output.Append($"call {WRITE_LN}");
                        }
                    }
                }
                else
                {
                    if (methodToken.Lexeme == READ)
                    {
                        output.AppendLine($"call {READ_INT}");
                        output.Append($"mov {GetBPOffset(parameter.Lexeme)}, BX");
                    }
                    else
                    {
                        output.AppendLine($"mov BX, {GetBPOffset(parameter.Lexeme)}");

                        if (methodToken.Lexeme == WRITE)
                        {
                            output.Append($"call {WRITE_INT}");
                        }
                        else
                        {
                            output.AppendLine($"call {WRITE_INT}");
                            output.Append($"call {WRITE_LN}");
                        }
                    }
                }

                return output.ToString();
            }
            else if (Mode == ASSIGNMENT)
            {
                var variableToken = _Tokens[0];
                var assignmentToken = _Tokens[1];

                ShuntYardToPostFix(_Tokens.Skip(2));

                var parsedPostfixStack = ParsePostfixStack();
                var result = SimplifyExpressionList(parsedPostfixStack);

                // The last item on the list will contain the result
                // Hence why the assignment to the entry
                // LEFT_HAND_VARIABLE = LAST_ITEM_ON_LIST
                // parsedPostfixStack can't be empty since the assumption is
                // that a proper assignment expression is provided
                var entry = CreateEntry(
                    name: variableToken.Lexeme,
                    operand1: parsedPostfixStack.Last()[NAME]);

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

        /// <summary>
        /// Shunts the yard to post fix.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
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

        /// <summary>
        /// Parses the postfix stack.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Invalid expression</exception>
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

        /// <summary>
        /// Generates the name of the variable.
        /// </summary>
        /// <returns></returns>
        private string GenerateVariableName()
        {
            var name = $"{GENERATED_NAME_PREFIX}{++_VariableNameCount}";

            // Assume only 2 byte data size
            TotalTempVariableSize += 2;
            _VariableLocations.Add(name, $"{BP_REGISTER}-{_BPOffset + TotalTempVariableSize}");

            return name;
        }

        /// <summary>
        /// Creates the entry.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="operand1">The operand1.</param>
        /// <param name="operator">The operator.</param>
        /// <param name="operand2">The operand2.</param>
        /// <param name="negateOperand1">The negate operand1.</param>
        /// <param name="negateOperand2">The negate operand2.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Stringifies the expression list.
        /// </summary>
        /// <param name="expressionList">The expression list.</param>
        /// <param name="variableLocations">The variable locations.</param>
        /// <returns></returns>
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
                    operandStr = $"{"-" + operandStr,15 }";
                }

                str = $"{str}{operandStr,15}";

                if (item[OPERATOR] != null)
                {
                    str = $"{str}  {((Token)item[OPERATOR]).Lexeme}  ";

                    operandStr = $"{GetStackBasedLocation(item[OPERAND2], variableLocations)}";
                    // Add visual indicator for negation
                    if (item[NEGATE_OPERAND2] != null && (bool)item[NEGATE_OPERAND2])
                    {
                        operandStr = $"-{operandStr}";
                    }

                    str = $"{str}{operandStr}";
                }

                res.AppendLine(str);
            }

            return res.ToString().TrimEnd();
        }

        /// <summary>
        /// Gets the stack based location.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="variableLocations">The variable locations.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Simplifies the expression list.
        /// </summary>
        /// <param name="expressionList">The expression list.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Determines whether [is number or variable] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static bool IsNumberOrVariable(Token token)
        {
            return
                token.Type == TokenType.LiteralInteger ||
                token.Type == TokenType.LiteralReal ||
                token.Type == TokenType.Identifier;
        }

        /// <summary>
        /// Determines whether the specified item is boolean.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private static bool IsBoolean(Token item)
        {
            return item.Type == TokenType.True || item.Type == TokenType.False;
        }

        /// <summary>
        /// Determines whether [is add sub] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool IsAddSub(TokenType type)
        {
            return type == TokenType.Plus || type == TokenType.Minus;
        }

        /// <summary>
        /// Determines whether [is mult div] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool IsMultDiv(TokenType type)
        {
            return type == TokenType.Multiplication || type == TokenType.Divide;
        }

        /// <summary>
        /// Determines whether [is arithmetic operator] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static bool IsArithmeticOperator(TokenType type)
        {
            return
                type == TokenType.Multiplication ||
                type == TokenType.Divide ||
                type == TokenType.Plus ||
                type == TokenType.Minus;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
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