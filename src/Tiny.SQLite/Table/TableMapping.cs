using System;
using System.Linq;

namespace Tiny.SQLite
{
    internal class TableMapping
    {
        internal TableMapping(
            string tableName,
            Type mappedType,
            TableColumn[] columns,
            TableIndex[] indexes)
        {
            TableName = tableName;
            MappedType = mappedType;
            Columns = columns;
            Indexes = indexes;
            HasAutoIncrement = Columns.Any(c => c.IsAutoIncrement);
        }

        public string TableName { get; }
        public Type MappedType { get; }
        public TableColumn[] Columns { get; }
        public TableIndex[] Indexes { get; }
        public bool HasAutoIncrement { get; }
    }
}
