using System;
using System.Linq.Expressions;

namespace Simple.NH.Modeling
{
    /// <summary>
    /// Notifies NHibernate of any properties modified during the Pre-Save and Pre-Update events
    /// </summary>
    public delegate void SetPropertiesHandler(params Expression<Func<ITrackedEntity, object>>[] properties);
}