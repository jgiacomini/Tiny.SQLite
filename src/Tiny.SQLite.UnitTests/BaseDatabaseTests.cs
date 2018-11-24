using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tiny.SQLite.UnitTests
{
    public abstract class BaseDatabaseTests
    {
        public TestContext TestContext { get; set; }
        protected string PathOfDb { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            if (string.IsNullOrEmpty(PathOfDb))
            {
                var directoryPath = Path.Combine(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Tiny.Sqlite"));
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                PathOfDb = Path.Combine(directoryPath, $"{GetType().Name}_{TestContext.TestName}.db");
            }

            if (File.Exists(PathOfDb))
            {
                File.Delete(PathOfDb);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (!string.IsNullOrEmpty(PathOfDb))
            {
                if (File.Exists(PathOfDb))
                {
                    File.Delete(PathOfDb);
                }
            }
        }
    }
}
