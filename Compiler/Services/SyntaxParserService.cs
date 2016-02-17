using Compiler.Models;
using Compiler.Models.Exceptions;

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
                this.ReuseOldToken = true;
            }
        }

        private void Program()
        {
            // Program -> MoreClasses MainClass
            MoreClasses();
            MainClass();
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

        private void MoreClasses()
        {
            // MoreClasses -> ClassDeclaration MoreClasses | 

            //
            if (this.NextToken.Type != TokenType.EndOfFile)
            {
                ClassDeclaration();
                MoreClasses();
            }
        }

        private void ClassDeclaration()
        {
            // ClassDeclaration -> class idt { VariableDeclaration MethodDeclaration } | class idt extendst idt { VariableDeclaration MethodDeclaration }
            var production = "ClassDeclaration";

            MatchProductionsNextTokenAs(production, TokenType.Class);
            MatchProductionsNextTokenAs(production, TokenType.Identifier);

            // If an extends is matched
            if (MatchProductionsNextOptionalTokenAs(production, TokenType.Extends))
            {
                MatchProductionsNextTokenAs(production, TokenType.Identifier);
            }


            MatchProductionsNextTokenAs(production, TokenType.OpenCurlyBrace);

            VariableDeclaration();
            MethodDeclaration();

            MatchProductionsNextTokenAs(production, TokenType.CloseCurlyBrace);
        }

        private void VariableDeclaration()
        {
            // VariableDeclaration -> Type IdentifierList ; VariableDeclaration | finalt Type idt = numt; VariableDeclaration | 
        }

        private void IdentifierList()
        {
            // IdentifierList -> idt | IdentifierList , idt
        }

        private void Type()
        {
            // Type -> intt | booleant |voidt
        }

        private void MethodDeclaration()
        {
            // MethodDeclaration -> publict Type idt (FormalParameterList) { VariableDeclaration SequenceofStatements returnt Expression ; } MethodDeclaration | 
        }

        private void FormalParameterList()
        {
            // FormalParameterList -> Type idt RestOfFormalParameterList | 
        }

        private void RestOfFormalParameterList()
        {
            // RestOfFormalParameterList -> , Type idt RestOfFormalParameterList | 
        }

        private void SequenceofStatements()
        {
            // SequenceofStatements -> 
        }

        private void Expression()
        {
            // Expression -> 
        }

        private void MatchProductionsNextTokenAs(string production, TokenType tokenType)
        {
            GetNextToken();
            if (this.NextToken.Type != tokenType)
            {
                throw new MissingTokenException(this.NextToken.Type, production);
            }
        }

        private bool MatchProductionsNextOptionalTokenAs(string production, TokenType tokenType)
        {
            GetNextToken();

            if (this.NextToken.Type == tokenType)
            {
                this.ReuseOldToken = false;
                return true;
            }

            this.ReuseOldToken = true;
            return false;
        }
    }
}