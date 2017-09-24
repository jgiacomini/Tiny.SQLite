using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using TinySQLite.Attributes;

namespace TinySQLite
{

    public class DbContext : IDisposable
    {
        private readonly Dictionary<Type, TableMapping> _mappings = new Dictionary<Type, TableMapping>();

        private readonly SqliteConnection _connection;

        private readonly bool _storeDateTimeAsTicks;

        /// <summary>
        /// Create a Database context
        /// </summary>
        /// <param name="filePath">file path of the database</param>
        /// <param name="busyTimeout">A timeout, in milliseconds, to wait when the database is locked before throwing a SqliteBusyException
        /// The default value is 0, which means to throw a SqliteBusyException immediately if the database is locked.
        /// </param>
        /// <param name="storeDateTimeAsTicks">if true store dateTime as ticks(long)</param>

        public DbContext(string filePath, int busyTimeout = 0, bool autoCreateDatabaseFile = true, bool storeDateTimeAsTicks = false)
        {
            _connection = new SqliteConnection($"Data Source={filePath},busy_timeout={busyTimeout}");
            _storeDateTimeAsTicks = storeDateTimeAsTicks;
            Database = new Database(_connection, filePath);
            if (autoCreateDatabaseFile)
            {
                Database.CreateFile();
            }
        }

        private DbContext(int busyTimeout = 0, bool autoCreateDatabaseFile = true, bool storeDateTimeAsTicks = false)
        {
            _connection = new SqliteConnection("URI=file::memory:,version=3");
            _storeDateTimeAsTicks = storeDateTimeAsTicks;
            Database = new Database(_connection, null);
            if (autoCreateDatabaseFile)
            {
                Database.CreateFile();
            }
        }

        public static DbContext InMemory(int busyTimeout = 0, bool autoCreateDatabaseFile = true, bool storeDateTimeAsTicks = false)
        {
            return new DbContext(busyTimeout, storeDateTimeAsTicks);
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
                var mapper = new TableMapper(_storeDateTimeAsTicks);
                mapping = mapper.Map(type);
                _mappings.Add(type, mapping);
            }

            return new TableQuery<T>(mapping, _connection);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
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
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    internal static class QueryBuilder
    {
        public static string EscapeTableName(this string tableName)
        {
            return $"[{tableName}]";
        }
        
        public static string EscapeColumnName(this string columnName)
        {
            return $"[{columnName}]";
        }

    }

    public class Database
    {
        private readonly SqliteConnection _connection;
        private readonly string _filePath;
        public Database(SqliteConnection connection, string filePath)
        {
            _connection = connection;
            _filePath = filePath;
        }
        public void CreateFile()
        {
            if (File.Exists(_filePath))
            {
                SqliteConnection.CreateFile(_filePath);
            }
        }
    }

    public class TableQuery<T>
    {
        private readonly TableMapping _mapping;
        private readonly SqliteConnection _connection;

        internal TableQuery(TableMapping mapping, SqliteConnection connection)
        {
            _mapping = mapping;
            _connection = connection;
        }

        public Task CreateTableAsync()
        {
            var queryBuilder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {_mapping.TableName.EscapeTableName()}(\n");

            var lastColumn = _mapping.Columns.Last();
            foreach (var column in _mapping.Columns)
            {
                queryBuilder.Append($"{column.ColumnName.EscapeColumnName()} {column.ColumnType} ");

                if (column.IsPK)
                {
                    queryBuilder.Append("PRIMARY KEY ");
                }
                if (column.IsAutoInc)
                {
                    queryBuilder.Append("AUTOINCREMENT ");
                }
                if (!column.IsNullable)
                {
                    queryBuilder.Append("NOT NULL ");
                }
                if (column.Collate != Collate.Binary)
                {
                    queryBuilder.Append($"COLLATE {column.Collate} ");
                }

                if (column == lastColumn)
                {
                    queryBuilder.Append(");");
                }
                else
                {
                    queryBuilder.Append(",");
                }

                queryBuilder.AppendLine();

            }
            queryBuilder.AppendLine();

            foreach (var column in _mapping.Columns)
            {
                if (column.IsUnique)
                {
                    queryBuilder.Append($"CREATE UNIQUE INDEX IF NOT EXISTS {("IX_UNIQUE_" + _mapping.TableName).EscapeColumnName()} ON {_mapping.TableName.EscapeTableName()}({column.ColumnName.EscapeColumnName()});");
                }
            }

            //Collating (Binary, NOCASE, RTRIM)
            return ExecuteNonQueryAsync(queryBuilder.ToString());
        }

        private string GetCollate(Collate collate)
        {
            switch (collate)
            {
                case Collate.Binary:
                    return "BINARY";
                case Collate.RTrim:
                    return "RTRIM";
                case Collate.NoCase:
                    return "NOCASE";
                default:
                    throw new NotImplementedException();
            }
        }
        
        public async Task<bool> ExistsAsync()
        {
            var result = await ExecuteScalarAsync($"SELECT Count(name) FROM sqlite_master WHERE type = 'table' AND name = '{_mapping.TableName}'");
            return Convert.ToBoolean(result);
        }
        public Task DropTableAsync()
        {
            var query = $"DROP TABLE IF EXISTS \"{_mapping.TableName.EscapeTableName()}\";";
            return ExecuteNonQueryAsync(query);
        }

        private async Task ExecuteNonQueryAsync(string sql)
        {
            await _connection.OpenAsync();

            var command = _connection.CreateCommand();

            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();

            _connection.Close();
        }

        private async Task<object> ExecuteScalarAsync(string sql)
        {
            await _connection.OpenAsync();

            var command = _connection.CreateCommand();

            command.CommandText = sql;
            return await command.ExecuteScalarAsync();
        }
    }
}