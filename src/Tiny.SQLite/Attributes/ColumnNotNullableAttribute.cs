using System;

namespace Tiny.SQLite.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnNotNullableAttribute : Attribute
    {
        public ColumnNotNullableAttribute()
        {
        }
    }
}