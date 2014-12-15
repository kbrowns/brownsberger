using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Event;

namespace Simple.NH
{
    /// <summary />
    public abstract class Feature
    {
        /// <summary>
        /// Provides an extensibility point for immediately before the domain types were scanned for code mappings.
        /// </summary>
        public Action<IModelConfig, NHibernate.Cfg.Configuration> OnPreMapping;
        /// <summary>
        /// Provides an extensibility point for immediately after the mapping has been compiled and before it's been added to the configuraiton object.
        /// </summary>
        public Action<IModelConfig, HbmMapping> OnMappingCompilation;
        /// <summary>
        /// Provides an extensibility point for immediately after the domain types were scanned for code mappings.
        /// </summary>
        public Action<IModelConfig, NHibernate.Cfg.Configuration> OnPostMapping;
        /// <summary>
        /// Provides a hook to override behavior performed at the time the NHibernate configuration object 
        /// has been constructed and is being merged with the CSG.Framework.Data configuration section.
        /// </summary>
        public Action<IModelConfig, NHibernate.Cfg.Configuration> OnInitConnectionConfig;
        /// <summary>
        /// Provides a hook to override behavior performed via event listeners.  Rather than mutate the EventListeners on the 
        /// NHibernate Configuration.EventListeners object, event listeners should be configured here and the framework
        /// with synchronize all feature event listeners at the end of startup.
        /// </summary>
        public Action<IModelConfig, SimpleEventListenerSet> OnInitEventListeners;
        /// <summary>
        /// Provides a hook to override behavior performed at the time a new NHibernate session is opened.
        /// </summary>
        public Action<IModelConfig, ISession> OnOpenSession;
        /// <summary>
        /// Provides a hook to override behavior performed at the time a new NHibernate session factory is constructed.  This is the last thing that's done at application start-up.
        /// </summary>
        public Action<IModelConfig, ISessionFactory, NHibernate.Cfg.Configuration> OnPostFactoryBuild;
    }

    /// <summary>
    /// This class accompanies the NHibernate EventListeners class.  That class defaults a number of it's properties with 
    /// the default listeners the framework wants.  Some features need to wire up custom listeners and/ore remove the default
    /// implementations
    /// </summary>
    public class SimpleEventListenerSet
    {
        // wish I didn't have to write this class, but it seems to be the lesser of evils
        //  non-deterministic, hard to debug runtime behavior?  or maintain this mapper class?
        // if NHibernate adds a new even some day, it's unlikely users of the framework will care and if they do, adding that to the list below
        // should be a single line of code (easily submitted via a contribution).

        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPreInsertEventListener> PreInsertEventListeners = new SimpleEventListeners<IPreInsertEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostInsertEventListener> PostInsertEventListeners = new SimpleEventListeners<IPostInsertEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPreUpdateEventListener> PreUpdateEventListeners = new SimpleEventListeners<IPreUpdateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostUpdateEventListener> PostUpdateEventListeners = new SimpleEventListeners<IPostUpdateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IAutoFlushEventListener> AutoFlushEventListeners = new SimpleEventListeners<IAutoFlushEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IDeleteEventListener> DeleteEventListeners = new SimpleEventListeners<IDeleteEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IDirtyCheckEventListener> DirtyCheckEventListeners = new SimpleEventListeners<IDirtyCheckEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IEvictEventListener> EvictEventListeners = new SimpleEventListeners<IEvictEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IFlushEntityEventListener> FlushEntityEventListeners = new SimpleEventListeners<IFlushEntityEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IFlushEventListener> FlushEventListeners = new SimpleEventListeners<IFlushEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IInitializeCollectionEventListener> InitializeCollectionEventListeners = new SimpleEventListeners<IInitializeCollectionEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<ILoadEventListener> LoadEventListeners = new SimpleEventListeners<ILoadEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<ILockEventListener> LockEventListeners = new SimpleEventListeners<ILockEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IMergeEventListener> MergeEventListeners = new SimpleEventListeners<IMergeEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPersistEventListener> PersistEventListeners = new SimpleEventListeners<IPersistEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPersistEventListener> PersistOnFlushEventListeners = new SimpleEventListeners<IPersistEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostCollectionRecreateEventListener> PostCollectionRecreateEventListeners = new SimpleEventListeners<IPostCollectionRecreateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostCollectionRemoveEventListener> PostCollectionRemoveEventListeners = new SimpleEventListeners<IPostCollectionRemoveEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostCollectionUpdateEventListener> PostCollectionUpdateEventListeners = new SimpleEventListeners<IPostCollectionUpdateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostDeleteEventListener> PostCommitDeleteEventListeners = new SimpleEventListeners<IPostDeleteEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostInsertEventListener> PostCommitInsertEventListeners = new SimpleEventListeners<IPostInsertEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostUpdateEventListener> PostCommitUpdateEventListeners = new SimpleEventListeners<IPostUpdateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostDeleteEventListener> PostDeleteEventListeners = new SimpleEventListeners<IPostDeleteEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPostLoadEventListener> PostLoadEventListeners = new SimpleEventListeners<IPostLoadEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPreCollectionRecreateEventListener> PreCollectionRecreateEventListeners = new SimpleEventListeners<IPreCollectionRecreateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPreCollectionRemoveEventListener> PreCollectionRemoveEventListeners = new SimpleEventListeners<IPreCollectionRemoveEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPreCollectionUpdateEventListener> PreCollectionUpdateEventListeners = new SimpleEventListeners<IPreCollectionUpdateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPreDeleteEventListener> PreDeleteEventListeners = new SimpleEventListeners<IPreDeleteEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IPreLoadEventListener> PreLoadEventListeners = new SimpleEventListeners<IPreLoadEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IRefreshEventListener> RefreshEventListeners = new SimpleEventListeners<IRefreshEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IReplicateEventListener> ReplicateEventListeners = new SimpleEventListeners<IReplicateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<ISaveOrUpdateEventListener> SaveEventListeners = new SimpleEventListeners<ISaveOrUpdateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<IMergeEventListener> SaveOrUpdateCopyEventListeners = new SimpleEventListeners<IMergeEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<ISaveOrUpdateEventListener> SaveOrUpdateEventListeners = new SimpleEventListeners<ISaveOrUpdateEventListener>();
        /// <summary>
        /// Maps to NHibernate's equivalent event listener
        /// </summary>
        public SimpleEventListeners<ISaveOrUpdateEventListener> UpdateEventListeners = new SimpleEventListeners<ISaveOrUpdateEventListener>();
    }

    /// <summary>
    /// 
    /// </summary>
    public class SimpleEventListeners<T>
    {
        private IList<T> _listeners;

        /// <summary>
        /// Any customer listeners that were added
        /// </summary>
        public IEnumerable<T> Listeners { get { return _listeners; } }

        /// <summary>
        /// If true this will attempt to merge with any listeners of this type that were defaulted by NHibernate.  If false
        /// this will overwrite whatever default listeners are present with the custom listeners.
        /// </summary>
        public bool MergeWithFrameworkListeners { get; set; }

        /// <summary>
        /// Number of custom listeners that were added.
        /// </summary>
        public int ListenersCount
        {
            get { return _listeners == null ? 0 : _listeners.Count; }
        }

        /// <summary>
        /// Helper method for adding listeners in a safe and chainable manner.
        /// </summary>
        /// <param name="listener"></param>
        /// <returns></returns>
        public SimpleEventListeners<T> AddListener(T listener)
        {
            if (_listeners == null)
                _listeners = new List<T>();

            _listeners.Add(listener);

            return this;
        }
    }


}