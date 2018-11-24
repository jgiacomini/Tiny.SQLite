using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tiny.SQLite.Attributes;

namespace Tiny.SQLite
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

        public Task CreateAsync(CancellationToken cancellationToken = default)
        {
            var queryBuilder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {_mapping.TableName.EscapeTableName()}(\n");

            var lastColumn = _mapping.Columns.Last();
            bool primaryKeysAlreadyCreated = false;
            bool hasPrimaryKey = false;
            foreach (var column in _mapping.Columns)
            {
                queryBuilder.Append($"{column.ColumnName.EscapeColumnName()} {column.ColumnType} ");

                if (column.IsPrimaryKey)
                {
                    hasPrimaryKey = true;
                    if (column.IsAutoIncrement)
                    {
                        queryBuilder.Append("PRIMARY KEY AUTOINCREMENT ");
                        primaryKeysAlreadyCreated = true;
                    }
                }

                if (!column.IsNullable)
                {
                    queryBuilder.Append("NOT NULL ");
                }

                if (column.Collate != Collate.Binary)
                {
                    queryBuilder.Append($"COLLATE {GetCollate(column.Collate)} ");
                }

                if (column == lastColumn)
                {
                    // Add primary keys
                    // PRIMARY KEY(column_1, column_2,...)
                    if (!primaryKeysAlreadyCreated && hasPrimaryKey)
                    {
                        var columnsNames = string.Join(",", _mapping.Columns.Where(c => c.IsPrimaryKey).Select(p => p.ColumnName.EscapeColumnName()));
                        queryBuilder.AppendLine();
                        queryBuilder.AppendLine($", PRIMARY KEY({columnsNames}) ");
                    }

                    queryBuilder.Append(");");
                }
                else
                {
                    queryBuilder.Append(",");
                }

                queryBuilder.AppendLine();
            }

            queryBuilder.AppendLine();

            foreach (var index in _mapping.Indexes)
            {
                var columns = string.Join(",", index.Columns.Select(t => t.ColumnName.EscapeColumnName()));

                if (index.IsUnique)
                {
                    queryBuilder.Append($"CREATE UNIQUE INDEX IF NOT EXISTS {index.Name} ON {_mapping.TableName.EscapeTableName()}({columns});");
                }
                else
                {
                    queryBuilder.Append($"CREATE INDEX IF NOT EXISTS {index.Name} ON {_mapping.TableName.EscapeTableName()}({columns});");
                }
            }

            return _queriesManager.ExecuteNonQueryAsync(queryBuilder.ToString(), null, cancellationToken);
        }

        public Task<int> InsertAsync(T item, CancellationToken cancellationToken = default)
        {
            return InsertAsync(new[] { item }, cancellationToken);
        }

        public async Task<int> InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var queryBuilder = new StringBuilder($"INSERT INTO {_mapping.TableName.EscapeTableName()} (");

            var columnsFiltred = _mapping.Columns.Where(c => !c.IsAutoIncrement);
            var lastColumn = columnsFiltred.Last();
            foreach (var column in columnsFiltred)
            {
                queryBuilder.Append($"{column.ColumnName.EscapeColumnName()}");

                if (column == lastColumn)
                {
                    queryBuilder.Append(") ");
                }
                else
                {
                    queryBuilder.Append(",");
                }
            }

            queryBuilder.Append(" VALUES ");

            var parameters = new List<SqliteParameter>();
            var count = items.Count();
            for (int indexItem = 0; indexItem < count; indexItem++)
            {
                var item = items.ElementAt(indexItem);

                queryBuilder.Append("(");
                for (int i = 0; i < columnsFiltred.Count(); i++)
                {
                    var column = columnsFiltred.ElementAt(i);
                    var value = column.PropertyInfo.GetValue(item);
                    var paramName = $"@param{indexItem}_{i}";

                    var parameter = new SqliteParameter(paramName, value);

                    if (column.PropertyInfo.DeclaringType == typeof(byte[]))
                    {
                        parameter.DbType = System.Data.DbType.Binary;
                    }

                    parameters.Add(parameter);
                    queryBuilder.Append(paramName);

                    if (column == lastColumn)
                    {
                        queryBuilder.Append(") ");
                    }
                    else
                    {
                        queryBuilder.Append(", ");
                    }
                }

                if (indexItem < count - 1)
                {
                    queryBuilder.Append(", ");
                }

                queryBuilder.AppendLine();
            }

            queryBuilder.Append(";");

            return await _queriesManager.ExecuteNonQueryAsync(queryBuilder.ToString(), parameters, cancellationToken);
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

        public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _queriesManager.ExecuteScalarAsync($"SELECT Count(name) FROM sqlite_master WHERE type = 'table' AND name = '{_mapping.TableName}'", cancellationToken);
            return Convert.ToBoolean(result);
        }

        public Task DropAsync(CancellationToken cancellationToken = default)
        {
            var query = $"DROP TABLE IF EXISTS {_mapping.TableName.EscapeTableName()};";
            return _queriesManager.ExecuteNonQueryAsync(query, null, cancellationToken);
        }

        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var query = $"SELECT COUNT(*) FROM {_mapping.TableName.EscapeTableName()};";

            return (long)await _queriesManager.ExecuteScalarAsync(query, cancellationToken);
        }
    }
}