namespace Tiny.SQLite
{
    internal static class QueryBuilder
    {
        public static string EscapeTableName(this string tableName)
        {
            return $"[{tableName}]";
        }

        public static string EscapeColumnName(this string columnName)
        {
            return $"[{columnName}]";
        }
    }
}