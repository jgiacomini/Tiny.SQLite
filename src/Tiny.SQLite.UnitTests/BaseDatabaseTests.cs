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

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            if (string.IsNullOrEmpty(_pathOfDb))
            {
                _pathOfDb = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());
            }

            if (_autoCreateDatabase)
            {
                var context = new DbContext(_pathOfDb);
                await context.Database.CreateFileAsync();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (string.IsNullOrEmpty(_pathOfDb))
            {
                if (!File.Exists(_pathOfDb))
                {
                    File.Delete(_pathOfDb);
                }
            }
        }
    }
}
