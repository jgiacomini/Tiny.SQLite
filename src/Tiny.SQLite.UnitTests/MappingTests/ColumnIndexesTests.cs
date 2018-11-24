using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;
using TinySQLite.Exceptions;
using System.Linq;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnIndexesTests : BaseColumnTest
    {
        public ColumnIndexesTests() : base(true)
        {
        }

        public class IndexTable
        {
            [Indexed("IX_INDEX", 0)]
            public int Index { get; set; }
        }

        public class IndexOnMultiColumns
        {
            [Indexed("IX_INDEX", 1)]
            public int Index2 { get; set; }
            [Indexed("IX_INDEX", 0)]
            public int Index1 { get; set; }
        }

        public class IndexOnMultiColumnsUnique
        {
            [Indexed("IX_INDEX", 1, IsUnique = true)]
            public int Index2 { get; set; }
            [Indexed("IX_INDEX", 0, IsUnique = true)]
            public int Index1 { get; set; }
        }

        public class IndexOnMultiColumnsButWithNotSameUniqueValue
        {
            [Indexed("IX_INDEX", 1, IsUnique = true)]
            public int Index2 { get; set; }
            [Indexed("IX_INDEX", 0, IsUnique = false)]
            public int Index1 { get; set; }
        }


        public class IndexOnMultiColumnsButWithNotDifferentUniqueValue
        {
            [Indexed("IX_INDEX", 1, IsUnique = true)]
            public int Index2 { get; set; }
            [Indexed("IX_INDEX", 0, IsUnique = false)]
            public int Index1 { get; set; }
        }



        [TestMethod]
        public void TestSimpleIndexColumn()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<IndexTable>();

            var column0 = GetColumnByPropertyName(mapping, nameof(IndexTable.Index));

            var index = mapping.Indexes.FirstOrDefault(i => i.Columns.Contains(column0));

            Assert.IsNotNull(index, $"Column '{nameof(IndexTable.Index)}' must have an index");
            Assert.IsFalse(index.IsUnique, $"Column '{nameof(IndexTable.Index)}'  must be no unique");
        }


        [TestMethod]
        public async Task CreateTableWithSimpleIndex()
        {
            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);
            var table = context.Table<IndexTable>();
            await table.CreateAsync();
        }

        [TestMethod]
        public async Task TestSimpleIndexonMultiColumns()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<IndexOnMultiColumns>();

            var column0 = GetColumnByPropertyName(mapping, nameof(IndexOnMultiColumns.Index1));
            var column1 = GetColumnByPropertyName(mapping, nameof(IndexOnMultiColumns.Index2));

            var index = mapping.Indexes.FirstOrDefault(i => i.Columns.Contains(column0));

            Assert.IsNotNull(index, $"Column '{nameof(IndexOnMultiColumns.Index1)}' must have an index");
            Assert.IsFalse(index.IsUnique, $"Column '{nameof(IndexOnMultiColumns.Index2)}' must be not unique");

            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);
            var table = context.Table<IndexOnMultiColumns>();
            await table.CreateAsync();
        }

        [TestMethod]
        public async Task TestSimpleIndexonMultiBugColumns()
        {
            bool exceptionThrown = false;

            try
            {
                var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);

                var table = context.Table<IndexOnMultiColumnsButWithNotDifferentUniqueValue>();
                await table.CreateAsync();
            }
            catch (AllColumnsInIndexMustHaveTheSameIsUniqueValueException)
            {
                exceptionThrown = true;
            }

            if (exceptionThrown == false)
            {
                Assert.Fail($"The creation of this table must throw {nameof(AllColumnsInIndexMustHaveTheSameIsUniqueValueException)}");
            }
        }
    }
}