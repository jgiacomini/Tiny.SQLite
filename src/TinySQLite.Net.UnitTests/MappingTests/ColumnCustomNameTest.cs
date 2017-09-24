using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TinySQLite.Attributes;

namespace TinySQLite.Net.UnitTests
{
    [TestClass]
    public class ColumnCustomNameTest : BaseColumnTest
    {
        public ColumnCustomNameTest() : base(false)
        {
        }

        public class CustomTable
        {
            [Column("CUSTOM")]
            public string Max { get; set; }
        }

        [TestMethod]
        public void TestCustomColumnName()
        {
            TableMapper mapper = new TableMapper(true);
            var mapping = mapper.Map<CustomTable>();
            var column = GetColumnByPropertyName(mapping, "Max");

            Assert.IsTrue(column.ColumnName == "CUSTOM");
        }
    }
}
