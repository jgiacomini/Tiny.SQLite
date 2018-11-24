using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Tiny.SQLite.Attributes;
using Tiny.SQLite.Exceptions;

namespace Tiny.SQLite
{
    internal class TableMapper
    {
        private readonly bool _storeDateTimeAsTicks;
        private readonly bool _removeDiacriticsOnTableNameAndColumnName;

        public TableMapper(bool storeDateTimeAsTicks, bool removeDiacriticsOnTableNameAndColumnName)
        {
            _storeDateTimeAsTicks = storeDateTimeAsTicks;
            _removeDiacriticsOnTableNameAndColumnName = removeDiacriticsOnTableNameAndColumnName;
        }

        public TableMapping Map<T>()
            where T : class, new()
        {
            return Map(typeof(T));
        }

        public TableMapping Map(Type type)
        {
            TableMapping mapping = new TableMapping
            {
                MappedType = type,
                TableName = GetTableName(type)
            };
            mapping.MappedType = type;
            var columnsDictionaries = GetColums(type);
            mapping.Columns = columnsDictionaries.Select(a => a.Key).ToArray();
            mapping.Indexes = GetTableIndexes(columnsDictionaries);

            if (mapping.Columns.Count(c => c.IsAutoIncrement) > 1)
            {
                throw new TableHaveMoreThanOneAutoIncrementedColumnException(mapping.TableName);
            }

            return mapping;
        }

        private Dictionary<TableColumn, IEnumerable<Attribute>> GetColums(Type type)
        {
            var properties = type.GetRuntimeProperties();
            var columns = new Dictionary<TableColumn, IEnumerable<Attribute>>();

            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes();

                if (TryToGetColumnType(property, attributes, out string columnType))
                {
                    var column = GetTableColumn(property, attributes);
                    column.ColumnType = columnType;
                    columns.Add(column, attributes);
                }
            }

            return columns;
        }

        private string GetTableName(Type type)
        {
            var attribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();

            if (attribute != null)
            {
                return RemoveDiacriticsIfNeeded(attribute.Name);
            }

            return RemoveDiacriticsIfNeeded(type.Name);
        }

        private TableColumn GetTableColumn(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var colum = new TableColumn
            {
                PropertyInfo = info,
                IsPrimaryKey = GeColumnIsPrimaryKey(info, attributes),
                IsAutoIncrement = GeColumnIsAutoInc(info, attributes),

                ColumnName = GeColumnName(info, attributes),
                IsNullable = GetColumnIsNullable(info, attributes),
                Collate = GetColumnCollating(info, attributes)
            };
            return colum;
        }

        private TableIndex[] GetTableIndexes(Dictionary<TableColumn, IEnumerable<Attribute>> tableColumns)
        {
            var dictionary = new Dictionary<TableIndex, List<Tuple<TableColumn, int>>>();

            foreach (var column in tableColumns)
            {
                var attribute = column.Value.OfType<IndexedAttribute>().FirstOrDefault();

                if (attribute != null)
                {
                    var currentIndex = dictionary.FirstOrDefault(i => i.Key.Name == attribute.Name);
                    if (currentIndex.Key != null)
                    {
                        currentIndex.Value.Add(new Tuple<TableColumn, int>(column.Key, attribute.Order));

                        if (currentIndex.Key.IsUnique != attribute.IsUnique)
                        {
                            throw new AllColumnsInIndexMustHaveTheSameIsUniqueValueException();
                        }
                    }
                    else
                    {
                        var tableIndex = new TableIndex
                        {
                            IsUnique = attribute.IsUnique,
                            Name = attribute.Name
                        };
                        dictionary.Add(tableIndex, new List<Tuple<TableColumn, int>>() { new Tuple<TableColumn, int>(column.Key, attribute.Order) });
                    }
                }
            }

            var indexes = new List<TableIndex>();
            foreach (var index in dictionary)
            {
                index.Key.Columns = index.Value.OrderBy(v => v.Item2).Select(v => v.Item1).ToArray();
                indexes.Add(index.Key);
            }

            return indexes.ToArray();
        }

        #region Column Mapping
        private bool GetColumnIsNullable(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            if (attributes.FirstOrDefault(a => a is ColumnNotNullableAttribute) is ColumnNotNullableAttribute attribute)
            {
                return false;
            }

            if (info.PropertyType.IsConstructedGenericType)
            {
                var isNullable = info.PropertyType.IsConstructedGenericType &&
                    info.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);

                return true;
            }

            if (info.PropertyType == typeof(string))
            {
                return true;
            }

            if (info.PropertyType == typeof(byte[]))
            {
                return true;
            }

