using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;
using TinySQLite.Exceptions;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnAutoIncTests : BaseColumnTest
    {
        public ColumnAutoIncTests() : base(true)
        {
        }

        public class AutInc
        {
            [PrimaryKey(AutoIncrement =  true)]
            public int AutoInc { get; set; }
        }

        public class AutIncWith2Column
        {
            [PrimaryKey(AutoIncrement = true)]
            public int AutoInc1 { get; set; }

            [PrimaryKey(AutoIncrement = true)]
            public int AutoInc2 { get; set; }
        }

        public class BadAutInc
        {
            [PrimaryKey(AutoIncrement = true)]
            public string AutoInc { get; set; }
        }


        [TestMethod]
        public void TestAutoColumn()
        {
            TableMapper mapper = new TableMapper(true);
            var mapping = mapper.Map<AutInc>();

            var column0 = GetColumnByPropertyName(mapping, nameof(AutInc.AutoInc));

            Assert.IsTrue(column0.IsAutoIncrement, "Column 'AutoInc' must be auto incremented");
        }

        [TestMethod]
        public void Test2ColumnsAutoIncremented()
        {
            bool exceptionThrown = false;

            TableMapper mapper = new TableMapper(true);
            try
            {

                var mapping = mapper.Map<AutIncWith2Column>();
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
            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);
            var table = context.Table<AutInc>();
            await table.CreateTableAsync();
        }

        [TestMethod]
        public async Task AutoIncOnGuidColumn()
        {
            bool exceptionThrown = false;
            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);

            try
            {
                var table = context.Table<BadAutInc>();
                await table.CreateTableAsync();
            }
            catch (TypeNotSupportedAutoIncrementException)
            {
                exceptionThrown = true;
            }

            if (exceptionThrown == false)
            {
                Assert.Fail("The creation of this table must throw TypeNotSupportedAutoIncrementException");
            }
        }

    }
}