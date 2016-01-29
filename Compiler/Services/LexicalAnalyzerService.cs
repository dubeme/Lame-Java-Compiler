using Compiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Services
{
    public class LexicalAnalyzerService
    {
        public Dictionary<string, TokenType> Store { get; set; }
        public Token GetNextToken()
        {
            return null;
        }



        private void Init()
        {
            Store.Add("!=", TokenType.Notequal);
            Store.Add("&&", TokenType.BooleanAnd);
            Store.Add("(", TokenType.OpenParen);
            Store.Add(")", TokenType.CloseParen);
            Store.Add("*", TokenType.Multiplication);
            Store.Add("+", TokenType.Addition);
            Store.Add("", TokenType.Comma);
            Store.Add("-", TokenType.Minus);
            Store.Add(".", TokenType.Dot);
            Store.Add("/", TokenType.Division);
            Store.Add(";", TokenType.Semicolon);
            Store.Add("<", TokenType.LessThan);
            Store.Add("<=", TokenType.LessThanOrEqual);
            Store.Add("=", TokenType.Assignment);
            Store.Add("==", TokenType.Equal);
            Store.Add(">", TokenType.GreaterThan);
            Store.Add(">=", TokenType.GreaterThanOrEqual);
            Store.Add("String", TokenType.String);
            Store.Add("System.out.println", TokenType.SystemOutPrintln);
            Store.Add("[", TokenType.OpenSquareBracket);
            Store.Add("\"", TokenType.DoubleQuote);
            Store.Add("]", TokenType.CloseSquareBracket);
            Store.Add("boolean", TokenType.Boolean);
            Store.Add("class", TokenType.Class);
            Store.Add("else", TokenType.Else);
            Store.Add("extends", TokenType.Extends);
            Store.Add("FALSE", TokenType.False);
            Store.Add("if", TokenType.If);
            Store.Add("int", TokenType.Int);
            Store.Add("length", TokenType.Length);
            Store.Add("main", TokenType.Main);
            Store.Add("new", TokenType.New);
            Store.Add("public", TokenType.Public);
            Store.Add("return", TokenType.Return);
            Store.Add("static", TokenType.Static);
            Store.Add("this", TokenType.This);
            Store.Add("TRUE", TokenType.True);
            Store.Add("void", TokenType.Void);
            Store.Add("while", TokenType.While);
            Store.Add("{", TokenType.OpenCurlyBrace);
            Store.Add("||", TokenType.BooleanOr);
            Store.Add("}", TokenType.CloseCurlyBrace);

        }
    }
}
