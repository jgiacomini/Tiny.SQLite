using System;
using System.Collections.Generic;
using System.Text;

namespace Tiny.SQLite.Exceptions
{
    public class AllColumnsInIndexMustHaveTheSameIsUniqueValueException : Exception
    {
        public AllColumnsInIndexMustHaveTheSameIsUniqueValueException()
            : base("All the columns in an index must have the same value for their IsUnique property")
        {
        }
    }
}
