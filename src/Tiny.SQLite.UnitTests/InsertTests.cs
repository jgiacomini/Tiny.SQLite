using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

                var toInsert = new InsertTable() { Name = "ITEM", Date = DateTime.Now };
                await table.InsertAsync(toInsert);

                Assert.IsTrue(toInsert.Id == 1);

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

                var toInsert = new List<InsertTable>();

                var date = DateTime.Now;
                for (int i = 0; i < 10; i++)
                {
                    toInsert.Add(new InsertTable() { Name = $"VAL{i}", Date = date.AddMinutes(i) });
                }

                await table.InsertAsync(toInsert);

                var count = await table.CountAsync();
                Assert.IsTrue(count == toInsert.Count);

                for (int i = 0; i < toInsert.Count; i++)
                {
                    Assert.IsTrue(toInsert[i].Id == i + 1);
                    Assert.IsTrue(toInsert[i].Date == date.AddMinutes(i));
                }
            }
        }
    }
}
