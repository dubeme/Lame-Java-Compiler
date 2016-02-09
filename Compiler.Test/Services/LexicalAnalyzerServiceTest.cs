using Compiler.Models;
using Compiler.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace Compiler.Test.Services
{
    [TestClass]
    public class LexicalAnalyzerServiceTest
    {
        private const string BOOLEAN_OPERATORS = "! != && || ==";
        private const string ARITHMETIC_OPERATORS = "+ ++ += - -- -= / /= * *= % %=";
        private const string COMPARISON_OPERATORS = "<= < >= >";
        private const string LOGICAL_SHIFT_OPERATORS = "<<= << >>>= >>> >>= >>";
        private const string BITWISE_OPERATORS = "~ | |= & &= ^ ^=";
        private const string OTHER_OPERATORS = "[ ] ( ) { } , . : ; ? =";

        [TestMethod]
        public void TestArithmeticOperators()
        {
            var reader = CreateStreamReaderWith(ARITHMETIC_OPERATORS);
            var lexAnalyzer = new LexicalAnalyzerService(reader);
            var token = lexAnalyzer.GetNextToken();

            Assert.AreEqual(TokenType.Plus, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.PlusPlus, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.PlusEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Minus, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.MinusMinus, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.MinusEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Divide, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.DivideEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Multiplication, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.MultiplicationEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Modulo, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.ModuloEqual, token.Type);
        }

        [TestMethod]
        public void TestBooleanOperators()
        {
            var reader = CreateStreamReaderWith(BOOLEAN_OPERATORS);
            var lexAnalyzer = new LexicalAnalyzerService(reader);
            var token = lexAnalyzer.GetNextToken();

            Assert.AreEqual(TokenType.BooleanNot, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.BooleanNotEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.BooleanAnd, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.BooleanOr, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.BooleanEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.EndOfFile, token.Type);
        }

        [TestMethod]
        public void TestComparisonOperators()
        {
            var reader = CreateStreamReaderWith(COMPARISON_OPERATORS);
            var lexAnalyzer = new LexicalAnalyzerService(reader);
            var token = lexAnalyzer.GetNextToken();

            Assert.AreEqual(TokenType.LessThanOrEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.LessThan, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.GreaterThanOrEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.GreaterThan, token.Type);
        }

        [TestMethod]
        public void TestLogicalShifOperators()
        {
            var reader = CreateStreamReaderWith(LOGICAL_SHIFT_OPERATORS);
            var lexAnalyzer = new LexicalAnalyzerService(reader);
            var token = lexAnalyzer.GetNextToken();

            Assert.AreEqual(TokenType.BitwiseLeftShiftEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.BitwiseLeftShift, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.UnsignedRightShiftEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.UnsignedRightShift, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.BitwiseRightShiftEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.BitwiseRightShift, token.Type);
        }

        [TestMethod]
        public void TestBitwiseOperators()
        {
            var reader = CreateStreamReaderWith(BITWISE_OPERATORS);
            var lexAnalyzer = new LexicalAnalyzerService(reader);
            var token = lexAnalyzer.GetNextToken();
            
            Assert.AreEqual(TokenType.LogicalNot, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.LogicalOr, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.LogicalOrEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.LogicalAnd, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.LogicalAndEqual, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.LogicalExclusiveOr, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.LogicalExclusiveOrEqual, token.Type);
        }

        [TestMethod]
        public void TestOtherOperators()
        {
            var reader = CreateStreamReaderWith(OTHER_OPERATORS);
            var lexAnalyzer = new LexicalAnalyzerService(reader);
            var token = lexAnalyzer.GetNextToken();
            
            Assert.AreEqual(TokenType.OpenSquareBracket, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.CloseSquareBracket, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.OpenParen, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.CloseParen, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.OpenCurlyBrace, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.CloseCurlyBrace, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Comma, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Dot, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Colon, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Semicolon, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.QuestionMark, token.Type);

            token = lexAnalyzer.GetNextToken();
            Assert.AreEqual(TokenType.Assignment, token.Type);
        }

        private StreamReader CreateStreamReaderWith(string content)
        {
            return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
        }
    }
}