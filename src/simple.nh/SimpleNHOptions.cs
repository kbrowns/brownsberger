using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;
using NHibernate;

namespace Simple.NH
{
    /// <summary>
    /// Simple DTO representing a collection of Feature instances with helper methods
    /// for mutating the contents.
    /// </summary>
    public class SimpleNHOptions
    {
        private readonly List<Feature> _features = new List<Feature>();
        private Func<IModelConfig, ISessionFactory, UnitOfWork> _unitOfWorkProvider;
        private Func<IModelConfig, ContextInstancePolicy> _contextInstancePolicyProvider = DefaultContextInstancePolicy;

        private static ContextInstancePolicy DefaultContextInstancePolicy(IModelConfig arg)
        {
            if (HttpContext.Current != null)
                return ContextInstancePolicy.managed_web;

            if (OperationContext.Current != null)
                return ContextInstancePolicy.wcf_operation;

            return ContextInstancePolicy.isolated_thread_static;
        }

        private Func<IModelConfig, NHibernate.Cfg.Configuration> _configurationProvider;

        /// <summary>
        /// Returns the features that have been added to the options.  The returned object will always be a valid object - null will never be returned.
        /// </summary>
        /// <value></value>
        public IEnumerable<Feature> Features
        {
            get { return _features; }
        }

        /// <summary>
        /// Provide an extensibility point where you can override the implementation of the IPersistentSession interface that's used by the runtime.
        /// </summary>
        public Func<IModelConfig, ISessionFactory, UnitOfWork> UnitOfWorkProvider
        {
            get { return _unitOfWorkProvider; }
            set
            {
                value.CheckArg("value");
                _unitOfWorkProvider = value;
            }
        }

        /// <summary>
        /// Provides a hook to override behavior performed at the time the NHibernate configuration object 
        /// is be constructed prior to when a SessionFactory is being constructed.  This only happens 
        /// once per IDomainModelManifest for the lifetime of the application.
        /// </summary>
        public Func<IModelConfig, NHibernate.Cfg.Configuration> ConfigurationProvider
        {
            get { return _configurationProvider; }
            set
            {
                value.CheckArg("value");
                _configurationProvider = value;
            }
        }

        /// <summary>
        /// Provides an extensibility point changing the session context instance policy.  By default the framework will set the
        /// context policy based on runtime conditions - i.e. if HttpContext.Current is present, the managed_web policy will be used.  If 
        /// the WCF operation context is present, the wcf_operation policy will be used.  Otherwise, thread_static will be used.
        /// </summary>
        public Func<IModelConfig, ContextInstancePolicy> ContextInstancePolicyProvider
        {
            get { return _contextInstancePolicyProvider; }
            set
            {
                value.CheckArg("value");
                _contextInstancePolicyProvider = value;
            }
        }


        /// <summary>
        /// Adds a feature instance to the inner collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <returns></returns>
        public SimpleNHOptions AddFeature<T>(T feature) where T : Feature
        {
            return this.AddFeature(feature, true);
        }

        /// <summary>
        /// Adds a feature instance to the inner collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="feature"></param>
        /// <param name="preventDuplicates"></param>
        /// <returns></returns>
        public SimpleNHOptions AddFeature<T>(T feature, bool preventDuplicates) where T : Feature
        {
            if (!preventDuplicates)
            {
                _features.Add(feature);
            }
            else
            {
                if (!Contains<T>())
                    _features.Add(feature);
            }
            
            return this;
        }

        /// <summary>
        /// Returns true of an instance of the specified type already exists.  Otherwise false.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Contains<T>() where T : Feature
        {
            return _features.OfType<T>().Any();
        }

        /// <summary>
        /// Removes all instances of the specified type from the inner collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RemoveInstancesOf<T>() where T : Feature
        {
            for (int i = 0; i < _features.Count; i++)
            {
                var feature = _features[i];

                if (feature is T)
                {
                    _features.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}