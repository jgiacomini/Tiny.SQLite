using System.Linq;

namespace TinySQLite.Net.UnitTests
{
    public abstract class BaseColumnTest : BaseDatabaseTests
    {
        public BaseColumnTest(bool autoCreateDatabase) : base(autoCreateDatabase)
        {
        }

        public TableColumn GetColumnByPropertyName(TableMapping mapping, string propertyName)
        {
            return mapping.Columns.FirstOrDefault(f => f.PropertyName == propertyName);
        }
    }
}
