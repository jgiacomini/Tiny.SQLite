using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnNullableTest : BaseColumnTest
    {
        public ColumnNullableTest() : base(false)
        {
        }

        class NullableColumnsTable
        {
            public string NullableString { get; set; }

            [ColumnNotNullable]
            public string NotNullableString { get; set; }
            public byte[] MyBytes { get; set; }

            [ColumnNotNullable]
            public byte[] MyBytesNotNullable { get; set; }
        }

        class NullableColumnsIntTable
        {
            public int Int { get; set; }
            public int? IntNullable { get; set; }
  
        }


        [TestMethod]
        public void CollumnNullable_TestString()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<NullableColumnsTable>();
            var column = GetColumnByPropertyName(mapping,
                nameof(NullableColumnsTable.NullableString));

            Assert.IsTrue(column.IsNullable,
                "string without attribute must be nullable");

            var columnNotNull = GetColumnByPropertyName(mapping, nameof(NullableColumnsTable.NotNullableString));
            Assert.IsFalse(columnNotNull.IsNullable,
                "string with attribute not null must be not nullable");
        }

        [TestMethod]
        public void CollumnNullable_TestBytes()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<NullableColumnsTable>();
            var column = GetColumnByPropertyName(mapping, nameof(NullableColumnsTable.MyBytes));

            Assert.IsTrue(column.IsNullable);


            var columnNotNull = GetColumnByPropertyName(mapping, nameof(NullableColumnsTable.MyBytesNotNullable));
            Assert.IsFalse(columnNotNull.IsNullable);
        }


        [TestMethod]
        public void CollumnNullable_TestInt()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<NullableColumnsIntTable>();

            var columnNotNull = GetColumnByPropertyName(mapping, nameof(NullableColumnsIntTable.Int));
            Assert.IsFalse(columnNotNull.IsNullable, "int is not nullable type");

            var column = GetColumnByPropertyName(mapping, nameof(NullableColumnsIntTable.IntNullable));
            Assert.IsTrue(column.IsNullable);
        }
    }
}
