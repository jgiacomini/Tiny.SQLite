using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiny.SQLite.Attributes;

namespace Tiny.SQLite.UnitTests
{
    [TestClass]
    public class ColumnPKTests : BaseColumnTest
    {
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
            [PrimaryKey]
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
            using (var context = new DbContext(PathOfDb))
            {
                var table = context.Table<DoublePrimaryKey>();
                await table.CreateAsync();
            }
        }

        [TestMethod]
        public async Task PK_CreateTable()
        {
            using (var context = new DbContext(PathOfDb))
            {
                var table = context.Table<PrimaryTable>();
                await table.CreateAsync();
            }
        }
    }
}