using System;
using System.Reflection;
using NHibernate;

namespace Simple.NH
{
    /// <summary>
    /// Wrapper around the inner NHibernate SessionFactory as well as the Configuration object.
    /// </summary>
    internal class InternalUnitOfWorkFactory : IDisposable
    {
        private readonly ISessionFactory _instance;
        private readonly NHibernate.Cfg.Configuration _configuration;
        private readonly SimpleNHOptions _options = new SimpleNHOptions();
        private readonly IModelConfig _model;

        internal InternalUnitOfWorkFactory(IModelConfig model)
        {
            model.CheckArg("model");

            _model = model;

            try
            {
                _configuration = ConfigurationBuilder.Build(model, _options);

                _instance = _configuration.BuildSessionFactory();

                ExecutePostFactoryBuild(_model, _instance, _configuration, _options);
            }
            catch
            {
                if(_instance != null)
                    _instance.Dispose();

                throw;
            }
        }

        private ISessionFactory SessionFactoryInstance { get { return _instance; } }

        internal ISessionFactory GetSessionFactory()
        {
            return this.SessionFactoryInstance;
        }

        private static void ExecutePostFactoryBuild(IModelConfig model, ISessionFactory factory, NHibernate.Cfg.Configuration configuration, SimpleNHOptions options)
        {
            foreach (var feature in options.Features)
            {
                if (feature.OnPostFactoryBuild != null)
                    feature.OnPostFactoryBuild(model, factory, configuration);
            }
        }

        /// <summary>
        /// Returns a IPersistentSession instance representing the current ISession as dictated by the context instance policy.
        /// </summary>
        /// <returns></returns>
        public IUnitOfWork Create()
        {
            if (_options.UnitOfWorkProvider == null)
                return new UnitOfWork(_model, SessionFactoryInstance, _options);

            var uow = _options.UnitOfWorkProvider(_model, SessionFactoryInstance);

            return uow ?? new UnitOfWork(_model, SessionFactoryInstance, _options);
        }

        /// <summary>
        /// Provides an instance the other underlying configuration instance.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public NHibernate.Cfg.Configuration GetConfiguration()
        {
            return _configuration;
        }

        public void Dispose()
        {
            _instance.Dispose();
        }
    }
}
