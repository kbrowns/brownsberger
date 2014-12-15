using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping.ByCode;

namespace Simple.NH.Querying
{
    public class ManageFiltersFeature : Feature
    {
        public ManageFiltersFeature()
        {
            base.OnPreMapping = (dmm, cfg) =>
            {
                var filters = GetFilterDefinitions(dmm);

                foreach (var filter in filters)
                {
                    if (!cfg.FilterDefinitions.ContainsKey(filter.Name.ToString()))
                    {
                        var definition = new BridgeFilterDefinition(filter);
                        cfg.FilterDefinitions.Add(filter.Name.ToString(), definition);
                    }
                }
            };

            base.OnOpenSession = (dmm, session) =>
            {
                var @event = new SessionOpenedEvent();

                Pipeline.Raise(@event);

                // all filters are disabled until enabled, so let's enable
                // all filters that were not otherwise asked to be disabled.
                // for those asked to be disabled, just don't enable them
                foreach (var filterName in session.SessionFactory.DefinedFilterNames)
                {
                    if (!@event.ShouldDisableFilter(filterName))
                    {
                        var filter = session.EnableFilter(filterName);
                        var definition = (BridgeFilterDefinition)filter.FilterDefinition;
                        definition.Definition.LoadParameters(filter);
                    }
                }
            };
        }

        private IEnumerable<Filter> GetFilterDefinitions(IModelConfig dmm)
        {
            var result = new Dictionary<Type, Filter>();

            if (dmm.CandidateTypes != null)
            {
                foreach (var candidateType in dmm.CandidateTypes)
                {
                    var defineFilter = candidateType.GetAttribute<AttachFilterAttribute>(true);

                    if (defineFilter != null && !result.ContainsKey(defineFilter.FilterType))
                    {
                        result.Add(defineFilter.FilterType, defineFilter.FilterType.CreateInstanceAs<Filter>());
                    }
                }
            }

            return result.Values;
        }

        public static IEnumerable<Tuple<string, Action<IFilterMapper>>> GetFiltersForType(Type type)
        {
            var defineFilterAttributes = type.GetAttributes<AttachFilterAttribute>(true);
            var disableFilterAttributes = type.GetAttributes<DetachFilterAttribute>(true).ToArray();

            foreach (var filter in defineFilterAttributes)
            {
                if (disableFilterAttributes.All(x => x.FilterType != filter.FilterType))
                {
                    var filterInstance = filter.FilterType.CreateInstanceAs<Filter>();
                    yield return new Tuple<string, Action<IFilterMapper>>(filterInstance.Name, cfg => cfg.Condition(filterInstance.GetCondition()));
                }
            }
        }
    }
}
