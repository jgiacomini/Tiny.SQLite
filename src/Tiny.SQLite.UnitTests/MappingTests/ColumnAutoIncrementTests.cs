using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiny.SQLite.Attributes;
using Tiny.SQLite.Exceptions;

namespace Tiny.SQLite.UnitTests
{
    [TestClass]
    public class ColumnAutoIncrementTests : BaseColumnTest
    {
        public class AutoIncrement
        {
            [PrimaryKey(AutoIncrement = true)]
            public int AutoInc { get; set; }
        }

        public class AutoIncrementWith2Column
        {
            [PrimaryKey(AutoIncrement = true)]
            public int AutoInc1 { get; set; }

            [PrimaryKey(AutoIncrement = true)]
            public int AutoInc2 { get; set; }
        }

        public class BadAutoIncrement
        {
            [PrimaryKey(AutoIncrement = true)]
            public string AutoInc { get; set; }
        }

        [TestMethod]
        public void TestAutoColumn()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<AutoIncrement>();

            var column0 = GetColumnByPropertyName(mapping, nameof(AutoIncrement.AutoInc));

            Assert.IsTrue(column0.IsAutoIncrement, "Column 'AutoInc' must be auto incremented");
        }

        [TestMethod]
        public void Test2ColumnsAutoIncremented()
        {
            bool exceptionThrown = false;

            TableMapper mapper = new TableMapper(true, true);
            try
            {
                var mapping = mapper.Map<AutoIncrementWith2Column>();
            }
            catch (TableHaveMoreThanOneAutoIncrementedColumnException)
            {
                exceptionThrown = true;
            }

            if (exceptionThrown == false)
            {
                Assert.Fail("The creation of this table must throw TableHaveMoreThanOnePrimaryKeyException");
            }
        }

        [TestMethod]
        public async Task CreateTable()
        {
            using (var context = new DbContext(PathOfDb))
            {
                var table = context.Table<AutoIncrement>();
                await table.CreateAsync();
            }
        }

        [TestMethod]
        public async Task AutoIncOnGuidColumn()
        {
            bool exceptionThrown = false;
            using (var context = new DbContext(PathOfDb))
            {
                try
                {
                    var table = context.Table<BadAutoIncrement>();
                    await table.CreateAsync();
                }
                catch (TypeNotSupportedAutoIncrementException)
                {
                    exceptionThrown = true;
                }

                if (exceptionThrown == false)
                {
                    Assert.Fail($"The creation of this table must throw {nameof(TypeNotSupportedAutoIncrementException)}");
                }
            }
        }
    }
}