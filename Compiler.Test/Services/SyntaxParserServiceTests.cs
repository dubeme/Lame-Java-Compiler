using Compiler.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Compiler.Services.Tests
{
    [TestClass()]
    public class SyntaxParserServiceTests
    {
        private const string FILE_PATH = @"..\..\test.files\";

        private const string justMain = @"justMain.java";
        private const string main1Class = @"main1Class.java";
        private const string main2Classes = @"main2Classes.java";
        private const string classWithFields = @"classWithFields.java";
        private const string classWithFieldsAndMethod = @"classWithFieldsAndMethod.java";
        private const string classWithFieldsAndMethods = @"classWithFieldsAndMethods.java";
        private const string classWithFieldsMethodsParameters = @"classWithFieldsMethodsParameters.java";
        private const string classWithFieldsMethodsParametersAndLocals = @"classWithFieldsMethodsParametersAndLocals.java";
        private const string classWithConstant = @"classWithConstant.java";
        private const string classWithExpression = @"classWithExpression.java";
        private const string classWithExpressionBasic = @"classWithExpressionBasic.java";
        private const string classWithExpressionSimpleArithmetic = @"classWithExpressionSimpleArithmetic.java";
        private const string classWithExpressionBoolean = @"classWithExpressionBoolean.java";
        private const string classWithExpressionPemdas = @"classWithExpressionPemdas.java";
        private const string classWithExpressionUndeclared1 = @"classWithExpressionUndeclared1.java";
        private const string classWithExpressionUndeclared2 = @"classWithExpressionUndeclared2.java";

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestJustMain()
        {
            var parser = CreateSyntaxParserService(justMain);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestMain1Class()
        {
            var parser = CreateSyntaxParserService(main1Class);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestMain2Classes()
        {
            var parser = CreateSyntaxParserService(main2Classes);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestClassWithFields()
        {
            var parser = CreateSyntaxParserService(classWithFields);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestClassWithFieldsAndMethod()
        {
            var parser = CreateSyntaxParserService(classWithFieldsAndMethod);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestClassWithFieldsAndMethods()
        {
            var parser = CreateSyntaxParserService(classWithFieldsAndMethods);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestClassWithFieldsMethodsParameters()
        {
            var parser = CreateSyntaxParserService(classWithFieldsMethodsParameters);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestclassWithFieldsMethodsParametersAndLocals()
        {
            var parser = CreateSyntaxParserService(classWithFieldsMethodsParametersAndLocals);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestClassWithConstant()
        {
            var parser = CreateSyntaxParserService(classWithConstant);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestclassWithExpression()
        {
            var parser = CreateSyntaxParserService(classWithExpression);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestclassWithExpressionBasic()
        {
            var parser = CreateSyntaxParserService(classWithExpressionBasic);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestclassWithExpressionSimpleArithmetic()
        {
            var parser = CreateSyntaxParserService(classWithExpressionSimpleArithmetic);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestclassWithExpressionBoolean()
        {
            var parser = CreateSyntaxParserService(classWithExpressionBoolean);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        public void TestclassWithExpressionPemdas()
        {
            var parser = CreateSyntaxParserService(classWithExpressionPemdas);
            parser.Parse();
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        [ExpectedException(typeof(UndeclaredIdentifierException))]
        public void TestclassWithExpressionUndeclared1()
        {
            try
            {
                var parser = CreateSyntaxParserService(classWithExpressionUndeclared1);
                parser.Parse();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [TestMethod()]
        [TestCategory("Syntax Tree Parser")]
        [ExpectedException(typeof(UndeclaredIdentifierException))]
        public void TestclassWithExpressionUndeclared2()
        {
            try
            {
                var parser = CreateSyntaxParserService(classWithExpressionUndeclared2);
                parser.Parse();
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        private SyntaxParserService CreateSyntaxParserService(string fileName)
        {
            var fullPath = $@"{FILE_PATH}{fileName}";
            var data = System.IO.File.ReadAllText(fullPath);

            System.Diagnostics.Debug.WriteLine(data + "\n\n\n\n\n\n\n");


            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(data);
            var lexAnalyzer = new LexicalAnalyzerService(streamReader);
            var symbolTable = new SymbolTable();
            symbolTable.Printer = (dump) => { };

            return new SyntaxParserService(lexAnalyzer, symbolTable);
        }
    }
}