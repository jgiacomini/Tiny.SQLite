using TinySQLite.Attributes;

namespace TinySQLite
{
    public class TableColumn
    {
        internal TableColumn()
        {
        }

        public string PropertyName { get; internal set; }
        public string ColumnType { get; internal set; }
        public string ColumnName { get; internal set; }
        public bool IsPrimaryKey { get; internal set; }
        public bool IsAutoIncrement { get; internal set; }
        public bool IsNullable { get; internal set; }
        public Collate Collate { get; internal set; }
    }
}