using System;
using NHibernate;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;

public static class NHibernateExtensionMethods
{
    public static Type GetUnproxiedType(this object value)
    {
        // Detect NHibernate proxies
        if (value.IsProxy())
            return NHibernateUtil.GetClass(value);

        return value.GetType();
    }

    public static string GetTableName(this IEntityPersister persister)
    {
        if (persister == null)
            return null;

        ILockable lockable = persister as ILockable;

        if (lockable == null)
            return null;

        return lockable.RootTableName;
    }
}