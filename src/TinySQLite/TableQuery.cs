using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using TinySQLite.Attributes;

namespace TinySQLite
{

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