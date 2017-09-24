using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnIgnoreTests : BaseColumnTest
    {
        public ColumnIgnoreTests() : base(false)
        {
        }

        public class IgnoredColumnTable
        {
            [Column("CUSTOM")]
            [MaxLength(255)]
            public string Max { get; set; }

            [Attributes.Ignore]
            public string Ignore { get; set; }

            public string Ignore2 { get { return "a"; } }
        }

        [TestMethod]
        public void TestIgnoredColumn()
        {
            TableMapper mapper = new TableMapper(true);
            var mapping = mapper.Map<IgnoredColumnTable>();
            var column1 = GetColumnByPropertyName(mapping, "Ignore");
            var column2 = GetColumnByPropertyName(mapping, "Ignore2");

            Assert.IsNull(column1, "Column 'ignore' not ignored");
            Assert.IsNull(column2, "Column 'ignore2' not ignored");
        }

        public class MustBeIgnoredColumn
        {
            public static int StaticInt { get; set; }
            public static int PrivateSetter { get; private set; }
            public static int PrivateGetter { private get; set; }

            public static int NoGetter { set { }  }

            public static int NoSetter { get { return 42; } }

        }

        [TestMethod]
        public void TestIgnored_Static_NoGetterSetters_Columns()
        {
            TableMapper mapper = new TableMapper(true);
            var mapping = mapper.Map<MustBeIgnoredColumn>();
            var staticColumn = GetColumnByPropertyName(mapping, nameof(MustBeIgnoredColumn.StaticInt));

            var privateGetterColumn = GetColumnByPropertyName(mapping, nameof(MustBeIgnoredColumn.PrivateGetter));
            var privateSetterColumn = GetColumnByPropertyName(mapping, nameof(MustBeIgnoredColumn.PrivateSetter));

            var noGetterColumn = GetColumnByPropertyName(mapping, nameof(MustBeIgnoredColumn.NoGetter));
            var noSetterColumn = GetColumnByPropertyName(mapping, nameof(MustBeIgnoredColumn.NoSetter));

            Assert.IsNull(staticColumn, nameof(MustBeIgnoredColumn.StaticInt));
            Assert.IsNull(privateGetterColumn, nameof(MustBeIgnoredColumn.PrivateGetter));
            Assert.IsNull(privateSetterColumn, nameof(MustBeIgnoredColumn.PrivateSetter));

            Assert.IsNull(noGetterColumn, nameof(MustBeIgnoredColumn.NoGetter));
            Assert.IsNull(noSetterColumn, nameof(MustBeIgnoredColumn.NoSetter));
        }
    }
}
