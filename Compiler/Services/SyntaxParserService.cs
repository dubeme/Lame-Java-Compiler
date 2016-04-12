using Compiler.Models;
using Compiler.Models.Exceptions;
using Compiler.Models.Misc;
using Compiler.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Services
{
    public class SyntaxParserService
    {
        private LexicalAnalyzerService LexicalAnalyzer;
        private SymbolTable SymbolTable;

        private Token _CurrentToken;

        private ExpressionExpanderService ExpressionExpander = new ExpressionExpanderService();

        private Token CurrentToken
        {
            get
            {
                if (_CurrentToken == null)
                {
                    throw new Exception("No more token found");
                }

                return _CurrentToken;
            }
            set
            {
                _CurrentToken = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether current token is closed curly braces OR return statement.
        /// </summary>
        public bool EndOfCodeBlock
        {
            get
            {
                return CurrentToken.Type == TokenType.CloseCurlyBrace || CurrentToken.Type == TokenType.Return;
            }
        }

        private int Depth;
        private int Offset;
        private MethodEntry CurrentMethod;
        private ClassEntry CurrentClass;
        private TokenType CurrentDataType;
        private VariableScope CurrentVariableScope;
        private Stack<string> ProductionStack = new Stack<string>();

        public SyntaxParserService(LexicalAnalyzerService lexAnalyzer, SymbolTable symbolTable)
        {
            this.LexicalAnalyzer = lexAnalyzer;
            this.SymbolTable = symbolTable;
        }

        public void Parse()
        {
            try
            {
                Program();
            }
            catch (Exception ex)
            {
                var errMessage = $"Error parsing token on" +
                    $" line #{LexicalAnalyzer.LineNumber}," +
                    $" Column #{LexicalAnalyzer.Column}.\n\n" +
                    $" {ex.Message}\n\n" +
                    $" Production Path => \n\n{GetProductionPath()}\n\n";

                throw new Exception(errMessage, ex);
            }
        }

        private void Program()
        {
            PushProduction("Program");
            SetNextToken();

            Depth = 0;
            Offset = 0;

            // Program -> MoreClasses MainClass
            MoreClasses();
            MainClass();

            PerformScopeExitAction();

            this.SetNextToken();

            if (this.CurrentToken.Type != TokenType.EndOfFile)
            {
                throw new Exception("Didn't reach end of file, but the program grammar is done matching");
            }
            PopProduction();
        }

        private void MoreClasses()
        {
            // MoreClasses -> class idt ExtendsClass { VariableDeclaration MethodDeclaration } MoreClasses | ε

            PushProduction("MoreClasses");

            if (CurrentToken.Type != TokenType.Class)
            {
                PopProduction();
                return;
            }

            SetNextToken();

            // Match class name, then insert into symbol table
            var classIdentifier = CurrentToken;
            MatchAndSetToken(TokenType.Identifier);
            InsertClass(classIdentifier);

            ExtendsClass();
            MatchAndSetToken(TokenType.OpenCurlyBrace);

            // Update values
            Depth++;

            // reset offset
            Offset = 0;

            CurrentVariableScope = VariableScope.ClassBody;
            VariableDeclaration();

            CurrentVariableScope = VariableScope.MethodBody;
            MethodDeclaration();

            MatchAndSetToken(TokenType.CloseCurlyBrace);

            // Exit current scope
            PerformScopeExitAction();

            MoreClasses();
            PopProduction();
        }

        private void ExtendsClass()
        {
            // ExtendsClass	->	extendst idt | ε

            PushProduction("ClassDeclaration");

            // If an extends is matched
            if (this.CurrentToken.Type == TokenType.Extends)
            {
                SetNextToken();
                MatchAndSetToken(TokenType.Identifier);
            }
            PopProduction();
        }

        private void VariableDeclaration()
        {
            // VariableDeclaration -> Type IdentifierList ; VariableDeclaration | finalt Type idt = numt; VariableDeclaration | ε

            PushProduction("VariableDeclaration");

            if (CurrentToken.Type == TokenType.Final)
            {
                SetNextToken();

                var dataType = CurrentToken.Type;
                Type();

                var identifier = CurrentToken;
                MatchAndSetToken(TokenType.Identifier);
                MatchAndSetToken(TokenType.Assignment);

                var value = CurrentToken.Lexeme;
                if (dataType == TokenType.Boolean)
                {
                    MatchAndSetToken(TokenGroup.ReservedWord);
                }
                else
                {
                    MatchAndSetToken(TokenGroup.Literal);
                }

                InsertVariable(dataType, identifier, true, value);
            }
            else
            {
                try
                {
                    CurrentDataType = CurrentToken.Type;
                    Type();
                }
                catch (Exception)
                {
                    // The ε case.
                    // Pop Type production
                    PopProduction();
                    // Pop VaraibleDeclaration production
                    PopProduction();
                    return;
                }

                IdentifierList();
            }
            MatchAndSetToken(TokenType.Semicolon);
            VariableDeclaration();
            PopProduction();
        }

        private void IdentifierList()
        {
            // IdentifierList -> idt | IdentifierList , idt

            var identifier = CurrentToken;
            PushProduction("IdentifierList");

            MatchAndSetToken(TokenType.Identifier);
            InsertVariable(CurrentDataType, identifier);

            if (CurrentToken.Type == TokenType.Comma)
            {
                SetNextToken();
                IdentifierList();
            }
            PopProduction();
        }

        private void Type()
        {
            // Type -> intt | booleant |voidt | floatt

            PushProduction("Type");

            switch (CurrentToken.Type)
            {
                case TokenType.Int:
                case TokenType.Void:
                case TokenType.Boolean:
                case TokenType.Float:
                    SetNextToken();
                    break;

                default:
                    throw new MissingTokenException("int|boolean|void|float", this.CurrentToken.Type.ToString(), ProductionStack.Peek());
            }
            PopProduction();
        }

        private void MethodDeclaration()
        {
            // MethodDeclaration -> publict Type idt (FormalParameterList) { VariableDeclaration SequenceofStatements returnt Expression ; } MethodDeclaration | ε

            PushProduction("MethodDeclaration");

            if (CurrentToken.Type == TokenType.Public)
            {
                SetNextToken();

                var returnType = CurrentToken.Type;
                Type();

                var identifier = CurrentToken;
                MatchAndSetToken(TokenType.Identifier);

                // Insert method in symbol table
                InsertMethod(returnType, identifier);

                // update depth
                Depth++;

                MatchAndSetToken(TokenType.OpenParen);

                CurrentVariableScope = VariableScope.MethodParameter;
                FormalParameterList();

                MatchAndSetToken(TokenType.CloseParen);
                MatchAndSetToken(TokenType.OpenCurlyBrace);

                // reset offset
                Offset = 0;
                CurrentVariableScope = VariableScope.MethodBody;

                VariableDeclaration();
                SequenceofStatements();

                if (returnType == TokenType.Void && CurrentToken.Type == TokenType.Return)
                {
                    MatchAndSetToken(TokenType.Return);
                }
                else
                {
                    MatchAndSetToken(TokenType.Return);
                    Expression();
                }

                MatchAndSetToken(TokenType.Semicolon);
                MatchAndSetToken(TokenType.CloseCurlyBrace);

                // Exit current scope
                PerformScopeExitAction();

                MethodDeclaration();
            }
            PopProduction();
        }

        private void FormalParameterList()
        {
            // FormalParameterList -> Type idt RestOfFormalParameterList | ε

            var dataType = CurrentToken.Type;
            PushProduction("FormalParameterList");

            try
            {
                Type();
            }
            catch (Exception)
            {
                // ε case
                PopProduction();
                PopProduction();
                return;
            }

            var identifier = CurrentToken;
            MatchAndSetToken(TokenType.Identifier);

            InsertVariable(dataType, identifier);
            RestOfFormalParameterList();
            PopProduction();
        }

        private void RestOfFormalParameterList()
        {
            // RestOfFormalParameterList -> , Type idt RestOfFormalParameterList | ε

            PushProduction("RestOfFormalParameterList");

            if (CurrentToken.Type == TokenType.Comma)
            {
                SetNextToken();
                var dataType = CurrentToken.Type;

                Type();

                var identifier = CurrentToken;

                MatchAndSetToken(TokenType.Identifier);

                InsertVariable(dataType, identifier);

                RestOfFormalParameterList();
            }
            PopProduction();
        }

        private void SequenceofStatements()
        {
            // SequenceOfStatements -> Statement ; StatementTail | ε
            PushProduction("SequenceOfStatements");

            // Sequence of statements exists when not at the end of a code block
            if (EndOfCodeBlock)
            {
                // ε production
                PopProduction();
                return;
            }

            Statement();
            MatchAndSetToken(TokenType.Semicolon);

            Console.WriteLine(ExpressionExpander);
            ExpressionExpander.Clear();

            StatementTail();
            PopProduction();
        }

        private void StatementTail()
        {
            // StatementTail -> Statement ; StatementTail | ε

            PushProduction("StatementTail");

            // Sequence of statements exists when not at the end of a code block
            if (EndOfCodeBlock)
            {
                // ε production
                PopProduction();
                return;
            }

            Statement();
            MatchAndSetToken(TokenType.Semicolon);

            Console.WriteLine(ExpressionExpander);
            ExpressionExpander.Clear();

            StatementTail();
            PopProduction();
        }

        private void Statement()
        {
            // Statement -> AssignmentStatement | IOStatement

            PushProduction("Statement");

            if (CurrentToken.Type == TokenType.Identifier)
            {
                AssignmentStatement();
            }
            else
            {
                IOStatement();
            }
            PopProduction();
        }

        private void AssignmentStatement()
        {
            // AssignmentStatement -> idt = Expression

            PushProduction("AssignmentStatement");
            var identifier = CurrentToken.Lexeme;

            ExpressionExpander.Push(CurrentToken);
            MatchAndSetToken(TokenType.Identifier);

            if (SymbolTable.Lookup(identifier) == null)
            {
                throw new UndeclaredVariableException(identifier);
            }

            ExpressionExpander.Push(CurrentToken);
            MatchAndSetToken(TokenType.Assignment);
            Expression();
            PopProduction();
        }

        private void IOStatement()
        {
            // IOStatement -> ε
            PushProduction("IOStatement");
            PopProduction();
        }

        private void Expression()
        {
            // Expression -> Relation | ε
            PushProduction("Expression");
            try
            {
                Relation();
            }
            catch (MissingOptionalTokenException)
            {
                // ε production
                PopProduction();
                PopProduction();
                return;
            }
            PopProduction();
        }

        private void Relation()
        {
            // Relation -> SimpleExpression
            PushProduction("Relation");
            SimpleExpression();
            PopProduction();
        }

        private void SimpleExpression()
        {
            // SimpleExpression -> Term MoreTerm
            PushProduction("SimpleExpression");
            Term();
            MoreTerm();
            PopProduction();
        }

        private void Term()
        {
            // Term -> Factor MoreFactor
            PushProduction("Term");
            Factor();
            MoreFactor();
            PopProduction();
        }

        private void MoreTerm()
        {
            // MoreTerm -> AddOperators Term MoreTerm | ε
            PushProduction("MoreTerm");
            try
            {
                ExpressionExpander.Push(CurrentToken);
                AddOperators();
            }
            catch (MissingOptionalTokenException)
            {
                // ε production
                PopProduction();
                PopProduction();
                ExpressionExpander.Pop();
                return;
            }

            Term();
            MoreTerm();
            PopProduction();
        }

        private void Factor()
        {
            // Factor -> id | num | ( Expression ) | SignOperator Factor | ! BooleanFactor | true | false

            var currentTokenType = CurrentToken.Type;
            ExpressionExpander.Push(CurrentToken);

            PushProduction("Factor");

            if (CurrentToken.Type == TokenType.Identifier)
            {
                var identifier = CurrentToken.Lexeme;

                MatchAndSetToken(TokenType.Identifier);

                if (SymbolTable.Lookup(identifier) == null)
                {
                    throw new UndeclaredVariableException(identifier);
                }
            }
            else if (currentTokenType == TokenType.LiteralInteger)
            {
                MatchAndSetToken(TokenType.LiteralInteger);
            }
            else if (currentTokenType == TokenType.LiteralReal)
            {
                MatchAndSetToken(TokenType.LiteralReal);
            }
            else if (currentTokenType == TokenType.OpenParen)
            {
                MatchAndSetToken(TokenType.OpenParen);
                Expression();
                ExpressionExpander.Push(CurrentToken);
                MatchAndSetToken(TokenType.CloseParen);
            }
            else if (currentTokenType == TokenType.BooleanNot)
            {
                MatchAndSetToken(TokenType.BooleanNot);
                BooleanFactor();
            }
            else if (currentTokenType == TokenType.True)
            {
                MatchAndSetToken(TokenType.True);
            }
            else if (currentTokenType == TokenType.False)
            {
                MatchAndSetToken(TokenType.False);
            }
            else if (currentTokenType == TokenType.Minus)
            {
                ExpressionExpander.Pop();
                ExpressionExpander.Push(Token.UNARY_MINUS);
                SignOperator();
                Factor();
            }
            else
            {
                throw new Exception($"Unexpected token {CurrentToken.Type}, found in production ({ProductionStack.Peek()}).");
            }

            PopProduction();
        }

        private void BooleanFactor()
        {
            // BooleanFactor -> ! BooleanFactor | ( BooleanFactor ) | true | false | id
            PushProduction("BooleanFactor");
            ExpressionExpander.Push(CurrentToken);

            if (CurrentToken.Type == TokenType.BooleanNot)
            {
                MatchAndSetToken(TokenType.BooleanNot);
                BooleanFactor();
            }
            else if (CurrentToken.Type == TokenType.OpenParen)
            {
                MatchAndSetToken(TokenType.OpenParen);
                BooleanFactor();
                ExpressionExpander.Push(CurrentToken);
                MatchAndSetToken(TokenType.CloseParen);
            }
            else if (CurrentToken.Type == TokenType.True)
            {
                MatchAndSetToken(TokenType.True);
            }
            else if (CurrentToken.Type == TokenType.False)
            {
                MatchAndSetToken(TokenType.False);
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                var identifier = CurrentToken.Lexeme;

                MatchAndSetToken(TokenType.Identifier);

                var entry = SymbolTable.Lookup(identifier);

                if (entry == null)
                {
                    throw new UndeclaredVariableException(identifier);
                }

                if (((VariableEntry)entry.Content).DataType != VariableType.Boolean)
                {
                    throw new Exception($"variable [{identifier}] has to be of boolean type.");
                }
            }
            else
            {
                throw new Exception($"Expected boolean terminal symbol, but found {CurrentToken.Type} in production ({ProductionStack.Peek()}).");
            }

            PopProduction();
        }

        private void MoreFactor()
        {
            // MoreFactor -> MultiplicationOperators Factor MoreFactor | ε
            PushProduction("MoreFactor");

            try
            {
                ExpressionExpander.Push(CurrentToken);
                MultiplicationOperators();
            }
            catch (MissingOptionalTokenException)
            {
                // ε production
                PopProduction();
                PopProduction();
                ExpressionExpander.Pop();
                return;
            }

            Factor();
            MoreFactor();
            PopProduction();
        }

        private void AddOperators()
        {
            // AddOperators -> + | - | ||
            PushProduction("AddOperators");
            SetTokenIfAnyMatch(TokenType.Plus, TokenType.Minus, TokenType.BooleanOr);
            PopProduction();
        }

        private void MultiplicationOperators()
        {
            // MultiplicationOperators -> * | / | &&
            PushProduction("MultiplicationOperators");
            SetTokenIfAnyMatch(TokenType.Multiplication, TokenType.Divide, TokenType.BooleanAnd);
            PopProduction();
        }

        private void SignOperator()
        {
            // SignOperator -> -
            PushProduction("SignOperator");
            MatchAndSetToken(TokenType.Minus);
            PopProduction();
        }

        private void MainClass()
        {
            // MainClass -> finalt classt idt { publict statict voidt main (String [] idt) {SequenceofStatements }}

            PushProduction("MainClass");

            MatchAndSetToken(TokenType.Final);
            MatchAndSetToken(TokenType.Class);

            var identifier = CurrentToken;

            MatchAndSetToken(TokenType.Identifier);
            InsertClass(identifier);

            MatchAndSetToken(TokenType.OpenCurlyBrace);

            Depth++;
            MatchAndSetToken(TokenType.Public);
            MatchAndSetToken(TokenType.Static);
            MatchAndSetToken(TokenType.Void);

            identifier = CurrentToken;
            MatchAndSetToken(TokenType.Main);
            InsertMethod(TokenType.Void, identifier);

            MatchAndSetToken(TokenType.OpenParen);
            MatchAndSetToken(TokenType.String);
            MatchAndSetToken(TokenType.OpenSquareBracket);
            MatchAndSetToken(TokenType.CloseSquareBracket);
            MatchAndSetToken(TokenType.Identifier);

            // For course, skipping command line arguments (above)
            MatchAndSetToken(TokenType.CloseParen);
            MatchAndSetToken(TokenType.OpenCurlyBrace);

            Depth++;
            SequenceofStatements();

            MatchAndSetToken(TokenType.CloseCurlyBrace);

            PerformScopeExitAction();

            MatchAndSetToken(TokenType.CloseCurlyBrace);

            PerformScopeExitAction();
            PopProduction();
        }

        private void PushProduction(string production)
        {
            ProductionStack.Push(production);
        }

        private void PopProduction()
        {
            ProductionStack.Pop();
        }

        private string GetProductionPath()
        {
            var tab = "    ";
            return tab + string.Join($"\n{tab}", ProductionStack.Reverse());
        }

        private void SetNextToken()
        {
            this.CurrentToken = this.LexicalAnalyzer.GetNextToken();
        }

        private void SetTokenIfAnyMatch(params TokenType[] types)
        {
            if (types == null || types.Length < 1)
            {
                return;
            }

            foreach (var type in types)
            {
                if (CurrentToken.Type == type)
                {
                    SetNextToken();
                    return;
                }
            }

            var expected = string.Join(" | ", types);
            throw new MissingOptionalTokenException(expected, CurrentToken.Type.ToString(), ProductionStack.Peek());
        }

        private void MatchAndSetToken(TokenType expectedType)
        {
            if (this.CurrentToken.Type != expectedType)
            {
                throw new MissingTokenException(expectedType, this.CurrentToken.Type, ProductionStack.Peek());
            }

            SetNextToken();
        }

        private void MatchAndSetToken(TokenGroup expectedGroup)
        {
            if (this.CurrentToken.Group != expectedGroup)
            {
                throw new MissingTokenException(expectedGroup, this.CurrentToken.Group, ProductionStack.Peek());
            }

            SetNextToken();
        }

        private void InsertVariable(TokenType dataType, Token identifier, bool isConstant = false, string value = "")
        {
            // Get type
            var _dataType = GetDataType(dataType);
            var _size = GetDataTypeSize(_dataType);
            var _offset = 0;
            var _scope = CurrentVariableScope;

            if (_scope == VariableScope.MethodParameter)
            {
                var paramType = new Models.Table.LinkedListNode<VariableType> { Value = _dataType };
                paramType.Next = CurrentMethod.ParameterTypes;
                CurrentMethod.ParameterTypes = paramType;

                CurrentMethod.NumberOfParameters++;
            }
            else if (_scope == VariableScope.ClassBody)
            {
                var field = new Models.Table.LinkedListNode<string> { Value = identifier.Lexeme };
                field.Next = CurrentClass.Fields;
                CurrentClass.Fields = field;

                CurrentClass.SizeOfLocal += _size;
                _offset = Offset;
                Offset += _size;
            }
            else if (_scope == VariableScope.MethodBody)
            {
                CurrentMethod.SizeOfLocal += _size;
                _offset = Offset;
                Offset += _size;
            }

            // Insert formal parameter
            SymbolTable.Insert(identifier, Depth);

            var entry = SymbolTable.Lookup(identifier.Lexeme);

            if (isConstant)
            {
                entry.Type = EntryType.Constant;
                entry.Content = new ConstantEntry
                {
                    DataType = _dataType,
                    Offset = _offset,
                    Value = value
                };
            }
            else
            {
                entry.Type = EntryType.Variable;
                entry.Content = new VariableEntry
                {
                    DataType = _dataType,
                    Offset = _offset,
                    Size = _size
                };
            }
        }

        private void InsertMethod(TokenType returnType, Token identifier)
        {
            // Get type
            var _returnType = GetDataType(returnType);

            CurrentMethod = new MethodEntry
            {
                NumberOfParameters = 0,
                ParameterTypes = null,
                ReturnType = _returnType,
                SizeOfLocal = 0
            };

            // Insert formal parameter
            SymbolTable.Insert(identifier, Depth);

            var entry = SymbolTable.Lookup(identifier.Lexeme);
            entry.Type = EntryType.Function;
            entry.Content = CurrentMethod;

            // Add method name to class
            var methodName = new Models.Table.LinkedListNode<string> { Value = identifier.Lexeme };
            methodName.Next = CurrentClass.MethodNames;
            CurrentClass.MethodNames = methodName;
        }

        private void InsertClass(Token identifier)
        {
            CurrentClass = new ClassEntry
            {
                MethodNames = null,
                Fields = null,
                SizeOfLocal = 0
            };

            SymbolTable.Insert(identifier, Depth);

            var entry = SymbolTable.Lookup(identifier.Lexeme);
            entry.Type = EntryType.Class;
            entry.Content = CurrentClass;
        }

        private VariableType GetDataType(TokenType type)
        {
            switch (type)
            {
                case TokenType.Char: return VariableType.Char;
                case TokenType.Float: return VariableType.Float;
                case TokenType.Int: return VariableType.Int;
                case TokenType.Boolean: return VariableType.Boolean;
                case TokenType.Void: return VariableType.Void;
            }

            return VariableType.Unkown;
        }

        private int GetDataTypeSize(VariableType type)
        {
            switch (type)
            {
                case VariableType.Char: return 1;
                case VariableType.Float: return 4;
                case VariableType.Int: return 2;
                case VariableType.Boolean: return 1;
            }

            return -1;
        }

        private void PerformScopeExitAction()
        {
            Console.WriteLine();
            Console.WriteLine($"Dumping entries at depth [{Depth}]\n");
            SymbolTable.WriteTable(Depth);
            SymbolTable.DeleteDepth(Depth);
            Depth--;
        }
    }
}