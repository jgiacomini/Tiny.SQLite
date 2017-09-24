using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinySQLite.Attributes;

namespace TinySQLite
{
    public class TableQuery<T>
    {
        private readonly TableMapping _mapping;
        private readonly QueriesManager _queriesManager;

        internal TableQuery(TableMapping mapping, QueriesManager queriesManager)
        {
            _mapping = mapping;
            _queriesManager = queriesManager;
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

            return _queriesManager.ExecuteNonQueryAsync(queryBuilder.ToString());
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
            var result = await _queriesManager.ExecuteScalarAsync($"SELECT Count(name) FROM sqlite_master WHERE type = 'table' AND name = '{_mapping.TableName}'");
            return Convert.ToBoolean(result);
        }
        public Task DropTableAsync()
        {
            var query = $"DROP TABLE IF EXISTS \"{_mapping.TableName.EscapeTableName()}\";";
            return _queriesManager.ExecuteNonQueryAsync(query);
        }

    }
}