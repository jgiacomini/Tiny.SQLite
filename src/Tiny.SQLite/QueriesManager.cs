using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace TinySQLite
{
    internal class QueriesManager : IDisposable
    {
        #region Fields
        private SqliteConnection _connection;
        private InternalLogger _internalLogger;
        private bool _disposedValue = false; // To detect redundant calls
        #endregion

        public QueriesManager(SqliteConnection connection)
        {
            _connection = connection;
            _internalLogger = new InternalLogger();
        }

        public InternalLogger InternalLogger
        {
            get
            {
                return _internalLogger;
            }
        }

        public async Task OpenConnectionAsync()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task ExecuteNonQueryAsync(string sql)
        {
            using (var monitor = new QueryMonitor(sql, _internalLogger))
            {
                await OpenConnectionAsync();
                var command = _connection.CreateCommand();

                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<object> ExecuteScalarAsync(string sql)
        {
            using (var monitor = new QueryMonitor(sql, _internalLogger))
            {
                await OpenConnectionAsync();
                var command = _connection.CreateCommand();

                command.CommandText = sql;
                return await command.ExecuteScalarAsync();
            }
        }

        #region IDisposable Support
     
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_connection.State != ConnectionState.Closed)
                    {
                        _connection.Close();
                    }
                    _connection.Dispose();
                    _connection = null;
                    _internalLogger?.Dispose();
                    _internalLogger = null;
                }

                _disposedValue = true;
            }
        }

        ~QueriesManager()
        {
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
