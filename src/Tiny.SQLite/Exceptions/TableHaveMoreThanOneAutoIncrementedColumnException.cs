using System;
using System.Collections.Generic;
using System.Text;

namespace Tiny.SQLite.Exceptions
{
    public class TableHaveMoreThanOneAutoIncrementedColumnException : Exception
    {
        public TableHaveMoreThanOneAutoIncrementedColumnException(string tableName)
            : base($"table {tableName} has more than one auto incremented column")
        {
        }
    }
}
