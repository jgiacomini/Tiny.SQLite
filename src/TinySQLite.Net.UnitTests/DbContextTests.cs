using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class DbContextTests : BaseDatabaseTests
    {
        public DbContextTests() : base(false)
        {
        }

        [TestMethod]
        public void CreateAutoCreateDatabase()
        {
            DbContext dbContext = new DbContext(_pathOfDb, 0, true);
            Assert.IsTrue(File.Exists(_pathOfDb));
        }

        [TestMethod]
        public void CreateDatabase()
        {
            DbContext dbContext = new DbContext(_pathOfDb, 0, false);
            
            dbContext.Database.CreateFile();

            Assert.IsTrue(File.Exists(_pathOfDb));
        }

        [TestMethod]
        public void CreateInMemoryDatabase()
        {
            DbContext dbContext = DbContext.InMemory();
            dbContext.Database.CreateFile();

            Assert.IsTrue(File.Exists(_pathOfDb));
        }

       
    }
}
