using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnUniqueTest : BaseColumnTest
    {
        public ColumnUniqueTest() : base(true)
        {

        }
        class UniqueColumnsTable
        {
            [Unique()]
            public string Unique { get; set; }

            public string NotUnique { get; set; }
        }

        [TestMethod]
        public void CollumnUnique_Test()
        {
            TableMapper mapper = new TableMapper(true);
            var mapping = mapper.Map<UniqueColumnsTable>();
            var column = GetColumnByPropertyName(mapping,
                nameof(UniqueColumnsTable.Unique));

            Assert.IsTrue(column.IsUnique,
                "string without attribute must be nullable");

            var columnNotUnique = GetColumnByPropertyName(mapping, nameof(UniqueColumnsTable.NotUnique));
            Assert.IsFalse(columnNotUnique.IsUnique,
                "column without attribute Unique must be not unique");
        }

        [TestMethod]
        public async Task CreateTableWithUniqueColumnTypes()
        {
            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);

            try
            {
                var table = context.Table<UniqueColumnsTable>();
                await table.CreateTableAsync();
                Assert.IsTrue(await table.ExistsAsync());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }


    }
}
