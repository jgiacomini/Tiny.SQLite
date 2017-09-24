using System;
using System.Data;
using System.Threading.Tasks;
using Mono.Data.Sqlite;

namespace TinySQLite
{
    internal class QueriesManager : IDisposable
    {
        #region Fields
        private SqliteConnection _connection;
        #endregion

        public QueriesManager(SqliteConnection connection)
        {
            _connection = connection;
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
            await OpenConnectionAsync();
            var command = _connection.CreateCommand();

            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string sql)
        {
            await OpenConnectionAsync();
            var command = _connection.CreateCommand();

            command.CommandText = sql;
            return await command.ExecuteScalarAsync();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

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
