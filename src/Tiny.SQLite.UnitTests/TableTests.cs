using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Tiny.SQLite.UnitTests
{
    [TestClass]
    public class TableTests : BaseDatabaseTests
    {
        internal class SmallTable
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [TestMethod]
        public async Task TestCreateTableAsync()
        {
            using (DbContext dbContext = DbContext.InMemory())
            {
                await dbContext.Table<SmallTable>().CreateAsync();
                Assert.IsTrue(await dbContext.Table<SmallTable>().ExistsAsync());
            }
        }

        [TestMethod]
        public async Task TestDropTableAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb))
            {
                await dbContext.Table<SmallTable>().CreateAsync();

                await dbContext.Table<SmallTable>().DropAsync();
                Assert.IsFalse(await dbContext.Table<SmallTable>().ExistsAsync());
            }
        }

        [TestMethod]
        public async Task TestCountTableAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb))
            {
                await dbContext.Table<SmallTable>().CreateAsync();
                var table = dbContext.Table<SmallTable>();

                var count = await table.CountAsync();
                Assert.AreEqual(count, 0);
                await table.InsertAsync(new SmallTable()
                {
                    Name = "TEST"
                });

                count = await dbContext.Table<SmallTable>().CountAsync();
                Assert.AreEqual(count, 1);
            }
        }
    }
}
