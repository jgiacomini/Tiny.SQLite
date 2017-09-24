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
        public bool IsPK { get; internal set; }
        public bool IsAutoInc { get; internal set; }
        public bool IsUnique { get; set; }
        public bool IsNullable { get; set; }
        public Collate Collate { get; set; }
    }
}