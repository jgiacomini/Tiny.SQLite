using System;

namespace TinySQLite.Attributes
{

    /// <summary>
    /// Every column of every table has an associated collating function. If no collating function is explicitly defined, then the collating function defaults to BINARY. The COLLATE clause of the column definition is used to define alternative collating functions for a column. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple =false)]
    public class CollationAttribute : Attribute
    {
        public Collate Collation { get; private set; }

        public CollationAttribute(Collate collation)
        {
            Collation = collation;
        }
    }

    /// <summary>
    /// When SQLite compares two strings, it uses a collating sequence or collating function (two words for the same thing) to determine which string is greater or if the two strings are equal. SQLite has three built-in collating functions: BINARY, NOCASE, and RTRIM.
    /// </summary>
    public enum Collate
    {
        /// <summary>
        /// Compares string data using memcmp(), regardless of text encoding.
        /// </summary>
        Binary = 0,

        /// <summary>
        /// The same as binary, except the 26 upper case characters of ASCII are folded to their lower case equivalents before the comparison is performed. Note that only ASCII characters are case folded. SQLite does not attempt to do full UTF case folding due to the size of the tables required.
        /// </summary>
        RTrim = 1,
        /// <summary>
        /// The same as binary, except that trailing space characters are ignored.
        /// </summary>
        NoCase = 2,
    }
}