            return false;
        }

        private string GeColumnName(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            if (attributes.FirstOrDefault(a => a is ColumnAttribute) is ColumnAttribute attribute)
            {
                return RemoveDiacriticsIfNeeded(attribute.Name);
            }

            return RemoveDiacriticsIfNeeded(info.Name);
        }

        private bool GeColumnIsPrimaryKey(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            if (attributes.FirstOrDefault(a => a is PrimaryKeyAttribute) is PrimaryKeyAttribute attribute)
            {
                return true;
            }

            return false;
        }

        private bool GeColumnIsAutoInc(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.OfType<PrimaryKeyAttribute>().FirstOrDefault();

            if (attribute != null && attribute.AutoIncrement)
            {
                var type = info.PropertyType;

                // TODO : check if all theses types are auto incrementable in SQLite
                if (type == typeof(long) ||
                    type == typeof(ulong) ||
                    type == typeof(int) ||
                    type == typeof(uint) ||
                    type == typeof(byte) ||
                    type == typeof(sbyte) ||
                    type == typeof(short) ||
                    type == typeof(sbyte))
                {
                    return true;
                }

                throw new TypeNotSupportedAutoIncrementException(type);
            }

            return false;
        }

        private Collate GetColumnCollating(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.OfType<CollationAttribute>().FirstOrDefault();

            if (attribute != null)
            {
                return attribute.Collation;
            }

            return Collate.Binary;
        }

        private bool TryToGetColumnType(PropertyInfo info, IEnumerable<Attribute> attributes, out string columnType)
        {
            columnType = null;

            if (!info.CanRead || !info.CanWrite)
            {
                return false;
            }

            if (info.GetMethod.IsStatic)
            {
                return false;
            }

            if (info.SetMethod.IsStatic)
            {
                return false;
            }

            if (attributes.Any(a => a is IgnoreAttribute))
            {
                return false;
            }

            if (info.PropertyType == typeof(int) ||
                info.PropertyType == typeof(uint) ||
                info.PropertyType == typeof(int?) ||
                info.PropertyType == typeof(uint?))
            {
                columnType = "INTEGER";
                return true;
            }

            if (info.PropertyType == typeof(string))
            {
                var maxLengthAttr = info.GetCustomAttribute<MaxLengthAttribute>();

                if (maxLengthAttr != null)
                {
                    columnType = $"VARCHAR({maxLengthAttr.Value})";
                    return true;
                }

                columnType = "VARCHAR";
                return true;
            }

            if (info.PropertyType == typeof(bool) ||
                 info.PropertyType == typeof(bool?))
            {
                columnType = "BOOLEAN";
                return true;
            }

            if (info.PropertyType == typeof(DateTime) ||
                 info.PropertyType == typeof(DateTime?))
            {
                if (_storeDateTimeAsTicks)
                {
                    columnType = "BIGINT";
                }
                else
                {
                    columnType = "DATETIME";
                }

                return true;
            }

            if (IsNullableEnum(info.PropertyType) || info.PropertyType.GetTypeInfo().IsEnum)
            {
                columnType = "INTEGER";
                return true;
            }

            if (info.PropertyType == typeof(Guid) ||
                 info.PropertyType == typeof(Guid?))
            {
                columnType = "CHAR(36)";
                return true;
            }

            if (info.PropertyType == typeof(byte) ||
                info.PropertyType == typeof(sbyte) ||
                 info.PropertyType == typeof(byte?) ||
                 info.PropertyType == typeof(sbyte?))
            {
                columnType = "SMALLINT";
                return true;
            }

            if (info.PropertyType == typeof(byte[]))
            {
                columnType = "BLOB";
                return true;
            }

            if (info.PropertyType == typeof(char) ||
                info.PropertyType == typeof(char?))
            {
                columnType = "CHARACTER";
                return true;
            }

            if (info.PropertyType == typeof(short) ||
                info.PropertyType == typeof(ushort) ||
                info.PropertyType == typeof(short?) ||
                info.PropertyType == typeof(ushort?))
            {
                columnType = "MEDIUMINT";
                return true;
            }

            if (info.PropertyType == typeof(long) ||
                info.PropertyType == typeof(ulong) ||
                info.PropertyType == typeof(long?) ||
                info.PropertyType == typeof(ulong?))
            {
                columnType = "BIGINT";
                return true;
            }

            if (info.PropertyType == typeof(float) ||
                info.PropertyType == typeof(float?))
            {
                columnType = "FLOAT";
                return true;
            }

            if (info.PropertyType == typeof(double) ||
                info.PropertyType == typeof(double?))
            {
                columnType = "DOUBLE";
                return true;
            }

            if (info.PropertyType == typeof(decimal) ||
                info.PropertyType == typeof(decimal?))
            {
                columnType = "DECIMAL";
                return true;
            }

            throw new TypeNotSupportedException(info.PropertyType);
        }

        private bool IsNullableEnum(Type t)
        {
            Type u = Nullable.GetUnderlyingType(t);
            return (u != null) && u.IsEnum;
        }
        #endregion

        private string RemoveDiacriticsIfNeeded(string text)
        {
            if (_removeDiacriticsOnTableNameAndColumnName)
            {
                return RemoveDiacritics(text);
            }

            return text;
        }

        private string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}