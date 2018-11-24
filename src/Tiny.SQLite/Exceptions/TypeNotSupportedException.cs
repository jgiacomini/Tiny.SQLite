using System;
using System.Collections.Generic;
using System.Text;

namespace TinySQLite.Exceptions
{
    public class TypeNotSupportedException : Exception
    {
        public TypeNotSupportedException(Type type)
            : base($"The type {type.Name} cannot be mapped to table")
        {
        }
    }
}
