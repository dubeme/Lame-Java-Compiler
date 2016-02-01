using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Compiler.Services;

namespace Compiler.Test.Services
{
    [TestClass]
    public class LexicalAnalyzerServiceTest
    {
        const string TEST_FILE_NAME = "";

        [TestMethod]
        public void TestMethod1()
        {
            var lexAnalyzer = new LexicalAnalyzerService(TEST_FILE_NAME);


        }
    }
}
