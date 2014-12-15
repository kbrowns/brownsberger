using System.Collections.Generic;
using NHibernate.Engine;

namespace Simple.NH.Querying
{
    /// <summary>
    /// Allows the constructor to specify filter definitions that will be adding to the configuration object via the 
    /// OnPreMapping hook.  This means these filter definitions will be added to the configuration before the mappings are
    /// added to the configuration object.
    /// </summary>
    public class WireUpFiltersFeature : Feature
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filters"></param>
        public WireUpFiltersFeature(IEnumerable<FilterDefinition> filters)
        {
           this.OnPreMapping = (manifest, cfg) =>
                {
                    if(filters != null)
                    {
                        foreach (var filterDefinition in filters)
                        {
                            cfg.AddFilterDefinition(filterDefinition);
                        }
                    }
                };
        }
    }
}