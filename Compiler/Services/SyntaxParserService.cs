using Compiler.Models;
using Compiler.Models.Exceptions;
using Compiler.Models.Misc;
using Compiler.Models.Table;
using System;

namespace Compiler.Services
{
    public class SyntaxParserService
    {
        private LexicalAnalyzerService LexicalAnalyzer;
        private SymbolTable SymbolTable;

        private Token _CurrentToken;

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

        private int Depth;
        private int Offset;
        private MethodEntry CurrentMethod;
        private ClassEntry CurrentClass;
        private TokenType CurrentDataType;
        private VariableScope CurrentVariableScope;

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
                    $" {ex.Message}";

                throw new Exception(errMessage, ex);
            }
        }

        private void Program()
        {
            SetNextToken();

            Depth = 1;
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
        }

        private void MoreClasses()
        {
            // MoreClasses -> class idt ExtendsClass { VariableDeclaration MethodDeclaration } MoreClasses | ε

            var production = "MoreClasses";

            if (CurrentToken.Type != TokenType.Class)
            {
                return;
            }

            SetNextToken();

            // Match class name, then insert into symbol table
            var classIdentifier = CurrentToken;
            MatchAndSetToken(production, TokenType.Identifier);
            InsertClass(classIdentifier);

            ExtendsClass();
            MatchAndSetToken(production, TokenType.OpenCurlyBrace);

            // Update values
            Depth++;

            CurrentVariableScope = VariableScope.ClassBody;
            VariableDeclaration();

            CurrentVariableScope = VariableScope.MethodBody;
            MethodDeclaration();

            MatchAndSetToken(production, TokenType.CloseCurlyBrace);

            // Exit current scope
            PerformScopeExitAction();

            MoreClasses();
        }

        private void ExtendsClass()
        {
            // ExtendsClass	->	extendst idt | ε
            var production = "ClassDeclaration";

            // If an extends is matched
            if (this.CurrentToken.Type == TokenType.Extends)
            {
                SetNextToken();
                MatchAndSetToken(production, TokenType.Identifier);
            }
        }

        private void VariableDeclaration()
        {
            // VariableDeclaration -> Type IdentifierList ; VariableDeclaration | finalt Type idt = numt; VariableDeclaration | ε
            var production = "VariableDeclaration";

            if (CurrentToken.Type == TokenType.Final)
            {
                SetNextToken();

                var dataType = CurrentToken.Type;
                Type();

                var identifier = CurrentToken;
                MatchAndSetToken(production, TokenType.Identifier);
                MatchAndSetToken(production, TokenType.Assignment);

                var value = CurrentToken.Lexeme;
                MatchAndSetToken(production, TokenGroup.Literal);

                InsertConstant(dataType, identifier, value);
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
                    return;
                }

                IdentifierList();
            }
            MatchAndSetToken(production, TokenType.Semicolon);
            VariableDeclaration();
        }

        private void IdentifierList()
        {
            // IdentifierList -> idt | IdentifierList , idt
            var production = "IdentifierList";
            var identifier = CurrentToken;

            MatchAndSetToken(production, TokenType.Identifier);
            InsertVariable(CurrentDataType, identifier, CurrentVariableScope);

            if (CurrentToken.Type == TokenType.Comma)
            {
                SetNextToken();
                IdentifierList();
            }
        }

        private void Type()
        {
            // Type -> intt | booleant |voidt | floatt
            var production = "Type";

            switch (CurrentToken.Type)
            {
                case TokenType.Int:
                case TokenType.Void:
                case TokenType.Boolean:
                case TokenType.Float:
                    SetNextToken();
                    break;

                default:
                    throw new MissingTokenException("int|boolean|void|float", this.CurrentToken.Type.ToString(), production);
            }
        }

        private void MethodDeclaration()
       { 
            // MethodDeclaration -> publict Type idt (FormalParameterList) { VariableDeclaration SequenceofStatements returnt Expression ; } MethodDeclaration | ε
            var production = "MethodDeclaration";

            if (CurrentToken.Type == TokenType.Public)
            {
                SetNextToken();

                var returnType = CurrentToken.Type;
                Type();

                var identifier = CurrentToken;
                MatchAndSetToken(production, TokenType.Identifier);

                // Insert method in symbol table
                InsertMethod(returnType, identifier);

                // update depth
                Depth++;

                MatchAndSetToken(production, TokenType.OpenParen);

                FormalParameterList();

                MatchAndSetToken(production, TokenType.CloseParen);
                MatchAndSetToken(production, TokenType.OpenCurlyBrace);

                // reset offset
                Offset = 0;

                VariableDeclaration();
                SequenceofStatements();

                MatchAndSetToken(production, TokenType.Return);

                Expression();

                MatchAndSetToken(production, TokenType.Semicolon);
                MatchAndSetToken(production, TokenType.CloseCurlyBrace);

                // Exit current scope
                PerformScopeExitAction();

                MethodDeclaration();
            }
        }

        private void FormalParameterList()
        {
            // FormalParameterList -> Type idt RestOfFormalParameterList | ε
            var production = "FormalParameterList";
            var dataType = CurrentToken.Type;

            try
            {
                Type();
            }
            catch (Exception)
            {
                // ε case
                return;
            }

            var identifier = CurrentToken;
            MatchAndSetToken(production, TokenType.Identifier);

            InsertVariable(dataType, identifier, VariableScope.MethodParameter);
            RestOfFormalParameterList();
        }

        private void RestOfFormalParameterList()
        {
            // RestOfFormalParameterList -> , Type idt RestOfFormalParameterList | ε
            var production = "RestOfFormalParameterList";

            if (CurrentToken.Type == TokenType.Comma)
            {
                SetNextToken();
                var dataType = CurrentToken.Type;

                Type();

                var identifier = CurrentToken;

                MatchAndSetToken(production, TokenType.Identifier);

                InsertVariable(dataType, identifier, VariableScope.MethodParameter);

                RestOfFormalParameterList();
            }
        }

        private void SequenceofStatements()
        {
            // SequenceofStatements -> ε
        }

        private void Expression()
        {
            // Expression -> ε
        }

        private void MainClass()
        {
            // MainClass -> finalt classt idt { publict statict voidt main (String [] idt) {SequenceofStatements }}

            var production = "MainClass";

            MatchAndSetToken(production, TokenType.Final);
            MatchAndSetToken(production, TokenType.Class);

            var identifier = CurrentToken;

            MatchAndSetToken(production, TokenType.Identifier);
            InsertClass(identifier);

            MatchAndSetToken(production, TokenType.OpenCurlyBrace);


            Depth++;
            MatchAndSetToken(production, TokenType.Public);
            MatchAndSetToken(production, TokenType.Static);
            MatchAndSetToken(production, TokenType.Void);

            identifier = CurrentToken;
            MatchAndSetToken(production, TokenType.Main);
            InsertMethod(TokenType.Void, identifier);

            MatchAndSetToken(production, TokenType.OpenParen);
            MatchAndSetToken(production, TokenType.String);
            MatchAndSetToken(production, TokenType.OpenSquareBracket);
            MatchAndSetToken(production, TokenType.CloseSquareBracket);
            MatchAndSetToken(production, TokenType.Identifier);

            // TODO: Handle command line argument parsing
            MatchAndSetToken(production, TokenType.CloseParen);
            MatchAndSetToken(production, TokenType.OpenCurlyBrace);

            Depth++;
            SequenceofStatements();

            MatchAndSetToken(production, TokenType.CloseCurlyBrace);

            PerformScopeExitAction();

            MatchAndSetToken(production, TokenType.CloseCurlyBrace);

            PerformScopeExitAction();
        }


        private void SetNextToken()
        {
            this.CurrentToken = this.LexicalAnalyzer.GetNextToken();
        }

        private void MatchAndSetToken(string production, TokenType expectedType)
        {
            if (this.CurrentToken.Type != expectedType)
            {
                throw new MissingTokenException(expectedType, this.CurrentToken.Type, production);
            }

            SetNextToken();
        }

        private void MatchAndSetToken(string production, TokenGroup expectedGroup)
        {
            if (this.CurrentToken.Group != expectedGroup)
            {
                throw new MissingTokenException(expectedGroup, this.CurrentToken.Group, production);
            }

            SetNextToken();
        }

        private void InsertVariable(TokenType dataType, Token identifier, VariableScope scope)
        {
            // Get type
            var _dataType = GetDataType(dataType);
            var _size = GetDataTypeSize(_dataType);
            var _offset = 0;

            if (scope == VariableScope.MethodParameter)
            {
                var paramType = new LinkedListNode<VariableType> { Value = _dataType };
                paramType.Next = CurrentMethod.ParameterTypes;
                CurrentMethod.ParameterTypes = paramType;

                CurrentMethod.NumberOfParameters++;
            }
            else if (scope == VariableScope.ClassBody)
            {
                var field = new LinkedListNode<string> { Value = identifier.Lexeme};
                field.Next = CurrentClass.Fields;
                CurrentClass.Fields = field;

                CurrentClass.SizeOfLocal += _size;
                
            }
            else if (scope == VariableScope.MethodBody)
            {
                CurrentMethod.SizeOfLocal += _size;
                _offset = Offset;
                Offset += _size;
            }

            // Insert formal parameter
            SymbolTable.Insert(identifier, Depth);

            var entry = SymbolTable.Lookup(identifier.Lexeme);
            entry.Type = EntryType.Variable;
            entry.Content = new VariableEntry
            {
                DataType = _dataType,
                Offset = _offset,
                Size = _size
            };
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
            var methodName = new LinkedListNode<string> { Value = identifier.Lexeme };
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

        private void InsertConstant(TokenType dataType, Token identifier, string value)
        {
            throw new NotImplementedException();
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