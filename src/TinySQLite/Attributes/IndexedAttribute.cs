using System;

namespace TinySQLite.Attributes
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IndexedAttribute : Attribute
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public bool Unique { get; set; }

        public IndexedAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }
    }
}