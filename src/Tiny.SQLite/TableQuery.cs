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

        public Task CreateAsync()
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
                    queryBuilder.Append($"COLLATE {column.Collate} ");
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

        public Task DropAsync()
        {
            var query = $"DROP TABLE IF EXISTS \"{_mapping.TableName.EscapeTableName()}\";";
            return _queriesManager.ExecuteNonQueryAsync(query);
        }
    }
}