﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tiny.SQLite.Attributes;

namespace Tiny.SQLite.UnitTests
{
    [TestClass]
    public class ColumnCustomNameTest : BaseColumnTest
    {
        public class CustomTable
        {
            [Column("CUSTOM")]
            public string Max { get; set; }
        }

        [TestMethod]
        public void TestCustomColumnName()
        {
            TableMapper mapper = new TableMapper(true, true);
            var mapping = mapper.Map<CustomTable>();
            var column = GetColumnByPropertyName(mapping, "Max");

            Assert.IsTrue(column.ColumnName == "CUSTOM");
        }
    }
}
