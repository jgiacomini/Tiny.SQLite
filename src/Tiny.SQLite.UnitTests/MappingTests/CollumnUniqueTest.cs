using System;
using System.Linq;
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
            [Indexed("IX_UQ_UNIQUE", 0, IsUnique = true)]
            public string Unique { get; set; }

            public string NotUnique { get; set; }
        }

        [TestMethod]
        public void CollumnUnique_Test()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<UniqueColumnsTable>();
            var column = GetColumnByPropertyName(mapping,
                nameof(UniqueColumnsTable.Unique));
            var index = mapping.Indexes.FirstOrDefault(i => i.Columns.Contains(column));

            Assert.IsNotNull(index,
                $"column {nameof(UniqueColumnsTable.Unique)} must have index");
            Assert.IsTrue(index.IsUnique,
                $"column {nameof(UniqueColumnsTable.Unique)} must have unique index");

            var columnNotUnique = GetColumnByPropertyName(mapping, nameof(UniqueColumnsTable.NotUnique));
            var noIndex = mapping.Indexes.FirstOrDefault(i => i.Columns.Contains(columnNotUnique));

            Assert.IsNull(noIndex,
                "column without attribute Unique must be not unique");
        }

        [TestMethod]
        public async Task CreateTableWithUniqueColumnTypes()
        {
            var context = new DbContext(_pathOfDb);

            try
            {
                var table = context.Table<UniqueColumnsTable>();
                await table.CreateAsync();
                Assert.IsTrue(await table.ExistsAsync());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
