using NHibernate;
using NHibernate.Engine;

namespace Simple.NH.Seeding
{
    public class SeedDataFactoryContext
    {
        public SeedDataFactoryContext(IModelConfig config, NHibernate.Cfg.Configuration configuration, ISessionFactory sessionFactory, string schema)
        {
            Schema = schema;
            Config = config;
            Configuration = configuration;
            SessionFactory = (ISessionFactoryImplementor)sessionFactory;
        }

        public IModelConfig Config { get; private set; }

        public NHibernate.Cfg.Configuration Configuration { get; private set; }

        public ISessionFactoryImplementor SessionFactory { get; private set; }

        public string Schema { get; private set; }
    }
}
