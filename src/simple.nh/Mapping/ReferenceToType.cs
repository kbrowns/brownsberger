using System;
using System.Data;
using System.Diagnostics;
using NHibernate;
using NHibernate.SqlTypes;
using Simple.NH.Modeling;

namespace Simple.NH.Mapping
{
    [Serializable]
    public class ReferenceToType<T> : AbstractReferenceToType
        where T : IHasId, IHasName
    {
        public override bool Equals(object x, object y)
        {
            return object.Equals(x, y);
        }

        public override int GetHashCode(object x)
        {
            if (x == null)
                return 0;

            var referenceTo = (ReferenceTo<T>)x;

            return referenceTo.GetHashCode();
        }

        public override object NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            int idOrdinal = rs.GetOrdinal(names[0]);
            int nameOrdinal = rs.GetOrdinal(names[1]);

            if (rs.IsDBNull(idOrdinal))
                return null;

            if (rs.IsDBNull(nameOrdinal))
                return new ReferenceTo<T>(rs.GetInt64(idOrdinal));

            return new ReferenceTo<T>(rs.GetInt64(idOrdinal), rs.GetString(nameOrdinal));
        }

        public override void NullSafeSet(IDbCommand cmd, object value, int index)
        {
            Debug.Assert(cmd != null);

            if (value != null)
            {
                var referenceTo = (ReferenceTo<T>)value;
                NHibernateUtil.Int64.NullSafeSet(cmd, referenceTo.Id, index);
                NHibernateUtil.AnsiString.NullSafeSet(cmd, referenceTo.Name, index + 1);
            }
        }

        public override object DeepCopy(object value)
        {
            return value;
        }

        public override object Replace(object original, object target, object owner)
        {
            return original;
        }

        public override object Assemble(object cached, object owner)
        {
            return cached;
        }

        public override object Disassemble(object value)
        {
            return value;
        }

        public override SqlType[] SqlTypes
        {
            get { return new[] { new SqlType(DbType.Int64), new SqlType(DbType.AnsiString) }; }
        }

        public override Type ReturnedType
        {
            get { return typeof(ReferenceTo<T>); }
        }

        public override bool IsMutable
        {
            get { return false; }
        }
    }
}