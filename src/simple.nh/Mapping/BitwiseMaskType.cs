using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Simple.NH.Modeling;

namespace Simple.NH.Mapping
{
    [Serializable]
    public class BitwiseMask<T>
        where T : IHasId
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BitwiseMask()
        {
            _value = 0;
        }

        /// <summary>
        /// Constructor with initializing value.
        /// </summary>
        /// <param name="value">Initializing value.</param>
        public BitwiseMask(long value)
        {
            _value = value;
        }

        protected long _value;

        /// <summary>
        /// Convert value to long.
        /// </summary>
        /// <returns>long value.</returns>
        public virtual long ToLong()
        {
            return _value;
        }

        /// <summary>
        /// Converts value to a string.
        /// </summary>
        public override string ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts value to a string.
        /// </summary>
        public virtual string ToString(string format)
        {
            return _value.ToString(format);
        }

        /// <summary>
        /// Type-specific comparison method for IsoDateTimeOffset instances
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(BitwiseMask<T> other)
        {
            return _value.CompareTo(other._value);
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual bool Equals(BitwiseMask<T> other)
        {
            return _value.Equals(other.ToLong());
        }

        /// <summary>
        /// Check if BitwiseMask contains the passed-in value.
        /// </summary>
        /// <param name="value">Value/flag for which to check.</param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            long id = ((IHasId)value).Id;
            return Contains(id);
        }

        /// <summary>
        /// Check if BitwiseMask contains the passed-in value.
        /// </summary>
        /// <param name="value">Value/flag for which to check.</param>
        /// <returns></returns>
        public bool Contains(long value)
        {
            var currentValue = (BitFlags)_value;
            var flagsValue = (BitFlags)value;
            return (currentValue & flagsValue) == flagsValue;
        }

        /// <summary>
        /// Check if BitwiseMask contains the passed-in Bit Flag.
        /// </summary>
        /// <param name="value">Value/flag for which to check.</param>
        /// <returns></returns>
        private bool Contains(BitFlags value)
        {
            var currentValue = (BitFlags)_value;
            var flagsValue = value;
            return (currentValue & flagsValue) == flagsValue;
        }

        /// <summary>
        /// Add a value to the current BitwiseMask.
        /// </summary>
        /// <param name="value">Value/flag to add.</param>
        /// <returns></returns>
        public BitwiseMask<T> Add(T value)
        {
            long id = ((IHasId)value).Id;

            return Add(id);
        }

        /// <summary>
        /// Add a value to the current BitwiseMask.
        /// </summary>
        /// <param name="value">Value/flag to add.</param>
        /// <returns></returns>
        public BitwiseMask<T> Add(long value)
        {
            var currentValue = (BitFlags)_value;
            var flagsValue = (BitFlags)value;
            _value = (long)(currentValue | flagsValue);

            return this;
        }

        /// <summary>
        /// Remove a value from the current BitwiseMask.
        /// </summary>
        /// <param name="value">Value/flag to remove.</param>
        /// <returns></returns>
        public BitwiseMask<T> Remove(T value)
        {
            long id = ((IHasId)value).Id;

            return Remove(id);
        }

        /// <summary>
        /// Remove a value from the current BitwiseMask.
        /// </summary>
        /// <param name="value">Value/flag to remove.</param>
        /// <returns></returns>
        public BitwiseMask<T> Remove(long value)
        {
            var currentValue = (BitFlags)_value;
            var flagsValue = (BitFlags)value;
            _value = (long)(currentValue ^ flagsValue);

            return this;
        }

        /// <summary>
        /// Convert long value to BitwiseMask.
        /// </summary>
        /// <param name="value">long value.</param>
        /// <returns>BitwiseMask object.</returns>
        public static implicit operator BitwiseMask<T>(long value)
        {
            return new BitwiseMask<T>(value);
        }

