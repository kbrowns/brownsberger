using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Context;
using Simple.NH.Exceptions;
using Simple.NH.Modeling;

namespace Simple.NH
{
    /// <summary>
    /// Represents an instance of the ISession
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly bool _isSessionOpener;

        /// <summary>
        /// The default constructor accepting a session factory as a parameter.
        /// </summary>
        /// <param name="manifest"></param>
        /// <param name="sessionFactory"></param>
        /// <param name="options"></param>
        internal protected UnitOfWork(IModelConfig manifest, ISessionFactory sessionFactory, SimpleNHOptions options)
        {
            _sessionFactory = sessionFactory;

            if (HasBind())
                return;

            var session = _sessionFactory.OpenSession();

            session.FlushMode = FlushMode.Commit;
            
            foreach (var feature in options.Features)
            {
                if (feature.OnOpenSession != null)
                    feature.OnOpenSession(manifest, session);
            }

            Bind(session);
            _isSessionOpener = true;
        }

        /// <summary>
        /// Creates a new transaction on the session.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public virtual ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            ISession session = _sessionFactory.GetCurrentSession();
            return session.BeginTransaction(isolationLevel);
        }

        /// <summary>
        /// Creates a new transaction on the session.
        /// </summary>
        /// <returns></returns>
        public virtual ITransaction BeginTransaction()
        {
            ISession session = _sessionFactory.GetCurrentSession();
            return session.BeginTransaction();
        }

        /// <summary>
        /// Saves an untyped object to the session.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>The saved object instance.</returns>
        public virtual object Save(object entity)
        {
            var session = _sessionFactory.GetCurrentSession();

            if (entity == null)
                return null;

            session.SaveOrUpdate(entity);

            return session;
        }

        /// <summary>
        /// Saved an entity instance of type IEntity to the session.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns>Returns the entity instance that was saved</returns>
        public virtual T Save<T>(T entity) where T : class, IEntity
        {
            var session = _sessionFactory.GetCurrentSession();

            if (entity == null)
                return null;

            if (entity.IsNew())
                session.Save(entity);
            else
                session.Update(entity);

            return entity;
        }


        /// <summary>
        /// Fetches an entity instance from the session by the identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns>The entity instance</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        public virtual T Get<T>(object id) where T : class, IEntity
        {
            return Get<T>(id, null);
        }

        /// <summary>
        /// Fetches an entity instance from the session by the identifier.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="defaultProvider">Optional parameter for overriding what happens when the entity instance is not found.</param>
        /// <returns>The entity instance</returns>
        public virtual T Get<T>(object id, Func<T> defaultProvider) where T : class, IEntity
        {
            var session = _sessionFactory.GetCurrentSession();
            var t = session.Get<T>(id);

            if (t == null)
            {
                if (defaultProvider != null)
                    return defaultProvider();

                throw new EntityNotFoundException(typeof(T), id);
            }

            return t;
        }

        /// <summary>
        /// Deleting an instance from the session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        public virtual void Delete<T>(T entity) where T : class
        {
            var session = _sessionFactory.GetCurrentSession();

            if (entity == null)
                return;

            session.Delete(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IQueryOver<T, T> QueryOver<T>() where T : class
        {
            var session = _sessionFactory.GetCurrentSession();
            return session.QueryOver<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aliasExpression"></param>
        /// <returns></returns>
        public virtual IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> aliasExpression) where T : class
        {
            var session = _sessionFactory.GetCurrentSession();
            return session.QueryOver(aliasExpression);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual T Load<T>(object id) where T : class
        {
            var session = _sessionFactory.GetCurrentSession();
            return session.Load<T>(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns></returns>
        public virtual IFilter EnableFilter(string filterName)
        {
            var session = _sessionFactory.GetCurrentSession();
            return session.EnableFilter(filterName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterName"></param>
        public virtual void DisableFilter(string filterName)
        {
            var session = _sessionFactory.GetCurrentSession();
            session.DisableFilter(filterName);
        }

        /// <summary>
        /// Provides an easy way to have a function run without a transaction.  This will automatically start and commit a transaction for you
        /// within a using block and run your function.  It's short had for running some code in a transaction.  Use this if you prefer a terse means
        /// of running code in a dedicated transaction.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public virtual TResult Transact<TResult>(Func<IUnitOfWork, TResult> func)
        {
            TResult result;

            using (var tx = BeginTransaction())
            {
                result = func(this);
                tx.Commit();
            }

            return result;
        }

        /// <summary>
        /// Provides an easy way to have an action run without a transaction.  This will automatically start and commit a transaction for you
        /// within a using block and run your action.  It's short had for running some code in a transaction.  Use this if you prefer a terse means
        /// of running code in a dedicated transaction.
        /// </summary>
        /// <param name="action"></param>
        public virtual void Transact(Action<IUnitOfWork> action)
        {
            Transact((s) =>
            {
                action(s);
                return false;
            });
        }

        public ISession GetRawSession()
        {
            return _sessionFactory.GetCurrentSession();
        }

        /// <summary>
        /// Teardown method that unbinds the inner session from it's configured context and disposes it.
        /// </summary>
        public virtual void Dispose()
        {
            if(_isSessionOpener)
                Unbind();
        }

        private static ITransaction CreateDefaultTransaction(ISession session)
        {
            return session.BeginTransaction();
        }

        private bool HasBind()
        {
            return CurrentSessionContext.HasBind(_sessionFactory);
        }

        private static void Bind(ISession session)
        {
            CurrentSessionContext.Bind(session);
        }

        private void Unbind()
        {
            ISession session = CurrentSessionContext.Unbind(_sessionFactory);

            if (session != null)
                session.Dispose();
        }
    }
}