using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiny.SQLite.Attributes;

namespace Tiny.SQLite.UnitTests
{
    [TestClass]
    public class InsertTests : BaseDatabaseTests
    {
        public class InsertTable
        {
            [PrimaryKey(AutoIncrement =true)]
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }

        [TestMethod]
        public async Task TestInsertAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb, false))
            {
                var table = dbContext.Table<InsertTable>();
                await table.CreateAsync();

                await table.InsertAsync(new InsertTable() { Name = "TOTO", Date = DateTime.Now });

                var count = await table.CountAsync();
                Assert.IsTrue(count == 1);
            }
        }

        [TestMethod]
        public async Task TestMultipleInsertAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb, false))
            {
                var table = dbContext.Table<InsertTable>();
                await table.CreateAsync();

                var toInsert = new List<InsertTable>()
                {
                    new InsertTable() { Name = "VAL1", Date = DateTime.Now },
                    new InsertTable() { Name = "VAL2", Date = DateTime.Now },
                    new InsertTable() { Name = "VAL3", Date = DateTime.Now },
                    new InsertTable() { Name = "VAL4", Date = DateTime.Now },
                    new InsertTable() { Name = "VAL5", Date = DateTime.Now }
                };

                await table.InsertAsync(toInsert);

                var count = await table.CountAsync();
                Assert.IsTrue(count == 5);
            }
        }
    }
}
