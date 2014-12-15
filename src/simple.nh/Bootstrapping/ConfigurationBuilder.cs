using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;

namespace Simple.NH
{
    /// <summary>
    /// Provides a single point for bootstrapping a session factory from the config.
    /// </summary>
    public static class ConfigurationBuilder
    {
        private static readonly string LogCategory = MethodBase.GetCurrentMethod().DeclaringType.FullName;

        /// <summary>
        /// Uses the ManifestReader and options to construct an NHibernate Configuration object that's suitable for 
        /// constructing a ISessionFactory object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static Configuration Build(IModelConfig model, SimpleNHOptions options)
        {
            model.ConfigureOptions(options);

            Configuration config = null;

            if (options.ConfigurationProvider != null)
                config = options.ConfigurationProvider(model);

            if (config == null)
                config = new Configuration();

            config.DataBaseIntegration(db => model.Initialize(db, config, model));

            config.Properties[Environment.CurrentSessionContextClass] = options.ContextInstancePolicyProvider(model).ToString();

            InitializeConnectionConfig(model, config, options);

            ExecutePreMappingFeatures(model, config, options);

            InitializeMappings(model, config, options);

            ExecutePostMappingFeatures(model, config, options);

            InitializeEventListeners(model, config, options);

            return config;
        }

        private static void InitializeConnectionConfig(IModelConfig manifest, NHibernate.Cfg.Configuration config, SimpleNHOptions options)
        {
            foreach (var feature in options.Features)
            {
                if (feature.OnInitConnectionConfig != null)
                    feature.OnInitConnectionConfig(manifest, config);
            }
        }

        private static void ExecutePreMappingFeatures(IModelConfig manifest, NHibernate.Cfg.Configuration config, SimpleNHOptions options)
        {
            foreach (var feature in options.Features)
            {
                if (feature.OnPreMapping != null)
                    feature.OnPreMapping(manifest, config);
            }
        }

        private static void ExecutePostMappingFeatures(IModelConfig manifest, NHibernate.Cfg.Configuration config, SimpleNHOptions options)
        {
            foreach (var feature in options.Features)
            {
                if (feature.OnPostMapping != null)
                    feature.OnPostMapping(manifest, config);
            }
        }

