﻿using Compiler.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Compiler.Services.Tests
{
    [TestClass()]
    public class SyntaxParserServiceTests
    {
        private const string justMain = @"
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string main1Class = @"
            class tim {
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string main2Classes = @"
            class tim {
            }
            class tom {
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string classWithFields = @"
            class tim {
                int x, y, z;
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string classWithFieldsAndMethod = @"
            class tim {
                int x, y, z;
                public void ass(int a, int b, int c){
                    return;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string classWithFieldsAndMethods = @"
            class tim {
                int x, y, z;
                boolean a, b, c;
                public void ass(){
                    return;
                }
                public int kit(){
                    return 0;
                }
                public boolean dim(){
                    return 0;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string classWithFieldsMethodsParameters = @"
            class tim {
                int x, y, z;
                boolean a, b, c;
                public void ass(){
                    return;
                }
                public int kit(int x, int y, boolean z){
                    return 0;
                }
                public boolean sum(float num, int ans){
                    return 0;
                }
                public float jamb(float num){
                    return 0;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string classWithFieldsMethodsParametersAndLocals = @"
            class tim {
                int x, y, z;
                boolean a, b, c;
                public void ass(){
                    return;
                }
                public int kit(int x, int y, boolean z){
                    int a,b,c;
                    return 0;
                }
                public boolean sum(float num, int ans){
                    float x,y,z;
                    int a;
                    boolean k;
                    return 0;
                }
                public float jamb(float num){
                    return 0;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string classWithConstant = @"
            class two{
                final boolean BOOL = false;
                final int NUMBER = 1234567890;
                final float PI = 3.14;
            }
            final class Main {
                public static void main(String[] args) {
                }
            }";

        private const string classWithExpression= @"
            class calculator {
                int x, y, z;
                public void add(int a, int b){
                    int sum; 
                    sum = a + b;
                    return sum;
                }
                public void magic(int a, int b){
                    int ans; 
                    ans = 3 * a + b / 5;
                    return ans;
                }
                public void undefined(int a, int b){
                    int ans;
                    ans = 3 * a + b / 5;
                    return ans;
                }
                public void pemdas(int a, int b){
                    int ans;
                    ans = (2)+(3*4)-(((a));
                    return ans;
                }
                public void pemdas2(int a, int b){
                    int ans;
                    ans = !!!!!!!!!!2;
                    ans = !!!!(false);
                    return ans;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

        private const string classWithExpressionBasic = @"
            class calculator {
                public void func(){
                    int var_int;
                    float var_float;
                    boolean var_boolean;
                    
                    var_int = 9;
                    var_float = 3.14;
                    var_boolean = false;
                    return ;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";
        private const string classWithExpressionSimpleArithmetic = @"
            class calculator {
                int x, y, z;
                public void func(int a, int b){
                    float ans; 
                    ans = 3.14 * 5 * 5 + 5.3434 - 65.09909;
                    return;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";
        private const string classWithExpressionBoolean = @"
            class calculator {
                public int func(int a, int b){
                    boolean ans;
                    ans = !false;
                    ans = !!true;
                    ans = (!true);
                    return ans;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";
        private const string classWithExpressionPemdas = @"
            class calculator {
                public int func(int a, int b){
                    int ans;
                    ans = (-2)+(3*4)-(a);
                    return ans;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";
        private const string classWithExpressionUndeclared1 = @"
            class calculator {
                int x, y, z;
                public int func(int a, int b){
                    int ans;
                    ans = (3)/(a/b*c)+(13);
                    return ans;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";
        private const string classWithExpressionUndeclared2 = @"
            class calculator {
                public int func(int a, int b){
                    int ans;
                    var_ans = (3)/(a/b*)+(13);
                    return ans;
                }
            }
            final class Main {
                public static void main(String[] args){
                }
            }";

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
        [ExpectedException(typeof(UndeclaredVariableException))]
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
        public void TestclassWithExpressionUndeclared2()
        {
            var parser = CreateSyntaxParserService(classWithExpressionUndeclared2);
            parser.Parse();
        }

        private SyntaxParserService CreateSyntaxParserService(string data)
        {
            var streamReader = LexicalAnalyzerServiceTests.CreateStreamReaderWith(data);
            var lexAnalyzer = new LexicalAnalyzerService(streamReader);
            var symbolTable = new SymbolTable();
            symbolTable.Printer = (dump) => { };

            return new SyntaxParserService(lexAnalyzer, symbolTable);
        }
    }
}