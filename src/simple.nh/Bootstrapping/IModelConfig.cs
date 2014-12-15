using System;
using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Cfg.Loquacious;

namespace Simple.NH
{
    public interface IModelConfig
    {
        string DefaultSchema { get; }
        IEnumerable<Type> CandidateTypes { get; }
        void Initialize(IDbIntegrationConfigurationProperties db, Configuration config, IModelConfig model);
        void ConfigureOptions(SimpleNHOptions options);
    }
}