using Compiler.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                    return ;
                }
                public boolean dim(){
                    return ;
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
                    return ;
                }
                public boolean sum(float num, int ans){
                    return ;
                }
                public float jamb(float num){
                    return ;
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
                    return ;
                }
                public boolean sum(float num, int ans){
                    float x,y,z;
                    int a;
                    boolean k;
                    return ;
                }
                public float jamb(float num){
                    return ;
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