        private static void InitializeEventListeners(IModelConfig manifest, NHibernate.Cfg.Configuration config, SimpleNHOptions options)
        {
            SimpleEventListenerSet listeners = new SimpleEventListenerSet();

            foreach (var feature in options.Features)
            {
                if (feature.OnInitEventListeners != null)
                    feature.OnInitEventListeners(manifest, listeners);
            }

            config.EventListeners.AutoFlushEventListeners = ResolveListenerSet(listeners.AutoFlushEventListeners, config.EventListeners.AutoFlushEventListeners);
            config.EventListeners.DeleteEventListeners = ResolveListenerSet(listeners.DeleteEventListeners, config.EventListeners.DeleteEventListeners);
            config.EventListeners.DirtyCheckEventListeners = ResolveListenerSet(listeners.DirtyCheckEventListeners, config.EventListeners.DirtyCheckEventListeners);
            config.EventListeners.EvictEventListeners = ResolveListenerSet(listeners.EvictEventListeners, config.EventListeners.EvictEventListeners);
            config.EventListeners.FlushEntityEventListeners = ResolveListenerSet(listeners.FlushEntityEventListeners, config.EventListeners.FlushEntityEventListeners);
            config.EventListeners.FlushEventListeners = ResolveListenerSet(listeners.FlushEventListeners, config.EventListeners.FlushEventListeners);
            config.EventListeners.InitializeCollectionEventListeners = ResolveListenerSet(listeners.InitializeCollectionEventListeners, config.EventListeners.InitializeCollectionEventListeners);
            config.EventListeners.LoadEventListeners = ResolveListenerSet(listeners.LoadEventListeners, config.EventListeners.LoadEventListeners);
            config.EventListeners.LockEventListeners = ResolveListenerSet(listeners.LockEventListeners, config.EventListeners.LockEventListeners);
            config.EventListeners.MergeEventListeners = ResolveListenerSet(listeners.MergeEventListeners, config.EventListeners.MergeEventListeners);
            config.EventListeners.PersistEventListeners = ResolveListenerSet(listeners.PersistEventListeners, config.EventListeners.PersistEventListeners);
            config.EventListeners.PersistOnFlushEventListeners = ResolveListenerSet(listeners.PersistOnFlushEventListeners, config.EventListeners.PersistOnFlushEventListeners);
            config.EventListeners.PostCollectionRecreateEventListeners = ResolveListenerSet(listeners.PostCollectionRecreateEventListeners, config.EventListeners.PostCollectionRecreateEventListeners);
            config.EventListeners.PostCollectionRemoveEventListeners = ResolveListenerSet(listeners.PostCollectionRemoveEventListeners, config.EventListeners.PostCollectionRemoveEventListeners);
            config.EventListeners.PostCollectionUpdateEventListeners = ResolveListenerSet(listeners.PostCollectionUpdateEventListeners, config.EventListeners.PostCollectionUpdateEventListeners);
            config.EventListeners.PostCommitDeleteEventListeners = ResolveListenerSet(listeners.PostCommitDeleteEventListeners, config.EventListeners.PostCommitDeleteEventListeners);
            config.EventListeners.PostCommitInsertEventListeners = ResolveListenerSet(listeners.PostCommitInsertEventListeners, config.EventListeners.PostCommitInsertEventListeners);
            config.EventListeners.PostCommitUpdateEventListeners = ResolveListenerSet(listeners.PostCommitUpdateEventListeners, config.EventListeners.PostCommitUpdateEventListeners);
            config.EventListeners.PostDeleteEventListeners = ResolveListenerSet(listeners.PostDeleteEventListeners, config.EventListeners.PostDeleteEventListeners);
            config.EventListeners.PostInsertEventListeners = ResolveListenerSet(listeners.PostInsertEventListeners, config.EventListeners.PostInsertEventListeners);
            config.EventListeners.PostLoadEventListeners = ResolveListenerSet(listeners.PostLoadEventListeners, config.EventListeners.PostLoadEventListeners);
            config.EventListeners.PostUpdateEventListeners = ResolveListenerSet(listeners.PostUpdateEventListeners, config.EventListeners.PostUpdateEventListeners);
            config.EventListeners.PreCollectionRecreateEventListeners = ResolveListenerSet(listeners.PreCollectionRecreateEventListeners, config.EventListeners.PreCollectionRecreateEventListeners);
            config.EventListeners.PreCollectionRemoveEventListeners = ResolveListenerSet(listeners.PreCollectionRemoveEventListeners, config.EventListeners.PreCollectionRemoveEventListeners);
            config.EventListeners.PreCollectionUpdateEventListeners = ResolveListenerSet(listeners.PreCollectionUpdateEventListeners, config.EventListeners.PreCollectionUpdateEventListeners);
            config.EventListeners.PreDeleteEventListeners = ResolveListenerSet(listeners.PreDeleteEventListeners, config.EventListeners.PreDeleteEventListeners);
            config.EventListeners.PreInsertEventListeners = ResolveListenerSet(listeners.PreInsertEventListeners, config.EventListeners.PreInsertEventListeners);
            config.EventListeners.PreLoadEventListeners = ResolveListenerSet(listeners.PreLoadEventListeners, config.EventListeners.PreLoadEventListeners);
            config.EventListeners.PreUpdateEventListeners = ResolveListenerSet(listeners.PreUpdateEventListeners, config.EventListeners.PreUpdateEventListeners);
            config.EventListeners.RefreshEventListeners = ResolveListenerSet(listeners.RefreshEventListeners, config.EventListeners.RefreshEventListeners);
            config.EventListeners.ReplicateEventListeners = ResolveListenerSet(listeners.ReplicateEventListeners, config.EventListeners.ReplicateEventListeners);
            config.EventListeners.SaveEventListeners = ResolveListenerSet(listeners.SaveEventListeners, config.EventListeners.SaveEventListeners);
            config.EventListeners.SaveOrUpdateEventListeners = ResolveListenerSet(listeners.SaveOrUpdateEventListeners, config.EventListeners.SaveOrUpdateEventListeners);
            config.EventListeners.UpdateEventListeners = ResolveListenerSet(listeners.UpdateEventListeners, config.EventListeners.UpdateEventListeners);
        }

        private static T[] ResolveListenerSet<T>(SimpleEventListeners<T> source, T[] target)
        {
            if (source.ListenersCount > 0)
            {
                if (source.MergeWithFrameworkListeners)
                {
                    List<T> merged = new List<T>();

                    if(target != null)
                        merged.AddRange(target);

                    if (source.Listeners != null)
                        merged.AddRange(source.Listeners);

                    return merged.ToArray();
                }
                
                return source.Listeners == null ? new T[0] : source.Listeners.ToArray();
            }

            return target;
        }

        private static void InitializeMappings(IModelConfig manifest, Configuration config, SimpleNHOptions options)
        {
            var mapper = new ModelMapper();
            mapper.AddMappings(manifest.CandidateTypes);
            var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            foreach (var feature in options.Features)
            {
                if (feature.OnMappingCompilation != null)
                    feature.OnMappingCompilation(manifest, mapping);
            }

            config.AddMapping(mapping);
        }
    }
}