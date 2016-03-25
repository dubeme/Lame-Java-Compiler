using Compiler.Models;
using Compiler.Models.Exceptions;
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

        public SyntaxParserService(LexicalAnalyzerService lexAnalyzer)
        {
            this.LexicalAnalyzer = lexAnalyzer;
            this.SymbolTable = new SymbolTable();
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

        private void GetNextToken()
        {
            this.CurrentToken = this.LexicalAnalyzer.GetNextToken();
        }

        private void Program()
        {
            GetNextToken();

            // Program -> MoreClasses MainClass
            MoreClasses();
            MainClass();

            this.GetNextToken();

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

            GetNextToken();
            MatchAndSetToken(production, TokenType.Identifier);

            ExtendsClass();

            MatchAndSetToken(production, TokenType.OpenCurlyBrace);

            VariableDeclaration();
            MethodDeclaration();

            MatchAndSetToken(production, TokenType.CloseCurlyBrace);

            MoreClasses();
        }

        private void ExtendsClass()
        {
            // ExtendsClass	->	extendst idt | ε
            var production = "ClassDeclaration";

            // If an extends is matched
            if (this.CurrentToken.Type == TokenType.Extends)
            {
                GetNextToken();
                MatchAndSetToken(production, TokenType.Identifier);
            }
        }

        private void MainClass()
        {
            // MainClass -> finalt classt idt { publict statict voidt main (String [] idt) {SequenceofStatements }}

            var production = "MainClass";

            MatchAndSetToken(production, TokenType.Final);
            MatchAndSetToken(production, TokenType.Class);
            MatchAndSetToken(production, TokenType.Identifier);
            MatchAndSetToken(production, TokenType.OpenCurlyBrace);
            MatchAndSetToken(production, TokenType.Public);
            MatchAndSetToken(production, TokenType.Static);
            MatchAndSetToken(production, TokenType.Void);
            MatchAndSetToken(production, TokenType.Main);
            MatchAndSetToken(production, TokenType.OpenParen);
            MatchAndSetToken(production, TokenType.String);
            MatchAndSetToken(production, TokenType.OpenSquareBracket);
            MatchAndSetToken(production, TokenType.CloseSquareBracket);
            MatchAndSetToken(production, TokenType.Identifier);
            MatchAndSetToken(production, TokenType.CloseParen);
            MatchAndSetToken(production, TokenType.OpenCurlyBrace);

            SequenceofStatements();

            MatchAndSetToken(production, TokenType.CloseCurlyBrace);
            MatchAndSetToken(production, TokenType.CloseCurlyBrace);
        }

        private void VariableDeclaration()
        {
            // VariableDeclaration -> Type IdentifierList ; VariableDeclaration | finalt Type idt = numt; VariableDeclaration | ε
            var production = "VariableDeclaration";

            if (CurrentToken.Type == TokenType.Final)
            {
                GetNextToken();
                Type();
                MatchAndSetToken(production, TokenType.Identifier);
                MatchAndSetToken(production, TokenType.Assignment);
                MatchAndSetToken(production, TokenGroup.Literal);
                MatchAndSetToken(production, TokenType.Semicolon);
                VariableDeclaration();
            }
            else
            {
                try
                {
                    Type();
                }
                catch (Exception)
                {
                    // The ε case.
                    return;
                }

                IdentifierList();
                MatchAndSetToken(production, TokenType.Semicolon);
                VariableDeclaration();
            }
        }

        private void IdentifierList()
        {
            // IdentifierList -> idt | IdentifierList , idt
            var production = "IdentifierList";

            MatchAndSetToken(production, TokenType.Identifier);

            if (CurrentToken.Type == TokenType.Comma)
            {
                GetNextToken();
                IdentifierList();
            }
        }

        private void Type()
        {
            // Type -> intt | booleant |voidt
            var production = "Type";

            switch (CurrentToken.Type)
            {
                case TokenType.Int:
                case TokenType.Void:
                case TokenType.Boolean:
                    GetNextToken();
                    break;

                default:
                    throw new MissingTokenException("int|boolean|void", this.CurrentToken.Type.ToString(), production);
            }
        }

        private void MethodDeclaration()
        {
            // MethodDeclaration -> publict Type idt (FormalParameterList) { VariableDeclaration SequenceofStatements returnt Expression ; } MethodDeclaration | ε
            var production = "MethodDeclaration";

            if (CurrentToken.Type == TokenType.Public)
            {
                GetNextToken();
                Type();

                MatchAndSetToken(production, TokenType.Identifier);
                MatchAndSetToken(production, TokenType.OpenParen);

                FormalParameterList();

                MatchAndSetToken(production, TokenType.CloseParen);
                MatchAndSetToken(production, TokenType.OpenCurlyBrace);

                VariableDeclaration();
                SequenceofStatements();

                MatchAndSetToken(production, TokenType.Return);

                Expression();

                MatchAndSetToken(production, TokenType.Semicolon);
                MatchAndSetToken(production, TokenType.CloseCurlyBrace);

                MethodDeclaration();
            }
        }

        private void FormalParameterList()
        {
            // FormalParameterList -> Type idt RestOfFormalParameterList | ε
            var production = "FormalParameterList";

            try
            {
                Type();
            }
            catch (Exception)
            {
                // ε case
                return;
            }

            MatchAndSetToken(production, TokenType.Identifier);
            RestOfFormalParameterList();
        }

        private void RestOfFormalParameterList()
        {
            // RestOfFormalParameterList -> , Type idt RestOfFormalParameterList | ε
            var production = "RestOfFormalParameterList";

            if (CurrentToken.Type == TokenType.Comma)
            {
                GetNextToken();
                Type();

                MatchAndSetToken(production, TokenType.Identifier);
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

        private void MatchAndSetToken(string production, TokenType expectedType)
        {
            if (this.CurrentToken.Type != expectedType)
            {
                throw new MissingTokenException(expectedType, this.CurrentToken.Type, production);
            }

            GetNextToken();
        }

        private void MatchAndSetToken(string production, TokenGroup expectedGroup)
        {
            if (this.CurrentToken.Group != expectedGroup)
            {
                throw new MissingTokenException(expectedGroup, this.CurrentToken.Group, production);
            }

            GetNextToken();
        }
    }
}