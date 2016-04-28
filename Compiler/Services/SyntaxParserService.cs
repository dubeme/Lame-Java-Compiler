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

        private IntermediateCodeGeneratorService ExpressionExpander = new IntermediateCodeGeneratorService();

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

        public Dictionary<string, string> GlobalStrings { get; set; }
        public Dictionary<string, int> MethodLocalSize { get; set; }
        public Dictionary<string, int> MethodParamSize { get; set; }

        private int Depth;
        private int Offset;
        private MethodEntry CurrentMethod;
        private ClassEntry CurrentClass;
        private TokenType CurrentDataType;
        private VariableScope CurrentVariableScope;
        private Stack<string> ProductionStack = new Stack<string>();
        private Dictionary<string, string> VariableLocation = new Dictionary<string, string>();
        private int BPOffset = 0;
        private const int RETURN_ADDRESS_OLD_BP_OFFSET = 2 + 2;
        private const string BP = "_BP";

        private const string READ = "read";
        private const string WRITE = "write";
        private const string WRITE_LN = "writeln";

        public SyntaxParserService(LexicalAnalyzerService lexAnalyzer, SymbolTable symbolTable)
        {
            this.LexicalAnalyzer = lexAnalyzer;
            this.SymbolTable = symbolTable;
            this.GlobalStrings = new Dictionary<string, string>();
            MethodParamSize = new Dictionary<string, int>();
            MethodLocalSize = new Dictionary<string, int>();
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

                if (CurrentVariableScope == VariableScope.MethodBody)
                {
                    // Not handling class body
                    // For constants, push their value since it is know at compile time,
                    // there's no need to add it to the stack

                    // BPOffset += GetDataTypeSize(GetDataType(dataType));
                    VariableLocation.Add(identifier.Lexeme, $"{value}");
                }
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

            if (CurrentVariableScope == VariableScope.MethodBody)
            {
                // Not handling class body

                BPOffset += GetDataTypeSize(GetDataType(CurrentDataType));
                VariableLocation.Add(identifier.Lexeme, $"{BP}-{BPOffset}");
            }

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

                var methodIdentifierToken = CurrentToken;
                MatchAndSetToken(TokenType.Identifier);

                // Insert method in symbol table
                InsertMethod(returnType, methodIdentifierToken);

                // Print intermediate code for procedure
                IntermediateCodePrinter($"proc {methodIdentifierToken.Lexeme}", false);

                // Reset expression expander
                ExpressionExpander.Reset();

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

                SetupParameterStackLocation();

                VariableDeclaration();
                SequenceofStatements();

                if (returnType == TokenType.Void && CurrentToken.Type == TokenType.Return)
                {
                    MatchAndSetToken(TokenType.Return);
                }
                else
                {
                    ExpressionExpander.Mode = IntermediateCodeGeneratorService.RETURN_EXPRESSION;
                    MatchAndSetToken(TokenType.Return);
                    Expression();

                    DumpIntermediateCode();
                }

                MatchAndSetToken(TokenType.Semicolon);
                MatchAndSetToken(TokenType.CloseCurlyBrace);

                IntermediateCodePrinter($"endp {methodIdentifierToken.Lexeme}\n", false);

                // Record the local and parameter byte size
                MethodParamSize.Add(methodIdentifierToken.Lexeme, CurrentMethod.ParameterSize);
                MethodLocalSize.Add(methodIdentifierToken.Lexeme, 
                    CurrentMethod.SizeOfLocal + ExpressionExpander.TotalTempVariableSize);

                // Exit current scope
                PerformScopeExitAction();

                VariableLocation.Clear();
                BPOffset = 0;

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

            DumpIntermediateCode();

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

            DumpIntermediateCode();

            StatementTail();
            PopProduction();
        }

        private void Statement()
        {
            // Statement -> AssignmentStatement | IOStatement

            PushProduction("Statement");

            if (CurrentToken.Type != TokenType.Identifier)
            {
                MatchAndSetToken(TokenType.Identifier);
            }

            if (CurrentToken.Lexeme == READ || CurrentToken.Lexeme == WRITE || CurrentToken.Lexeme == WRITE_LN)
            {
                IOStatement();
            }
            else
            {
                AssignmentStatement();
            }
            PopProduction();
        }

        private void AssignmentStatement()
        {
            // AssignmentStatement -> idt = Expression | idt = MethodCall | MethodCall
            PushProduction("AssignmentStatement");
            var identifier = CurrentToken.Lexeme;
            var entry = SymbolTable.Lookup(identifier);

            if (entry == null)
            {
                throw new UndeclaredIdentifierException(identifier);
            }
            else if (entry.Type == EntryType.Constant)
            {
                throw new InvalidOperationException("A constant variable can't be used in an assignment statement");
            }
            else if (entry.Type == EntryType.Class)
            {
                // MethodCall
                ExpressionExpander.Mode = IntermediateCodeGeneratorService.METHOD_CALL;
                MethodCall();
            }
            else
            {
                // assignment
                ExpressionExpander.Push(CurrentToken);
                MatchAndSetToken(TokenType.Identifier);

                ExpressionExpander.Push(CurrentToken);
                MatchAndSetToken(TokenType.Assignment);

                if (CurrentToken.Type == TokenType.Identifier)
                {
                    entry = SymbolTable.Lookup(CurrentToken.Lexeme);

                    if (entry == null)
                    {
                        throw new UndeclaredIdentifierException(identifier);
                    }

                    switch (entry.Type)
                    {
                        case EntryType.Variable:
                        case EntryType.Constant:
                            // An assignment with expression
                            ExpressionExpander.Mode = IntermediateCodeGeneratorService.ASSIGNMENT;
                            Expression();
                            break;

                        case EntryType.Class:
                            // Assignment with method call
                            // Since methods are called as if they're static
                            ExpressionExpander.Mode = IntermediateCodeGeneratorService.ASSIGNMENT_VIA_METHOD_CALL;
                            MethodCall();
                            break;

                        default:
                            throw new Exception("Invalid operation");
                    }
                }
                else
                {
                    // An assignment with expression
                    ExpressionExpander.Mode = IntermediateCodeGeneratorService.ASSIGNMENT;
                    Expression();
                }
            }

            PopProduction();
        }

        private void MethodCall()
        {
            // MethodCall		->	ClassName.idt ( Parameters )
            PushProduction("MethodCall");

            var className = CurrentToken.Lexeme;

            ClassName();
            MatchAndSetToken(TokenType.Dot);

            var methodName = CurrentToken.Lexeme;
            ExpressionExpander.Push(CurrentToken);
            MatchAndSetToken(TokenType.Identifier);

            var classEntry = SymbolTable.Lookup(className);
            if (classEntry == null)
            {
                throw new UndeclaredIdentifierException(className);
            }
            else
            {
                var methodNames = ((ClassEntry)classEntry.Content).MethodNames;
                var found = false;

                while (methodNames != null && !found)
                {
                    found = methodName == methodNames.Value;
                    methodNames = methodNames.Next;
                }

                if (!found)
                {
                    throw new UndeclaredIdentifierException(methodName);
                }
            }

            MatchAndSetToken(TokenType.OpenParen);
            Parameters();
            MatchAndSetToken(TokenType.CloseParen);

            PopProduction();
        }

        private void ClassName()
        {
            // ClassName		->	idt
            PushProduction("ClassName");

            var identifier = CurrentToken.Lexeme;
            ExpressionExpander.Push(CurrentToken);
            MatchAndSetToken(TokenType.Identifier);

            if (SymbolTable.Lookup(identifier) == null)
            {
                throw new UndeclaredIdentifierException(identifier);
            }

            PopProduction();
        }

        private void Parameters()
        {
            // Parameters		->	idt ParameterTail | num ParameterTail| ε
            PushProduction("Parameters");

            try
            {
                ExpressionExpander.Push(CurrentToken);
                SetTokenIfAnyMatch(
                        TokenType.Identifier,
                        TokenType.LiteralInteger,
                        TokenType.LiteralReal);
            }
            catch (MissingOptionalTokenException)
            {
                // ε production
                ExpressionExpander.Pop();
                PopProduction();
                return;
            }

            ParameterTail();
            PopProduction();
        }

        private void ParameterTail()
        {
            // ParameterTail	->	, idt ParameterTail | , num ParameterTail | ε
            PushProduction("ParameterTail");

            if (CurrentToken.Type != TokenType.Comma)
            {
                PopProduction();
                return;
            }

            MatchAndSetToken(TokenType.Comma);
            ExpressionExpander.Push(CurrentToken);
            SetTokenIfAnyMatch(
                TokenType.Identifier,
                TokenType.LiteralInteger,
                TokenType.LiteralReal);
            ParameterTail();

            PopProduction();
        }

        private void IOStatement()
        {
            //IOStatement -> InStatement | OutStatement
            PushProduction("IOStatement");

            ExpressionExpander.Mode = IntermediateCodeGeneratorService.IO_METHOD_CALL;
            if (CurrentToken.Lexeme == READ)
            {
                InStatement();
            }
            else
            {
                OutStatement();
            }

            PopProduction();
        }

        private void InStatement()
        {
            //InStatement -> read(IO_IdentiferList)

            PushProduction("InStatement");
            var identifier = CurrentToken.Lexeme;
            if (identifier != READ)
            {
                throw new MissingTokenException(READ, identifier, "InStatement");
            }

            ExpressionExpander.Push(CurrentToken);
            MatchAndSetToken(TokenType.Identifier);
            MatchAndSetToken(TokenType.OpenParen);
            IO_IdentiferList();
            MatchAndSetToken(TokenType.CloseParen);

            PopProduction();
        }

        private void IO_IdentiferList()
        {
            //IO_IdentiferList -> idt  IO_IdentiferListTail

            PushProduction("IO_IdentiferList");

            ExpressionExpander.Push(CurrentToken);
            MatchAndSetToken(TokenType.Identifier);
            IO_IdentiferListTail();

            PopProduction();
        }

        private void IO_IdentiferListTail()
        {
            //IO_IdentiferListTail ->  , idt IO_IdentiferListTail | ε

            PushProduction("IO_IdentiferListTail");

            if (CurrentToken.Type == TokenType.Comma)
            {
                MatchAndSetToken(TokenType.Comma);
                ExpressionExpander.Push(CurrentToken);
                MatchAndSetToken(TokenType.Identifier);
                IO_IdentiferListTail();
            }

            PopProduction();
        }

        private void OutStatement()
        {
            //OutStatement -> write(WriteList) | writeln(WriteList)

            PushProduction("OutStatement");

            var identifier = CurrentToken.Lexeme;
            if (identifier != WRITE && identifier != WRITE_LN)
            {
                throw new MissingTokenException($"{WRITE}|{WRITE_LN}", identifier, "OutStatement");
            }

            ExpressionExpander.Push(CurrentToken);
            MatchAndSetToken(TokenType.Identifier);
            MatchAndSetToken(TokenType.OpenParen);
            WriteList();
            MatchAndSetToken(TokenType.CloseParen);

            PopProduction();
        }

        private void WriteList()
        {
            //WriteList -> WriteToken WriteListTail
            PushProduction("WriteList");

            WriteToken();
            WriteListTail();

            PopProduction();
        }

        private void WriteListTail()
        {
            //WriteListTail ->  , WriteToken WriteListTail | ε

            PushProduction("WriteListTail");

            if (CurrentToken.Type == TokenType.Comma)
            {
                MatchAndSetToken(TokenType.Comma);
                WriteToken();
                WriteListTail();
            }

            PopProduction();
        }

        private void WriteToken()
        {
            //WriteToken -> idt | numt | literal

            PushProduction("WriteToken");

            var token = CurrentToken;

            if (token.Type == TokenType.LiteralString)
            {
                var tempName = InsertGlobalString(token.Lexeme);
                token = Token.CreateTemporaryToken(tempName, token.LineNumber);
            }

            ExpressionExpander.Push(token);

            SetTokenIfAnyMatch(
                TokenType.Identifier,
                TokenType.LiteralBoolean,
                TokenType.LiteralInteger,
                TokenType.LiteralReal,
                TokenType.LiteralString);

            PopProduction();
        }

        private string InsertGlobalString(string stringLiteral)
        {
            var globalName = $"_S{GlobalStrings.Count}";

            GlobalStrings.Add(globalName, stringLiteral);
            VariableLocation.Add(globalName, globalName);

            return globalName;
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
                    throw new UndeclaredIdentifierException(identifier);
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
                    throw new UndeclaredIdentifierException(identifier);
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

            // Print intermediate code for procedure
            IntermediateCodePrinter($"proc {identifier.Lexeme}", false);

            Depth++;
            SequenceofStatements();

            IntermediateCodePrinter($"endp {identifier.Lexeme}\n", false);

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
                var paramType = new Models.Table.LinkedListNode<KeyValuePair<string, VariableType>>
                {
                    Value = new KeyValuePair<string, VariableType>(identifier.Lexeme, _dataType)
                };
                paramType.Next = CurrentMethod.Parameters;
                CurrentMethod.Parameters = paramType;

                CurrentMethod.NumberOfParameters++;
                CurrentMethod.ParameterSize += _size;
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
                Parameters = null,
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
            // Console.WriteLine();
            // Console.WriteLine($"Dumping entries at depth [{Depth}]\n");
            // SymbolTable.WriteTable(Depth);
            SymbolTable.DeleteDepth(Depth);
            Depth--;
        }

        private void SetupParameterStackLocation()
        {
            var parameterList = CurrentMethod.Parameters;
            var sum = RETURN_ADDRESS_OLD_BP_OFFSET;

            // Loop1: Sum up total
            while (parameterList != null)
            {
                sum += GetDataTypeSize(parameterList.Value.Value);
                VariableLocation.Add(parameterList.Value.Key, "");
                parameterList = parameterList.Next;
            }

            parameterList = CurrentMethod.Parameters;

            // Loop2: Setup offsets
            while (parameterList != null)
            {
                sum -= GetDataTypeSize(parameterList.Value.Value);
                VariableLocation[parameterList.Value.Key] = $"{BP}+{sum}";
                parameterList = parameterList.Next;
            }
        }

        private void DumpIntermediateCode()
        {
            ExpressionExpander.GenerateIntermediateCode(IntermediateCodePrinter, VariableLocation, BPOffset);
            ExpressionExpander.Clear();
        }

        private void IntermediateCodePrinter(object str)
        {
            IntermediateCodePrinter(str, true);
        }

        private void IntermediateCodePrinter(object str, bool useTab)
        {
            if (str == null)
            {
                return;
            }

            if (useTab)
            {
                var tab = "    ";
                str = $"{tab}{str.ToString().Replace("\n", $"\n{tab}")}";
            }

            CompilerService.PrintToFile(str);
            Console.WriteLine(str);
        }
    }
}