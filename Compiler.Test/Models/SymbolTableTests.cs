using Compiler.Models.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Compiler.Models.Tests
{
    [TestClass()]
    public class SymbolTableTests
    {
        [TestMethod()]
        [TestCategory("Symbol Table")]
        public void Insert_Lookup_WriteTable_SymbolTableTest()
        {
            var count = 0;
            var symTable = new SymbolTable();
            symTable.Printer = (val) => { count++; };

            symTable.Insert(Token.CreateToken("x", 0), 0);
            symTable.Insert(Token.CreateToken("y", 0), 0);
            symTable.Insert(Token.CreateToken("z", 0), 0);

            symTable.Lookup("x").Content = CreateVariableContent();
            symTable.Lookup("y").Content = CreateVariableContent();
            symTable.Lookup("z").Content = CreateVariableContent();

            symTable.Insert(Token.CreateToken("a", 0), 1);
            symTable.Insert(Token.CreateToken("x", 0), 1);
            symTable.Insert(Token.CreateToken("y", 0), 1);
            symTable.Insert(Token.CreateToken("z", 0), 1);

            symTable.Lookup("a").Content = CreateVariableContent();
            symTable.Lookup("x").Content = CreateVariableContent();
            symTable.Lookup("y").Content = CreateVariableContent();
            symTable.Lookup("z").Content = CreateVariableContent();

            symTable.WriteTable(0);
            Assert.AreEqual(3, count);

            count = 0;
            symTable.WriteTable(1);
            Assert.AreEqual(4, count);

            count = 0;
            symTable.WriteTable(2);
            Assert.AreEqual(0, count);
        }

        [TestMethod()]
        [TestCategory("Symbol Table")]
        public void CRUD_SymbolTableTest()
        {
            var count = 0;
            var symTable = new SymbolTable();
            symTable.Printer = (val) => { count++; };

            symTable.Insert(Token.CreateToken("a", 0), 0);
            symTable.Insert(Token.CreateToken("b", 0), 0);

            symTable.Lookup("a").Content = CreateVariableContent();
            symTable.Lookup("b").Content = CreateVariableContent();

            symTable.Insert(Token.CreateToken("x", 0), 1);
            symTable.Insert(Token.CreateToken("y", 0), 1);
            symTable.Insert(Token.CreateToken("z", 0), 1);

            symTable.Lookup("x").Content = CreateVariableContent();
            symTable.Lookup("y").Content = CreateVariableContent();
            symTable.Lookup("z").Content = CreateVariableContent();

            symTable.Insert(Token.CreateToken("j", 0), 2);
            symTable.Lookup("j").Content = CreateVariableContent();

            // Delete depth 9, this should do nothing
            symTable.DeleteDepth(9);

            symTable.WriteTable(0);
            symTable.WriteTable(1);
            symTable.WriteTable(2);
            Assert.AreEqual(6, count);

            // Delete depth 1, 3 items should be deleted
            symTable.DeleteDepth(1);

            count = 0;
            symTable.WriteTable(0);
            symTable.WriteTable(1);
            symTable.WriteTable(2);
            Assert.AreEqual(3, count);

            // Delete depth 0, 2 items should be deleted
            symTable.DeleteDepth(0);

            count = 0;
            symTable.WriteTable(0);
            symTable.WriteTable(1);
            symTable.WriteTable(2);
            Assert.AreEqual(1, count);

            // Try deleting depth 0 again. This should have no effect
            symTable.DeleteDepth(0);

            count = 0;
            symTable.WriteTable(0);
            symTable.WriteTable(1);
            symTable.WriteTable(2);
            Assert.AreEqual(1, count);
        }

        private static VariableEntry CreateVariableContent()
        {
            return new VariableEntry
            {
                DataType = VariableType.Int,
                Offset = 0,
                Size = 4
            };
        }
    }
}