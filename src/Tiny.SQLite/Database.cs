using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tiny.SQLite
{
    public sealed class Database
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

        private void OnLog(string onLog)
        {
            Log?.Invoke(onLog);
        }

        /// <summary>
        /// Get the SQLite version
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>return the version of SQLite database</returns>
        public async Task<string> GetSQLiteVersionAsync(CancellationToken cancellationToken = default)
        {
            var version = await _queriesManager.ExecuteScalarAsync("SELECT sqlite_version();", cancellationToken);

            return version as string;
        }

        /// <summary>
        /// Get the user version of database.
        /// The user_version pragma will to get or set the value of the user-version integer at offset 60 in the database header. The user-version is an integer that is available to applications to use however they want. SQLite makes no use of the user-version itself.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>return the user version of SQLite database</returns>
        public async Task<long> GetUserVersionAsync(CancellationToken cancellationToken = default)
        {
            var version = await _queriesManager.ExecuteScalarAsync("PRAGMA user_version;", cancellationToken);

            return (long)version;
        }

        /// <summary>
        /// Set the user version of database
        /// The user_version pragma will to get or set the value of the user-version integer at offset 60 in the database header. The user-version is an integer that is available to applications to use however they want. SQLite makes no use of the user-version itself.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="userVersion">user version of database</param>
        /// <returns>return a Task</returns>
        public async Task SetUserVersionAsync(long userVersion, CancellationToken cancellationToken = default)
        {
            var version = await _queriesManager.ExecuteScalarAsync($"PRAGMA user_version = {userVersion};", cancellationToken);
        }

        /// <summary>
        /// The VACUUM command rebuilds the database file, repacking it into a minimal amount of disk space.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>return a Task</returns>
        /// <remarks>The ability to vacuum attached databases was added in version 3.15.0 (2016-10-14). Prior to that, a schema-name added to the VACUUM statement would be silently ignored and the "main" schema would be vacuumed.</remarks>
        public async Task VacuumAsync(CancellationToken cancellationToken = default)
        {
            await _queriesManager.ExecuteScalarAsync("VACUUM;", cancellationToken);
        }

        /// <summary>
        /// Enable Write-Ahead Log.
        /// There are advantages and disadvantages to using WAL instead of a rollback journal. Advantages include:
        /// - WAL is significantly faster in most scenarios.
        /// - WAL provides more concurrency as readers do not block writers and a writer does not block readers. Reading and writing can proceed concurrently.
        /// - Disk I/O operations tends to be more sequential using WAL.
        /// - WAL uses many fewer fsync() operations and is thus less vulnerable to problems on systems where the fsync() system call is broken.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>return a Task</returns>
        public async Task EnableWALAsync(CancellationToken cancellationToken = default)
        {
            await _queriesManager.ExecuteScalarAsync("PRAGMA journal_mode = WAL;", cancellationToken);
        }

        /// <summary>
        /// Disable the WAL
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>return a Task</returns>
        public async Task DisableWALAsync(CancellationToken cancellationToken = default)
        {
            await _queriesManager.ExecuteScalarAsync("PRAGMA journal_mode = DELETE;", cancellationToken);
        }

        /// <summary>
        /// Get the amout of time a table is locked before to throw SQLITE_BUSY exception
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>returns the amount of time a table is locked before throw  SQLITE_BUSY exception</returns>
        public async Task<TimeSpan> GetBusyTimeoutAsync(CancellationToken cancellationToken = default)
        {
            var milliSeconds = (long)await _queriesManager.ExecuteScalarAsync("PRAGMA busy_timeout;", cancellationToken);
            return TimeSpan.FromMilliseconds(milliSeconds);
        }

        /// <summary>
        /// Get the amout of time a table is locked before to throw SQLITE_BUSY exception
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns></returns>
        public async Task SetBusyTimeoutAsync(TimeSpan timeSpan, CancellationToken cancellationToken = default)
        {
            await _queriesManager.ExecuteScalarAsync($"PRAGMA busy_timeout = {timeSpan.TotalMilliseconds};", cancellationToken);
        }
    }
}