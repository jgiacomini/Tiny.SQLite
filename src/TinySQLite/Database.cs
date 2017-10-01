using System;
using System.IO;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace TinySQLite
{

    public class Database : IDisposable
    {
        #region Fields
        private readonly QueriesManager _queriesManager;
        private readonly string _filePath;
        #endregion

        #region Delegates
        public Action<string> Log { get; set; }
        #endregion
        internal Database(QueriesManager queriesManager, string filePath)
        {
            _queriesManager = queriesManager;
            _queriesManager.InternalLogger.OnLog = OnLog;
            _filePath = filePath;
        }

        void OnLog(string onLog)
        {
            Log?.Invoke(onLog);
        }

        /// <summary>
        /// Create database
        /// </summary>
        public void CreateFile()
        {
            if (_filePath != null && !File.Exists(_filePath))
            {
                SqliteConnection.CreateFile(_filePath);
            }
        }
        
        /// <summary>
        /// Get the SQLite version
        /// </summary>
        /// <returns>return the version of SQLite database</returns>
        public async Task<string> GetSQLiteVersionAsync()
        {
            var version = await _queriesManager.ExecuteScalarAsync("SELECT sqlite_version();");

            return version as string;
        }

        /// <summary>
        /// Get the user version of database.
        /// The user_version pragma will to get or set the value of the user-version integer at offset 60 in the database header. The user-version is an integer that is available to applications to use however they want. SQLite makes no use of the user-version itself. 
        /// </summary>
        /// <returns>return the user version of SQLite database</returns>
        public async Task<long> GetUserVersionAsync()
        {
            var version = await _queriesManager.ExecuteScalarAsync("PRAGMA user_version;");

            return (long)version;
        }

        /// <summary>
        /// Set the user version of database
        /// The user_version pragma will to get or set the value of the user-version integer at offset 60 in the database header. The user-version is an integer that is available to applications to use however they want. SQLite makes no use of the user-version itself. 
        /// </summary>
        /// <param name="userVersion">user version of database</param>
        /// <returns>return a Task</returns>
        public async Task SetUserVersionAsync(long userVersion)
        {
            var version = await _queriesManager.ExecuteScalarAsync($"PRAGMA user_version = {userVersion};");
        }

        /// <summary>
        /// The VACUUM command rebuilds the database file, repacking it into a minimal amount of disk space. 
        /// </summary>
        /// <returns>return a Task</returns>
        /// <remarks>The ability to vacuum attached databases was added in version 3.15.0 (2016-10-14). Prior to that, a schema-name added to the VACUUM statement would be silently ignored and the "main" schema would be vacuumed.</remarks>
        public async Task VacuumAsync()
        {
            await _queriesManager.ExecuteScalarAsync("VACUUM;");
        }

        /// <summary>
        /// Enable Write-Ahead Log.
        ///There are advantages and disadvantages to using WAL instead of a rollback journal. Advantages include:
        /// - WAL is significantly faster in most scenarios.
        /// - WAL provides more concurrency as readers do not block writers and a writer does not block readers. Reading and writing can proceed concurrently.
        /// - Disk I/O operations tends to be more sequential using WAL.
        /// - WAL uses many fewer fsync() operations and is thus less vulnerable to problems on systems where the fsync() system call is broken.
        /// </summary>  
        /// <returns>return a Task</returns>
        public async Task EnableWALAsync()
        {
            await _queriesManager.ExecuteScalarAsync("PRAGMA journal_mode = WAL;");
        }

        /// <summary>
        /// Disable the WAL
        /// </summary>
        /// <returns>return a Task</returns>
        public async Task DisableWALAsync()
        {
            await _queriesManager.ExecuteScalarAsync("PRAGMA journal_mode = DELETE;");
        }

        public void Dispose()
        {
            Log = null;
        }
    }
}