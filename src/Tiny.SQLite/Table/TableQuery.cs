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

        #region Create
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

            return _queriesManager.ExecuteNonQueryAsync(queryBuilder.ToString(), null, null, cancellationToken);
        }
        #endregion

        #region Insert
        public async Task<int> InsertAsync(T item, CancellationToken cancellationToken = default)
        {
            var result = await InternalInsertAsync(item, null, cancellationToken);

            var autoIncrementProperty = _mapping.Columns.Where(s => s.IsAutoIncrement).FirstOrDefault();

            if (autoIncrementProperty != null)
            {
                autoIncrementProperty.PropertyInfo.SetValue(item, ConvertToDesiredType(result.Identity, autoIncrementProperty.PropertyInfo.PropertyType));
            }

            return result.RowAffected;
        }

        private object ConvertToDesiredType(long identity, Type propertyType)
        {
            if (propertyType == typeof(int))
            {
                return Convert.ToInt32(identity);
            }

            if (propertyType == typeof(short))
            {
                return Convert.ToInt16(identity);
            }

            if (propertyType == typeof(uint))
            {
                return Convert.ToUInt32(identity);
            }

            if (propertyType == typeof(ushort))
            {
                return Convert.ToUInt16(identity);
            }

            return identity;
        }

        public async Task<int> InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var results = new List<InsertResult>();
            var transaction = _queriesManager.GetTransaction();
            try
            {
                foreach (var item in items)
                {
                    results.Add(await InternalInsertAsync(item, transaction, cancellationToken));
                }

                transaction.Commit();

                if (_mapping.HasAutoIncrement)
                {
                    var autoIncrementProperty = _mapping.Columns.Where(s => s.IsAutoIncrement).FirstOrDefault();
                    for (int i = 0; i < results.Count; i++)
                    {
                        var item = items.ElementAt(i);
                        var result = results[i];
                        autoIncrementProperty.PropertyInfo.SetValue(item, ConvertToDesiredType(result.Identity, autoIncrementProperty.PropertyInfo.PropertyType));
                    }
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            if (results.Any())
            {
                return results.Sum(v => v.RowAffected);
            }

            return 0;
        }

        private async Task<InsertResult> InternalInsertAsync(T item, SqliteTransaction transaction, CancellationToken cancellationToken = default)
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

            queryBuilder.Append(" VALUES (");

            var parameters = new List<SqliteParameter>();

            for (int i = 0; i < columnsFiltred.Count(); i++)
            {
                var column = columnsFiltred.ElementAt(i);
                var value = column.PropertyInfo.GetValue(item);
                var paramName = $"@param_{i}";

                var parameter = new SqliteParameter(paramName, value);

                // TODO  explicit more types
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

            queryBuilder.Append(";");

            var nbRowAffected = await _queriesManager.ExecuteNonQueryAsync(queryBuilder.ToString(), parameters, transaction, cancellationToken);

            long id = 0;

            if (_mapping.HasAutoIncrement)
            {
                id = (long)await _queriesManager.ExecuteScalarAsync("SELECT last_insert_rowid()", cancellationToken);
            }

            return new InsertResult(nbRowAffected, id);
        }

        internal class InsertResult
        {
            public InsertResult(int rowAffected, long identity)
            {
                RowAffected = rowAffected;
                Identity = identity;
            }

            public int RowAffected { get; }
            public long Identity { get; }
        }
        #endregion

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

        #region Exists
        public async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _queriesManager.ExecuteScalarAsync($"SELECT Count(name) FROM sqlite_master WHERE type = 'table' AND name = '{_mapping.TableName}'", cancellationToken);
            return Convert.ToBoolean(result);
        }
        #endregion

        #region Drop
        public Task DropAsync(CancellationToken cancellationToken = default)
        {
            var query = $"DROP TABLE IF EXISTS {_mapping.TableName.EscapeTableName()};";
            return _queriesManager.ExecuteNonQueryAsync(query, null, null, cancellationToken);
        }
        #endregion

        #region Count
        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var query = $"SELECT COUNT(*) FROM {_mapping.TableName.EscapeTableName()};";

            return (long)await _queriesManager.ExecuteScalarAsync(query, cancellationToken);
        }
        #endregion
    }
}