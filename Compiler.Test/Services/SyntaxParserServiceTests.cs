using Microsoft.VisualStudio.TestTools.UnitTesting;
using Compiler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.Models;

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
        [TestCategory("Syntax Tree Parser")]
        public void TestJustMainClass()
        {
            var parser = CreateSyntaxParserService(testString1);

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestMainClassAndMoreClasses()
        {
            var parser = CreateSyntaxParserService(testString2);

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestFieldsAndMethodsInClass()
        {
            var parser = CreateSyntaxParserService(testString3);

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestMultipleFieldOnOneLine()
        {
            var parser = CreateSyntaxParserService(testString4);

            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestMultipleParameterForMemberFunction()
        {
            var parser = CreateSyntaxParserService(testString5);

            parser.Parse();
        }

        private SyntaxParserService CreateSyntaxParserService(string data)
        {

            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(data);
            var lexAnalyzer = new LexicalAnalyzerService(streamReader);
            var symbolTable = new SymbolTable();

            return new SyntaxParserService(lexAnalyzer, symbolTable);
        }
    }
}