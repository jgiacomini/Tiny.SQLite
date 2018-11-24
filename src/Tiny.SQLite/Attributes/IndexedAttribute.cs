using System;

namespace TinySQLite.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IndexedAttribute : Attribute
    {
        public string Name { get; set; }
        public int Order { get; set; }

        /// <summary>
        /// A UNIQUE constraint is similar to a PRIMARY KEY constraint, except that a single table may have any number of UNIQUE constraints.
        /// For each UNIQUE constraint on the table, each row must contain a unique combination of values in the columns identified by the UNIQUE constraint.
        /// For the purposes of UNIQUE constraints, NULL values are considered distinct from all other values, including other NULLs.
        /// </summary>
        public bool IsUnique { get; set; }

        public IndexedAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }
    }
}