using System;
namespace TinySQLite
{

    public class TableMapping
    {
        internal TableMapping()
        {
        }

        public string TableName { get; internal set; }
        public Type MappedType { get; internal set; }
        public TableColumn[] Columns { get; internal set; }
    }

}
