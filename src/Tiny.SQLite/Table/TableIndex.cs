using System.Collections.Generic;
using Tiny.SQLite.Attributes;

namespace Tiny.SQLite
{
    public class TableIndex
    {
        internal TableIndex()
        {
        }

        public string Name { get; internal set; }
        public TableColumn[] Columns { get; internal set; }
        public bool IsUnique { get; internal set; }
    }
}