using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class TableWithAllTypesHandledTests : BaseDatabaseTests
    {
        public TableWithAllTypesHandledTests() : base(true)
        {

        }
        public class SimpleTableWithAllTypes
        {
            public Guid MyGuid { get; set; }

            public MyEnum MyEnum { get; set; }
            public string MyStr { get; set; }
            public char MyChar { get; set; }
            public DateTime Date { get; set; }
            public bool MyBoolean { get; set; }

            public int MyInt { get; set; }
            public uint MyuInt { get; set; }

            public short MyShort { get; set; }
            public ushort MyUShort { get; set; }

            public long MyLong { get; set; }
            public ulong MyULong { get; set; }

            public byte MyByte { get; set; }
            public byte[] MyBytes { get; set; }

            public sbyte MysByte { get; set; }
            public sbyte[] MysBytes { get; set; }
            public double MyDouble { get; set; }
            public float MyFloat { get; set; }
            public decimal MyDecimal { get; set; }

        }

        public class SimpleTableWithAllTypesNullable
        {
            public Guid? MyGuid { get; set; }
            public MyEnum? MyEnum { get; set; }
            //TODO : nullable
            public string MyStr { get; set; }
            public char? MyChar { get; set; }
            public DateTime? Date { get; set; }
            public bool? MyBoolean { get; set; }
            public int? MyInt { get; set; }
            public uint? MyuInt { get; set; }
            public short? MyShort { get; set; }
            public ushort? MyUShort { get; set; }
            public long? MyLong { get; set; }
            public ulong? MyULong { get; set; }
            public byte? MyByte { get; set; }
            //TODO : nullable
            public byte[] MyBytes { get; set; }
            public sbyte? MysByte { get; set; }
            //TODO : nullable
            public sbyte[] MysBytes { get; set; }
            public double? MyDouble { get; set; }
            public float? MyFloat { get; set; }
            public decimal? MyDecimal { get; set; }
        }

        public enum MyEnum
        {
           Val1,
           Val2,
        }


        [TestMethod]
        public void TestSimpleTypes()
        {
            TableMapper mapper = new TableMapper(false, true);
            var mapping = mapper.Map<SimpleTableWithAllTypes>();

            CheckColumns(mapping);
            Assert.IsTrue(mapping.TableName == "SimpleTableWithAllTypes", "TableName KO");
            Assert.IsTrue(mapping.MappedType == typeof(SimpleTableWithAllTypes), "MappedType KO");
          
            var dateColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypes.Date));
            Assert.IsTrue(dateColumn.ColumnType == "DATETIME");

            var booleanColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypes.MyBoolean));
            Assert.IsTrue(booleanColumn.ColumnType == "BOOLEAN");

            var byteColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypes.MyByte));
            Assert.IsTrue(byteColumn.ColumnType == "SMALLINT", $"failed get type of {nameof(byteColumn)}");

            var intColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypes.MyInt));
            Assert.IsTrue(intColumn.ColumnType == "INTEGER");

            var guidColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypes.MyGuid));
            Assert.IsTrue(guidColumn.ColumnType == "CHAR(36)");

            var myStrColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypes.MyStr));
            Assert.IsTrue(myStrColumn.ColumnType == "VARCHAR");

        }

        [TestMethod]
        public async Task CreateTableWithSimpleTypes()
        {
            var context = new DbContext(_pathOfDb, autoCreateDatabaseFile: true);

            try
            {
                var table = context.Table<SimpleTableWithAllTypes>();
                await table.CreateAsync();

                Assert.IsTrue(await table.ExistsAsync());

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }



        [TestMethod]
        public void TestSimpleTypesNullable()
        {
            TableMapper mapper = new TableMapper(false, true);
            var mapping = mapper.Map<SimpleTableWithAllTypesNullable>();

            CheckColumnsNullabe(mapping);
            Assert.IsTrue(mapping.TableName == "SimpleTableWithAllTypesNullable", "TableName KO");
            Assert.IsTrue(mapping.MappedType == typeof(SimpleTableWithAllTypesNullable), "MappedType KO");

            var dateColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypesNullable.Date));
            Assert.IsTrue(dateColumn.ColumnType == "DATETIME");

            var booleanColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypesNullable.MyBoolean));
            Assert.IsTrue(booleanColumn.ColumnType == "BOOLEAN");

            var byteColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypesNullable.MyByte));
            Assert.IsTrue(byteColumn.ColumnType == "SMALLINT", $"failed get type of {nameof(byteColumn)}");

            var intColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypesNullable.MyInt));
            Assert.IsTrue(intColumn.ColumnType == "INTEGER");

            var guidColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypesNullable.MyGuid));
            Assert.IsTrue(guidColumn.ColumnType == "CHAR(36)");

            var myStrColumn = GetColumnByPropertyName(mapping, nameof(SimpleTableWithAllTypesNullable.MyStr));
            Assert.IsTrue(myStrColumn.ColumnType == "VARCHAR");

        }

        private void CheckColumns(TableMapping mapping)
        {
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.Date)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyBoolean)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyByte)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyBytes)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyChar)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyDouble)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyDecimal)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyFloat)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyInt)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyLong)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MysByte)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MysBytes)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyShort)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyStr)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyuInt)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyULong)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyUShort)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyEnum)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypes.MyGuid)));
        }

        private void CheckColumnsNullabe(TableMapping mapping)
        {
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.Date)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyBoolean)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyByte)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyBytes)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyChar)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyDouble)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyDecimal)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyFloat)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyInt)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyLong)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MysByte)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MysBytes)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyShort)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyStr)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyuInt)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyULong)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyUShort)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyEnum)));
            Assert.IsTrue(mapping.Columns.Any(t => t.ColumnName == nameof(SimpleTableWithAllTypesNullable.MyGuid)));
        }

        private TableColumn GetColumnByPropertyName(TableMapping mapping, string propertyName)
        {
            return mapping.Columns.First(f => f.PropertyName == propertyName);
        }
    }
}
