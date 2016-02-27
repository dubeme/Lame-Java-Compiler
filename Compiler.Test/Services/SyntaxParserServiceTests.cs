using Microsoft.VisualStudio.TestTools.UnitTesting;
using Compiler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Services.Tests
{
    [TestClass()]
    public class SyntaxParserServiceTests
    {

        const string testString1= @"final class Main
        {public static void main(String[] args){}}";

        const string testString2 = @"class one {} class two{}
        final class Main
        {
            public static void main(String[] args)
            {
            }
        }";

        const string testString3 = @"class One
        {
            int erer;
            final int kl = 2;
            public void func(){ return ;}
        }
        class Two
        {
            int a;
            boolean b;
        } final class Main {
            public static void main(String[] args)
            {
            }
        }";

        const string testString4 = @"class tim { int x, y, z;}
        final class Main {public static void main(String[] args){}}";

        const string testString5 = @"class tim { 
            int x, y, z; 
            public void ass(int a, int b, int c){ return;}
        }
        final class Main {public static void main(String[] args){}}";
        
        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void TestJustMainClass()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString1);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void TestMainClassAndMoreClasses()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString2);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void TestFieldsAndMethodsInClass()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString3);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void TestMultipleFieldOnOneLine()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString4);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void TestMultipleParameterForMemberFunction()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString5);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }
    }
}