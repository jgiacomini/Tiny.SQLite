using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;
using TinySQLite.Exceptions;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnPrimaryTests : BaseColumnTest
    {
        public ColumnPrimaryTests() : base(true)
        {
        }

        public class PrimaryTable
        {
            [PrimaryKey]
            public int Primary { get; set; }
        }

        public class DoublePrimaryKey
        {
            [PrimaryKey]
            public int Primary1 { get; set; }

            [PrimaryKey]
            public int Primary2 { get; set; }
        }

        public class PrimaryString
        {
            [PrimaryKey()]
            public string PrimaryString1 { get; set; }
        }


        [TestMethod]
        public void TestAutoColumn()
        {
            TableMapper mapper = new TableMapper(true);
            var mapping = mapper.Map<PrimaryTable>();

            var column0 = GetColumnByPropertyName(mapping, nameof(PrimaryTable.Primary));

            Assert.IsTrue(column0.IsPrimaryKey, "Column 'Primary' must be IsPrimaryKey");
        }

        [TestMethod]
        public async Task Test2ColumnsPrimayKey()
        {
            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);
            var table = context.Table<DoublePrimaryKey>();
            await table.CreateTableAsync();
        }

        [TestMethod]
        public async Task CreateTable()
        {
            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);
            var table = context.Table<PrimaryTable>();
            await table.CreateTableAsync();
        }

    }
}