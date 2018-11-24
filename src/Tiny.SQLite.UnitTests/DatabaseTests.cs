using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class DatabaseTests : BaseDatabaseTests
    {
        [TestMethod]
        public async Task TestVacuumAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb, false))
            {
                await dbContext.Database.VacuumAsync();
            }
        }

        [TestMethod]
        public async Task GetVersionOfSQLiteDatabaseAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb, false))
            {
                var version = await dbContext.Database.GetSQLiteVersionAsync();

                System.Diagnostics.Debug.WriteLine(version);

                Assert.IsNotNull(version);
            }
        }

        [TestMethod]
        public async Task GetUserVersionAsync()
        {
            const long currentUserVersion = 7L;
            using (DbContext dbContext = new DbContext(PathOfDb, false))
            {
                dbContext.Database.Log = WriteLine;
                await dbContext.Database.SetUserVersionAsync(currentUserVersion);

                var version = await dbContext.Database.GetUserVersionAsync();

                System.Diagnostics.Debug.WriteLine(version);

                Assert.IsTrue(version == currentUserVersion);
            }
        }

        private void WriteLine(string line)
        {
            Debug.WriteLine(line);
        }

        [TestMethod]
        public async Task TestWALAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb, false))
            {
                await dbContext.Database.EnableWALAsync();
                await dbContext.Database.DisableWALAsync();
            }
        }

        [TestMethod]
        public async Task TestBusyTimeoutAsync()
        {
            using (DbContext dbContext = new DbContext(PathOfDb, false))
            {
                var timeout = await dbContext.Database.GetBusyTimeoutAsync();

                Assert.AreEqual(timeout, TimeSpan.FromSeconds(0));
                var specifiedTimeout = TimeSpan.FromSeconds(10);
                await dbContext.Database.SetBusyTimeoutAsync(specifiedTimeout);
                timeout = await dbContext.Database.GetBusyTimeoutAsync();
                Assert.AreEqual(timeout, specifiedTimeout);
            }
        }
    }
}
