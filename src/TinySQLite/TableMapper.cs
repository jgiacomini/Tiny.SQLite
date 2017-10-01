using System;
using System.Collections.Generic;
using System.Reflection;
using TinySQLite.Attributes;
using System.Linq;
using TinySQLite.Exceptions;

namespace TinySQLite
{
    public class TableMapper
    {
        private readonly bool _storeDateTimeAsTicks;

        public TableMapper(bool storeDateTimeAsTicks)
        {
            _storeDateTimeAsTicks = storeDateTimeAsTicks;
        }

        public TableMapping Map<T>() where T : class, new()
        {
            return Map(typeof(T));
        }

        public TableMapping Map(Type type)
        {
            TableMapping mapping = new TableMapping();
            mapping.MappedType = type;
            mapping.TableName = GetTableName(type);
            mapping.MappedType = type;
            mapping.Columns = GetColums(type);

            if (mapping.Columns.Count(c => c.IsAutoIncrement) > 1)
            {
                throw new TableHaveMoreThanOneAutoIncrementedColumnException(mapping.TableName);
            }

            return mapping;
        }

        private TableColumn[] GetColums(Type type)
        {
            var properties = type.GetRuntimeProperties();
            var columns = new List<TableColumn>();
           
            foreach (var property in properties)
            {
                string columnType = null;
                if (TryToGetColumnType(property, out columnType))
                {
                    var column = GetTableColumn(property);
                    column.ColumnType = columnType;
                    columns.Add(column);
                }
            }

            return columns.ToArray();
        }

        private string GetTableName(Type type)
        {
            var attribute = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();

            if (attribute != null)
            {
                return attribute.Name;
            }

            return type.Name;
        }

        private TableColumn GetTableColumn(PropertyInfo info)
        {
            var colum = new TableColumn();
            colum.PropertyName = info.Name;
            var attributes = info.GetCustomAttributes();
            colum.IsPrimaryKey = GeColumnIsPrimaryKey(info, attributes);
            colum.IsAutoIncrement = GeColumnIsAutoInc(info, attributes);

            colum.ColumnName = GeColumnName(info, attributes);
            colum.IsNullable = GetColumnIsNullable(info, attributes);
            colum.IsUnique = GeColumnIsUnique(info, attributes);
            colum.Collate = GetColumnCollating(info, attributes);
            return colum;
        }

#region Column Mapping
        private bool GetColumnIsNullable(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(a => a is ColumnNotNullableAttribute) as ColumnNotNullableAttribute;

            if (attribute != null)
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

            if (info.PropertyType == typeof(byte[]) || info.PropertyType == typeof(sbyte[]))
            {
                return true;
            }

            return false;
        }

        private string GeColumnName(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(a=> a is ColumnAttribute) as ColumnAttribute;

            if (attribute != null)
            {
                return attribute.Name;
            }

            return info.Name;
        }

        private bool GeColumnIsPrimaryKey(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(a => a is PrimaryKeyAttribute) as PrimaryKeyAttribute;

            if (attribute != null)
            {
                return true;
            }

            return false;
        }

        private bool GeColumnIsAutoInc(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(a => a is PrimaryKeyAttribute) as PrimaryKeyAttribute;

            if (attribute != null && attribute.AutoIncrement)
            {
                var type = info.PropertyType;

                if(type == typeof(long) ||
                    type == typeof(ulong) ||
                    type == typeof(int) ||
                    type == typeof(uint) ||
                    type == typeof(byte)||
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


        private bool GeColumnIsUnique(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(a=> a is UniqueAttribute) as UniqueAttribute;

            if (attribute != null)
            {
                return true;
            }

            return false;
        }

        private Collate GetColumnCollating(PropertyInfo info, IEnumerable<Attribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(a => a is CollationAttribute) as CollationAttribute;

            if (attribute != null)
            {
                return attribute.Collation;
            }
            return Collate.Binary;
        }

        private bool TryToGetColumnType(PropertyInfo info, out string columnType)
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

            if (info.GetCustomAttribute<IgnoreAttribute>() != null)
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

                if(maxLengthAttr != null)
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
            if (info.PropertyType == typeof(byte[]) ||
                info.PropertyType == typeof(sbyte[]))
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

    }
}