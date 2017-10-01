using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace TinySQLite
{

    public class DbContext : IDisposable
    {

#region Fields
        private readonly Dictionary<Type, TableMapping> _mappings = new Dictionary<Type, TableMapping>();
        private readonly QueriesManager _queriesManager;
        private readonly bool _storeDateTimeAsTicks;
        private readonly bool _removeDiacriticsOnTableNameAndColumnName;
        #endregion

        /// <summary>
        /// Create a Database context
        /// </summary>
        /// <param name="filePath">file path of the database</param>
        /// <param name="busyTimeout">A timeout, in milliseconds, to wait when the database is locked before throwing a SqliteBusyException
        /// The default value is 0, which means to throw a SqliteBusyException immediately if the database is locked.
        /// </param>
        /// <param name="autoCreateDatabaseFile">if true autre create database</param>
        /// <param name="removeDiacriticsOnTableNameAndColumnName">if true the tableName and column name are generated without diacritics ex : 'crèmeBrûlée' would become 'cremeBrulee'</param>
        /// <param name="storeDateTimeAsTicks">if true store dateTime as ticks(long)</param>

        public DbContext(
            string filePath,
            int busyTimeout = 0,
            bool autoCreateDatabaseFile = true,
            bool removeDiacriticsOnTableNameAndColumnName = true,
            bool storeDateTimeAsTicks = false)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException($"{nameof(filePath)}");
            }

            _removeDiacriticsOnTableNameAndColumnName = removeDiacriticsOnTableNameAndColumnName;
            _storeDateTimeAsTicks = storeDateTimeAsTicks;
            var connection = new SqliteConnection($"Data Source={filePath},busy_timeout={busyTimeout}");

            _queriesManager = new QueriesManager(connection);
            Database = new Database(_queriesManager, filePath);
            if (autoCreateDatabaseFile)
            {
                Database.CreateFile();
            }
        }

        private DbContext(
            int busyTimeout,
            bool removeDiacriticsOnTableNameAndColumnName, 
            bool storeDateTimeAsTicks)
        {
            var connection = new SqliteConnection("URI=file::memory:,version=3");
            _storeDateTimeAsTicks = storeDateTimeAsTicks;
            _removeDiacriticsOnTableNameAndColumnName = removeDiacriticsOnTableNameAndColumnName;
            _queriesManager = new QueriesManager(connection);
            Database = new Database(_queriesManager, null);
        }

        /// <summary>
        /// Create a in memory Database context
        /// </summary>
        /// <param name="busyTimeout">A timeout, in milliseconds, to wait when the database is locked before throwing a SqliteBusyException
        /// The default value is 0, which means to throw a SqliteBusyException immediately if the database is locked.
        /// </param>
        /// <param name="removeDiacriticsOnTableNameAndColumnName">if true the tableName and column name are generated without diacritics ex : 'crèmeBrûlée' would become 'cremeBrulee'</param>
        /// <param name="storeDateTimeAsTicks">if true store dateTime as ticks(long)</param>
        public static DbContext InMemory(
            int busyTimeout = 0,
            bool removeDiacriticsOnTableNameAndColumnName = true,
            bool storeDateTimeAsTicks = false)
        {
            return new DbContext(busyTimeout, removeDiacriticsOnTableNameAndColumnName, storeDateTimeAsTicks);
        }

        public Database Database { get; private set; }

        public TableQuery<T> Table<T>() where T : new()
        {
            var type = typeof(T);
            TableMapping mapping = null;
            if (_mappings.ContainsKey(type))
            {
                mapping = _mappings[type];
            }
            else
            {
                var mapper = new TableMapper(_storeDateTimeAsTicks, _removeDiacriticsOnTableNameAndColumnName);
                mapping = mapper.Map(type);
                _mappings.Add(type, mapping);
            }

            return new TableQuery<T>(mapping, _queriesManager);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Database.Dispose();
                    _queriesManager.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        ~DbContext()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}