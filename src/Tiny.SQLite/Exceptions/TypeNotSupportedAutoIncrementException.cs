using System;

namespace Tiny.SQLite.Exceptions
{
    public class TypeNotSupportedAutoIncrementException : Exception
    {
        public TypeNotSupportedAutoIncrementException(Type type)
            : base($"The type {type.Name} cannot support the autoincrement")
        {
        }
    }
}
