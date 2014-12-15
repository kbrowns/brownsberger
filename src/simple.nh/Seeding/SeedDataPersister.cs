using System;
using NHibernate;

namespace Simple.NH.Seeding
{
    public class SeedDataPersister
    {
        private readonly IModelConfig _config;
        private readonly SeedDataFactoryContext _seedDataFactoryContext;

        public SeedDataPersister(IModelConfig config, NHibernate.Cfg.Configuration configuration, ISessionFactory sessionFactory, string schema)
        {
            _config = config;
            _seedDataFactoryContext = new SeedDataFactoryContext(_config, configuration, sessionFactory, schema);
        }

        public void Persist()
        {
            Console.WriteLine("[{0}] Persist() Entered", DateTime.Now);

            var factories = _config.GetSeedDataFactories();

            using (var session = _seedDataFactoryContext.SessionFactory.OpenSession())
            using (var tran = session.BeginTransaction())
            {
                foreach (var factory in factories)
                {
                    // ReSharper disable once SuspiciousTypeConversion.Global
                    var handler = factory as ISeedDataFactoryHandler ?? new DefaultSeedDataFactoryHandler();

                    handler.Handle(_seedDataFactoryContext, session, factory);                    
                }

                tran.Commit();
            }

            Console.WriteLine("[{0}] Persist() Existed", DateTime.Now);
        }
    }

}
