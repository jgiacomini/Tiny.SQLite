using System.IO;
using Mono.Data.Sqlite;

namespace TinySQLite
{

    public class Database
    {
        private readonly SqliteConnection _connection;
        private readonly string _filePath;
        public Database(SqliteConnection connection, string filePath)
        {
            _connection = connection;
            _filePath = filePath;
        }
        public void CreateFile()
        {
            if (File.Exists(_filePath))
            {
                SqliteConnection.CreateFile(_filePath);
            }
        }
    }
}