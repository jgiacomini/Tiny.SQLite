using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TinySQLite.Net.UnitTests
{
    public abstract class BaseDatabaseTests
    {
        protected string _pathOfDb;
        private readonly bool _autoCreateDatabase;
        
        public BaseDatabaseTests(bool autoCreateDatabase)
        {
            _autoCreateDatabase = autoCreateDatabase;
        }

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            if (string.IsNullOrEmpty(_pathOfDb))
            {
                _pathOfDb = Path.Combine(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), $"{TestContext.TestName}.db"));
            }

            if (File.Exists(_pathOfDb))
            {
                File.Delete(_pathOfDb);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (string.IsNullOrEmpty(_pathOfDb))
            {
                if (File.Exists(_pathOfDb))
                {
                    File.Delete(_pathOfDb);
                }
            }
        }
    }
}
