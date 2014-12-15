using System;
using System.Collections.Generic;
using Simple.NH.Exceptions;

namespace Simple.NH
{
    internal class InternalSessionFactoryCache : IDisposable
    {
        private readonly Dictionary<string, InternalUnitOfWorkFactory> _cache = new Dictionary<string, InternalUnitOfWorkFactory>();

        internal void Load(IEnumerable<IModelConfig> models)
        {
            foreach (var model in models.CheckArg("models"))
            {
                var key = model.GetType().FullName;

                _cache[key] = new InternalUnitOfWorkFactory(model);
            }
        }

        internal InternalUnitOfWorkFactory GetFactory<TModel>() where TModel : IModelConfig
        {
            InternalUnitOfWorkFactory factory;

            var key = typeof(TModel).FullName;

            if (_cache.TryGetValue(key, out factory))
                return factory;

            throw new ApplicationException(string.Format("{0} was not found in the list of registered persistence manifests.  Ensure this modelConfigType is registered with the ContainerHost as an implementation of {1}.", key, typeof(IModelConfig).FullName));
        }

        internal bool TryGetFactory(Type modelConfigType, out InternalUnitOfWorkFactory manifest)
        {
            modelConfigType.CheckArg("modelConfigType");

            var key = modelConfigType.FullName;

            _cache.TryGetValue(key, out manifest);

            return manifest != null;
        }

        internal void AddManifest(Type manifestType, IModelConfig model)
        {
            var key = manifestType.FullName;

            if (_cache.ContainsKey(key))
                throw new SimpleNHException("Model {0} is already registered".FormatWith(key));

            _cache.Add(key, new InternalUnitOfWorkFactory(model));
        }

        internal void Clear()
        {
            foreach (InternalUnitOfWorkFactory sessionFactory in _cache.Values)
            {
                sessionFactory.Dispose();
            }
            _cache.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}