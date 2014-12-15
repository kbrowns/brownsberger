using System;
using System.Data;
using NHibernate.Mapping.ByCode;
using NHibernate.SqlTypes;

namespace Simple.NH.Mapping
{
    public abstract class AbstractReferenceToType
    {
        protected internal static readonly string IdColumnNameFormat = "{0}_id";
        protected internal static readonly string NameColumnNameFormat = "{0}_name";

        public new abstract bool Equals(object x, object y);
        public abstract int GetHashCode(object x);
        public abstract object NullSafeGet(IDataReader rs, string[] names, object owner);
        public abstract void NullSafeSet(IDbCommand cmd, object value, int index);
        public abstract object DeepCopy(object value);
        public abstract object Replace(object original, object target, object owner);
        public abstract object Assemble(object cached, object owner);
        public abstract object Disassemble(object value);
        public abstract SqlType[] SqlTypes { get; }
        public abstract Type ReturnedType { get; }
        public abstract bool IsMutable { get; }

        public static void MapColumns(Type referenceType, IPropertyMapper propertyMapper)
        {
            var tableName = referenceType.GetClassMapping().TableName;

            propertyMapper.Columns(
                idColumn =>
                {
                    idColumn.Name(IdColumnNameFormat.FormatWith(tableName));
                    idColumn.NotNullable(true);
                },
                nameColumn =>
                {
                    nameColumn.Name(NameColumnNameFormat.FormatWith(tableName));
                    nameColumn.NotNullable(false);
                }
                );
        }
    }

}