        public override bool Equals(object obj)
        {
            var value = obj as BitwiseMask<T>;
            if (value == null) return false;
            return this == value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(BitwiseMask<T> x, BitwiseMask<T> y)
        {
            if (object.ReferenceEquals(null, x) && object.ReferenceEquals(null, y)) return true;
            if (object.ReferenceEquals(null, x) || object.ReferenceEquals(null, y)) return false;
            return x._value == y._value;
        }

        public static bool operator !=(BitwiseMask<T> x, BitwiseMask<T> y)
        {
            return !(x == y);
        }

        [Flags]
        protected enum BitFlags
        {
            Flag0 = 0,
            Flag1 = 1,
            Flag2 = 2,
            Flag3 = 4,
            Flag4 = 8,
            Flag5 = 16,
            Flag6 = 32,
            Flag7 = 64,
            Flag8 = 128,
            Flag9 = 256,
            Flag10 = 512,
            Flag11 = 1024,
            Flag12 = 2048,
            Flag13 = 4096,
            Flag14 = 8192,
            Flag15 = 16384,
            Flag16 = 32768,
            Flag17 = 65536,
            Flag18 = 131072,
            Flag19 = 262144,
            Flag20 = 524288,
            Flag21 = 1048576,
            Flag22 = 2097152,
            Flag23 = 4194304,
            Flag24 = 8388608,
            Flag25 = 16777216,
            Flag26 = 33554432,
            Flag27 = 67108864,
            Flag28 = 134217728,
            Flag29 = 268435456,
            Flag30 = 536870912,
            Flag31 = 1073741824
        }

        private IEnumerable<BitFlags> GetSetFlags()
        {
            return Enum.GetValues(typeof(BitFlags)).Cast<object>().Where(flag => Contains((BitFlags)flag) && (int)flag != 0).Cast<BitFlags>().ToList();
        }

        public static bool CheckForOverlap(List<BitwiseMask<T>> masks)
        {
            var baseMask = new BitwiseMask<T>(0);

            foreach (var mask in masks)
            {
                if (mask.ToLong() <= 0) continue;
                var baseSelected = baseMask.GetSetFlags();
                var maskSelected = mask.GetSetFlags();
                var overlap = baseSelected.Intersect(maskSelected);
                if (overlap.Any())
                {
                    return true;
                }

                baseMask = baseMask.ToLong() ^ mask.ToLong();
            }

            return false;
        }
    }

    [Serializable]
    public class BitwiseMaskType<T> : IUserType
        where T : IHasId
    {
        bool IUserType.Equals(object x, object y)
        {
            return Equals(x, y);
        }

        int IUserType.GetHashCode(object x)
        {
            if (x == null)
                return 0;

            return x.GetHashCode();
        }

        object IUserType.NullSafeGet(IDataReader rs, string[] names, object owner)
        {
            int ordinal = rs.GetOrdinal(names[0]);

            if (rs.IsDBNull(ordinal))
                return null;

            var myObj = rs.GetValue(ordinal);
            if (myObj.GetType() != typeof(int))
            {
                if (myObj.GetType() == typeof(Int64))
                {
                    return new BitwiseMask<T>((long)myObj);
                }

                throw new Exception(string.Format("Expected int but found: " + myObj.GetType().Name));
            }


            int value = rs.GetInt32(ordinal);
            return new BitwiseMask<T>(value);
        }

        void IUserType.NullSafeSet(IDbCommand cmd, object value, int index)
        {
            Debug.Assert(cmd != null);

            var parameter = ((IDataParameter)cmd.Parameters[index]);

            if (value is BitwiseMask<T>)
                parameter.Value = ((BitwiseMask<T>)value).ToLong();
            else
                parameter.Value = DBNull.Value;
        }

        object IUserType.DeepCopy(object value)
        {
            return value;
        }

        object IUserType.Replace(object original, object target, object owner)
        {
            return original;
        }

        object IUserType.Assemble(object cached, object owner)
        {
            return cached;
        }

        object IUserType.Disassemble(object value)
        {
            return value;
        }

        SqlType[] IUserType.SqlTypes
        {
            get { return new[] { new SqlType(DbType.Int64) }; }
        }

        Type IUserType.ReturnedType
        {
            get { return typeof(BitwiseMask<T>); }
        }

        bool IUserType.IsMutable
        {
            get { return false; }
        }
    }

}
