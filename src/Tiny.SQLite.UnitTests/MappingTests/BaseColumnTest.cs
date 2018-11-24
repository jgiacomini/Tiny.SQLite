using System.Linq;

namespace Tiny.SQLite.UnitTests
{
    public abstract class BaseColumnTest : BaseDatabaseTests
    {
        public TableColumn GetColumnByPropertyName(TableMapping mapping, string propertyName)
        {
            return mapping.Columns.FirstOrDefault(f => f.PropertyName == propertyName);
        }
    }
}
