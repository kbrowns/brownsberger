using System;
using System.Collections.Generic;
using NHibernate;
using Simple.NH.Exceptions;

namespace Simple.NH
{
    /// <summary>
    /// Multiton wrapper around getting an InternalSessionFactory from which we obtain an InternalPersistentSession
    /// </summary>
    public static class SimpleNH
    {
        private static readonly object FactoryCacheSyncRoot = new object();
        private static readonly InternalSessionFactoryCache FactoryCache = new InternalSessionFactoryCache();

        public static void Initialize(IEnumerable<IModelConfig> models)
        {
            FactoryCache.Load(models.CheckArg("models"));
        }

        public static IUnitOfWork CreateUnitOfWork<TManifest>() where TManifest : IModelConfig
        {
            return FactoryCache.GetFactory<TManifest>().Create();
        }

        internal static NHibernate.Cfg.Configuration GetConfiguration<TModel>() where TModel : IModelConfig
        {
            return GetConfiguration(typeof(TModel));
        }

        internal static NHibernate.Cfg.Configuration GetConfiguration(Type modelType)
        {
            CheckModelType(modelType);

            lock (FactoryCacheSyncRoot)
            {
                InternalUnitOfWorkFactory factory;

                if (FactoryCache.TryGetFactory(modelType, out factory))
                    return factory.GetConfiguration();
            }

            throw new SimpleNHException("Model of type {0} was not found in the registry".FormatWith(modelType.FullName));
        }

        internal static ISessionFactory GetSessionFactory<TModel>() where TModel : IModelConfig
        {
            return GetSessionFactory(typeof(TModel));
        }

        internal static ISessionFactory GetSessionFactory(Type manifestType)
        {
            CheckModelType(manifestType);

            lock (FactoryCacheSyncRoot)
            {
                InternalUnitOfWorkFactory factory;

                if (FactoryCache.TryGetFactory(manifestType, out factory))
                    return factory.GetSessionFactory();
            }

            return null;
        }

        internal static void Reset(params IModelConfig[] models)
        {
            lock (FactoryCacheSyncRoot)
            {
                FactoryCache.Clear();
                Initialize(models);
            }
        }

        private static void CheckModelType(Type modelType)
        {
            modelType.CheckArg("modelType");

            if (!typeof(IModelConfig).IsAssignableFrom(modelType))
                throw new ArgumentException(string.Format("Type argument '{0}' must be an implementation of '{1}'", modelType.FullName, typeof(IModelConfig).Name));
        }
    }
}
