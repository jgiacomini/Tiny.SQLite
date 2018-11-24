using System;

namespace Tiny.SQLite.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// The <see cref="AutoIncrement"/> can be activated for this types : <see cref="int"/>, or <see cref="uint"/>, or <see cref="byte"/> , or <see cref="sbyte"/>, , or <see cref="long"/> , or <see cref="ulong"/> column.
        /// The AUTOINCREMENT keyword keyword imposes extra CPU, memory, disk space, and disk I/O overhead and should be avoided if not strictly needed.It is usually not needed.
        /// In SQLite, a column with type INTEGER PRIMARY KEY is an alias for the ROWID(except in WITHOUT ROWID tables) which is always a 64-bit signed integer.
        /// On an INSERT, if the ROWID or INTEGER PRIMARY KEY column is not explicitly given a value, then it will be filled automatically with an unused integer, usually one more than the largest ROWID currently in use.This is true regardless of whether or not the AUTOINCREMENT keyword is used.
        /// If the AUTOINCREMENT keyword appears after INTEGER PRIMARY KEY, that changes the automatic ROWID assignment algorithm to prevent the reuse of ROWIDs over the lifetime of the database.In other words, the purpose of AUTOINCREMENT is to prevent the reuse of ROWIDs from previously deleted rows.
        /// </summary>
        public bool AutoIncrement { get; set; }
    }
}