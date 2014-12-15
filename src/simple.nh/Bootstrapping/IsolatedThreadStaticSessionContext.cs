using System;
using System.Collections;
using NHibernate.Context;
using NHibernate.Engine;

namespace Simple.NH
{
    /// <summary>
    /// Stores the session in a thread static dictionary segmented by session factory.  In future versions of NHibernate this 
    /// can probably be replaced with ThreadLocalSessionContext, but at the time of this writing, this class (in CSG's current 
    /// version as well as trunk) is not compatible w/ their surrounding framework as well as usage warnings noted in the source.
    /// </summary>
    public class IsolatedThreadStaticSessionContext : MapBasedSessionContext
    {
        [ThreadStatic]
        private static IDictionary _map;

        /// <summary>
        /// The default constructor required by the NH framework.
        /// </summary>
        /// <param name="factory"></param>
        public IsolatedThreadStaticSessionContext(ISessionFactoryImplementor factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Override of the base class provided by NHibernate that allows for returning the dictionary of session factory to 
        /// session based on our storage implementation.  In this case a thread static dictionary
        /// </summary>
        /// <returns></returns>
        protected override IDictionary GetMap()
        {
            return _map;
        }

        /// <summary>
        /// Override of the base class provided by NHibernate that allows for storing the dictionary of session factory to 
        /// session based on our storage implementation.  In this case a thread static dictionary
        /// </summary>
        /// <returns></returns>
        protected override void SetMap(IDictionary value)
        {
            _map = value;
        }
    }
}