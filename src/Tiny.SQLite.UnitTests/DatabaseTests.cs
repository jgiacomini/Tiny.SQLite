using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class DatabaseTests : BaseDatabaseTests
    {
        public DatabaseTests() : base(true)
        {
        }
        
        [TestMethod]
        public async Task TestVacuumAsync()
        {
            DbContext dbContext = new DbContext(_pathOfDb, 0, false);
            await dbContext.Database.VacuumAsync();
        }

        [TestMethod]
        public async Task GetVersionOfSQLiteDatabaseAsync()
        {
            DbContext dbContext = new DbContext(_pathOfDb, 0, false);

            var version = await dbContext.Database.GetSQLiteVersionAsync();

            System.Diagnostics.Debug.WriteLine(version);

            Assert.IsNotNull(version);
        }

        [TestMethod]
        public async Task GetUserVersionAsync()
        {
            const long currentUserVersion = 7L;
            DbContext dbContext = new DbContext(_pathOfDb, 0, false);

            dbContext.Database.Log = WriteLine;
            await dbContext.Database.SetUserVersionAsync(currentUserVersion);

            var version = await dbContext.Database.GetUserVersionAsync();

            System.Diagnostics.Debug.WriteLine(version);

            Assert.IsTrue(version == currentUserVersion);
        }

        void WriteLine(string line)
        {
            Debug.WriteLine(line);
        }

        [TestMethod]
        public async Task TestWALAsync()
        {
            DbContext dbContext = new DbContext(_pathOfDb, 0, false);

            await dbContext.Database.EnableWALAsync();
            await dbContext.Database.DisableWALAsync();
        }
    }
}
