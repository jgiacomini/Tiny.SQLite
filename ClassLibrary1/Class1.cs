using Mono.Data.Sqlite;

namespace ClassLibrary1
{
    public class Class1
    {
        public Class1()
        {
            const string connectionString = "URI=file:SqliteTest.db";

            var dbcon = new SqliteConnection(connectionString);
            dbcon.Open();
            var dbcmd = dbcon.CreateCommand();
            dbcmd.ExecuteNonQueryAsync();
        }
    }
}
