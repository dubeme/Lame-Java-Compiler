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

        string testString1= @"final class Main
        {
            public static void main(String[] args)
            {
            }
        }";

        string testString2 = @"class one
        {

        }
        class two
        {

        }
        final class Main
        {
            public static void main(String[] args)
            {
            }
        }";


        string testString3 = @"class One
        {
            int 1;
            
        }
        class Two
        {
            int a;
            boolean b;
        }
        final class Main
        {
            public static void main(String[] args)
            {
            }
        }";

        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void ParseTest1()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString1);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void ParseTest2()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString2);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Parse Syntax Tree")]
        public void ParseTest3()
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(testString3);
            var parser = new SyntaxParserService(new LexicalAnalyzerService(streamReader));

            parser.Parse();
        }
    }
}