using System.Linq;

namespace Tiny.SQLite.UnitTests
{
    public abstract class BaseColumnTest : BaseDatabaseTests
    {
        internal TableColumn GetColumnByPropertyName(TableMapping mapping, string propertyName)
        {
            return mapping.Columns.FirstOrDefault(f => f.PropertyInfo.Name == propertyName);
        }
    }
}
