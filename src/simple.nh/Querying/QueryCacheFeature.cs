using NHibernate.Cfg;

namespace Simple.NH.Querying
{
    /// <summary>
    /// Turns on the query caching feature
    /// </summary>
    public class QueryCacheFeature : Feature
    {
        /// <summary>
        /// Turn on Query caching in NHibernate. This works in conjuction with FrameworkCacheProvide and FrameworkCache
        /// http://stackoverflow.com/questions/15492302/nhibernate-l2-caching-configure-by-code
        /// Somehow turning on the cache.use_second_level_cache and cache.use_query_cache does properties does not seem to turn this feature on and 
        /// doing it by code (below) was the only way it does. 
        /// </summary>
        public QueryCacheFeature()
        {
            OnInitConnectionConfig = (manifest, configuration) => configuration.Cache(x => x.UseQueryCache = true);
        }
    }
}