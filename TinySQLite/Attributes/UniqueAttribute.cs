using System;

namespace TinySQLite.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UniqueAttribute : Attribute
    {
        public UniqueAttribute()
        {
        }
    }
}