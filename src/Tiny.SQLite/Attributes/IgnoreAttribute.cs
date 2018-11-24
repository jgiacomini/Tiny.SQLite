using System;

namespace Tiny.SQLite.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {
    }
}