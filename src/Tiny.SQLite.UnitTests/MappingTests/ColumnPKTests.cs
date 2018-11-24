using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnPKTests : BaseColumnTest
    {
        public ColumnPKTests() : base(true)
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
        public void PK_OneColumn()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<PrimaryTable>();

            var column0 = GetColumnByPropertyName(mapping, nameof(PrimaryTable.Primary));

            Assert.IsTrue(column0.IsPrimaryKey, "Column 'Primary' must be IsPrimaryKey");
        }

        [TestMethod]
        public async Task PK_2ColumnsPrimayKey()
        {
            var context = new DbContext(_pathOfDb);
            var table = context.Table<DoublePrimaryKey>();
            await table.CreateAsync();
        }

        [TestMethod]
        public async Task PK_CreateTable()
        {
            var context = new DbContext(_pathOfDb);
            var table = context.Table<PrimaryTable>();
            await table.CreateAsync();
        }

    }
}