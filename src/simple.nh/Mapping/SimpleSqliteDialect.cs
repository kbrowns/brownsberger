using System.Data;

namespace Simple.NH.Mapping
{
    /// <summary>
    /// Extends the stock SQLiteDialect to add additional data type support.
    /// </summary>
    public class SimpleSqliteDialect : NHibernate.Dialect.SQLiteDialect
    {
        /// <summary>
        /// Addiing support for additional data types.
        /// </summary>
        protected override void RegisterColumnTypes()
        {
            base.RegisterColumnTypes();
            RegisterColumnType(DbType.DateTimeOffset, "DATETIME");
        }
    }
}
