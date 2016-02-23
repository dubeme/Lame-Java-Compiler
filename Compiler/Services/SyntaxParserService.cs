using Compiler.Models;
using Compiler.Models.Exceptions;
using System;

namespace Compiler.Services
{
    public class SyntaxParserService
    {
        private LexicalAnalyzerService LexicalAnalyzer;
        private Token NextToken;
        private bool ReuseOldToken;

        public SyntaxParserService(LexicalAnalyzerService lexAnalyzer)
        {
            this.LexicalAnalyzer = lexAnalyzer;
        }

        public void Parse()
        {
            try
            {
                Program();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing token on line #{LexicalAnalyzer.LineNumber}.\n\n {ex.Message}", ex);
            }
        }

        private void GetNextToken()
        {
            if (!this.ReuseOldToken)
            {
                this.NextToken = this.LexicalAnalyzer.GetNextToken();
            }
            else
            {
                // Since I only buffer 1 token, this has to be reset
                // The caller will get the buffered token
                this.ReuseOldToken = false;
            }
        }

        private void Program()
        {
            // Program -> MoreClasses MainClass
            MoreClasses();
            MainClass();

            this.GetNextToken();

            if (this.NextToken.Type != TokenType.EndOfFile)
            {
                throw new Exception("Didn't reach end of file, but the program grammar is done matching");
            }
        }

        private void MoreClasses()
        {
            // MoreClasses -> class idt ExtendsClass { VariableDeclaration MethodDeclaration } MoreClasses | ε

            var production = "MoreClasses";

            if (MatchProductionsNextOptionalTokenAs(production, TokenType.Class))
            {
                MatchProductionsNextTokenAs(production, TokenType.Identifier);

                ExtendsClass();

                MatchProductionsNextTokenAs(production, TokenType.OpenCurlyBrace);

                VariableDeclaration();
                MethodDeclaration();

                MatchProductionsNextTokenAs(production, TokenType.CloseCurlyBrace);

                MoreClasses();
            }
        }

        private void ExtendsClass()
        {
            // ExtendsClass	->	extendst idt | ε
            var production = "ClassDeclaration";

            // If an extends is matched
            if (MatchProductionsNextOptionalTokenAs(production, TokenType.Extends))
            {
                MatchProductionsNextTokenAs(production, TokenType.Identifier);
            }
        }

        private void MainClass()
        {
            // MainClass -> finalt classt idt { publict statict voidt main (String [] idt) {SequenceofStatements }}

            var production = "MainClass";

            MatchProductionsNextTokenAs(production, TokenType.Final);
            MatchProductionsNextTokenAs(production, TokenType.Class);
            MatchProductionsNextTokenAs(production, TokenType.Identifier);
            MatchProductionsNextTokenAs(production, TokenType.OpenCurlyBrace);
            MatchProductionsNextTokenAs(production, TokenType.Public);
            MatchProductionsNextTokenAs(production, TokenType.Static);
            MatchProductionsNextTokenAs(production, TokenType.Void);
            MatchProductionsNextTokenAs(production, TokenType.Main);
            MatchProductionsNextTokenAs(production, TokenType.OpenParen);
            MatchProductionsNextTokenAs(production, TokenType.String);
            MatchProductionsNextTokenAs(production, TokenType.OpenSquareBracket);
            MatchProductionsNextTokenAs(production, TokenType.CloseSquareBracket);
            MatchProductionsNextTokenAs(production, TokenType.Identifier);
            MatchProductionsNextTokenAs(production, TokenType.CloseParen);
            MatchProductionsNextTokenAs(production, TokenType.OpenCurlyBrace);

            SequenceofStatements();

            MatchProductionsNextTokenAs(production, TokenType.CloseCurlyBrace);
            MatchProductionsNextTokenAs(production, TokenType.CloseCurlyBrace);
        }

        private void VariableDeclaration()
        {
            // VariableDeclaration -> Type IdentifierList ; VariableDeclaration | finalt Type idt = numt; VariableDeclaration | ε
            var production = "VariableDeclaration";

            if (MatchProductionsNextOptionalTokenAs(production, TokenType.Final))
            {
                Type();
                MatchProductionsNextTokenAs(production, TokenType.Identifier);
                MatchProductionsNextTokenAs(production, TokenType.Assignment);
                MatchProductionsNextTokenAs(production, TokenGroup.Literal);
                MatchProductionsNextTokenAs(production, TokenType.Semicolon);
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
                MatchProductionsNextTokenAs(production, TokenType.Semicolon);
                VariableDeclaration();
            }
        }

