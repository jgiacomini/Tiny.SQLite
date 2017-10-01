using TinySQLite.Attributes;

namespace TinySQLite
{

    public class TableColumn
    {
        internal TableColumn()
        {
        }

        public string PropertyName { get; internal set; }
        public string ColumnType { get; set; }
        public string ColumnName { get; internal set; }
        public bool IsPrimaryKey { get; internal set; }
        public bool IsAutoIncrement { get; internal set; }
        public bool IsUnique { get; set; }
        public bool IsNullable { get; set; }
        public Collate Collate { get; set; }
    }
}