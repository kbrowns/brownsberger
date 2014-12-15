using System;
using System.Data;
using System.Linq.Expressions;
using NHibernate;
using Simple.NH.Modeling;

namespace Simple.NH
{
    /// <summary>
    /// Interface modeling a persistent session established with the database over NHibernate
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Initiates a new transaction within the context of this session.
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        ITransaction BeginTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Attaches a transient entity with the session and in the process converts the object into a persistent object.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        object Save(object entity);

        /// <summary>
        /// Attaches a transient entity with the session and in the process converts the object into a persistent object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        T Save<T>(T entity) where T : class, IEntity;

        /// <summary>
        /// Pulls a single entity instance from the session's identity map.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="defaultProvider"></param>
        /// <returns></returns>
        T Get<T>(object id, Func<T> defaultProvider = null) where T : class, IEntity;

        /// <summary>
        /// Deletes a persistent object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        void Delete<T>(T entity) where T : class;

        /// <summary>
        /// Constructs a QueryOver objects for the provided generic type to serve as a types query builder API.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryOver<T, T> QueryOver<T>() where T : class;

        /// <summary>
        /// Constructs a QueryOver objects for the provided generic type to serve as a types query builder API.  This overload allows for an 
        /// alias expression to be provided as a parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> aliasExpression) where T : class;

        /// <summary>
        /// Function providing a proxy instance of an entity to be used either for attaching associations to persistent objects, or for lazily fetching entity object graphs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Load<T>(object id) where T : class;

        /// <summary>
        /// Enables the filter of the specified name on the inner session.
        /// </summary>
        /// <param name="filterName"></param>
        /// <returns></returns>
        IFilter EnableFilter(string filterName);

        /// <summary>
        /// Disables the filter of the specified name on the inner session.
        /// </summary>
        /// <param name="filterName"></param>
        void DisableFilter(string filterName);

        /// <summary>
        /// Provides an easy way to have a function run without a transaction.  This will automatically start and commit a transaction for you
        /// within a using block and run your function.  It's short had for running some code in a transaction.  Use this if you prefer a terse means
        /// of running code in a dedicated transaction.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        TResult Transact<TResult>(Func<IUnitOfWork, TResult> func);

        /// <summary>
        /// Provides an easy way to have an action run without a transaction.  This will automatically start and commit a transaction for you
        /// within a using block and run your action.  It's short had for running some code in a transaction.  Use this if you prefer a terse means
        /// of running code in a dedicated transaction.
        /// </summary>
        /// <param name="action"></param>
        void Transact(Action<IUnitOfWork> action);
    }
}