        private void IdentifierList()
        {
            // IdentifierList -> idt | IdentifierList , idt
            var production = "IdentifierList";

            MatchProductionsNextTokenAs(production, TokenType.Identifier);

            if (MatchProductionsNextOptionalTokenAs(production, TokenType.Comma))
            {
                IdentifierList();
            }
        }

        private void Type()
        {
            // Type -> intt | booleant |voidt
            var production = "Type";

            var matchedType =
                MatchProductionsNextOptionalTokenAs(production, TokenType.Int) ||
                MatchProductionsNextOptionalTokenAs(production, TokenType.Boolean) ||
                MatchProductionsNextOptionalTokenAs(production, TokenType.Void);

            if (!matchedType)
            {
                throw new MissingTokenException("int|boolean|void", this.NextToken.Type.ToString(), production);
            }
        }

        private void MethodDeclaration()
        {
            // MethodDeclaration -> publict Type idt (FormalParameterList) { VariableDeclaration SequenceofStatements returnt Expression ; } MethodDeclaration | ε
            var production = "MethodDeclaration";

            if (MatchProductionsNextOptionalTokenAs(production, TokenType.Public))
            {
                Type();

                MatchProductionsNextTokenAs(production, TokenType.Identifier);
                MatchProductionsNextTokenAs(production, TokenType.OpenParen);

                FormalParameterList();

                MatchProductionsNextTokenAs(production, TokenType.CloseParen);
                MatchProductionsNextTokenAs(production, TokenType.OpenCurlyBrace);

                VariableDeclaration();
                SequenceofStatements();

                MatchProductionsNextTokenAs(production, TokenType.Return);

                Expression();

                MatchProductionsNextTokenAs(production, TokenType.Semicolon);
                MatchProductionsNextTokenAs(production, TokenType.CloseCurlyBrace);

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

            MatchProductionsNextTokenAs(production, TokenType.Identifier);
            RestOfFormalParameterList();
        }

        private void RestOfFormalParameterList()
        {
            // RestOfFormalParameterList -> , Type idt RestOfFormalParameterList | ε
            var production = "RestOfFormalParameterList";

            if (MatchProductionsNextOptionalTokenAs(production, TokenType.Comma))
            {
                Type();

                MatchProductionsNextTokenAs(production, TokenType.Identifier);

                if (MatchProductionsNextOptionalTokenAs(production, TokenType.Comma))
                {
                    RestOfFormalParameterList();
                }
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

        private void MatchProductionsNextTokenAs(string production, TokenType expectedType)
        {
            GetNextToken();

            if (this.NextToken.Type != expectedType)
            {
                throw new MissingTokenException(expectedType, this.NextToken.Type, production);
            }
        }

        private void MatchProductionsNextTokenAs(string production, TokenGroup expectedGroup)
        {
            GetNextToken();

            if (this.NextToken.Group != expectedGroup)
            {
                throw new MissingTokenException(expectedGroup, this.NextToken.Group, production);
            }
        }

        private bool MatchProductionsNextOptionalTokenAs(string production, TokenType expectedType)
        {
            GetNextToken();

            if (this.NextToken.Type == expectedType)
            {
                this.ReuseOldToken = false;
                return true;
            }

            this.ReuseOldToken = true;
            return false;
        }

        //TODO: Add functionality for matching multiple tokens
        //private void MatchProductionsNextTokenAs(string production, params TokenType[] tokenTypes)
        //{
        //    GetNextToken();

        //    if (tokenTypes != null)
        //    {
        //        if (!tokenTypes.Any(tt => tt == this.NextToken.Type))
        //        {
        //            throw new MissingTokenException(this.NextToken.Type, production);
        //        }
        //    }

        //    throw new MissingTokenException(this.NextToken.Type, production);
        //}

        //private bool MatchProductionsNextOptionalTokenAs(string production, params TokenType[] tokenTypes)
        //{
        //    GetNextToken();

        //    if (tokenTypes != null)
        //    {
        //        foreach (var tokenType in tokenTypes)
        //        {
        //            if (this.NextToken.Type == tokenType)
        //            {
        //                this.ReuseOldToken = false;
        //                return true;
        //            }
        //        }
        //    }

        //    this.ReuseOldToken = true;
        //    return false;
        //}
    }
}