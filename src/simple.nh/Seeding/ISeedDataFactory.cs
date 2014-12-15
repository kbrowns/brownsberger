using System;
using System.Collections.Generic;
using NHibernate;
using Simple.NH.Modeling;

namespace Simple.NH.Seeding
{
    public interface ISeedDataFactory
    {
        IEnumerable<IDomainEntity> GetSeedData(SeedDataFactoryContext context);
    }

    public interface ISeedDataFactoryHandler
    {
        void Handle(SeedDataFactoryContext context, ISession session, ISeedDataFactory factory);
    }

    public class DefaultSeedDataFactoryHandler : ISeedDataFactoryHandler
    {
        public void Handle(SeedDataFactoryContext context, ISession session, ISeedDataFactory factory)
        {
            Console.WriteLine("[{0}] Calling LoadToSession() on {1}", DateTime.Now, factory.GetType().Name);

            foreach (var instance in factory.GetSeedData(context))
            {
                if (instance != null)
                {
                    session.Save(instance);
                }
            }

            session.Flush(); // must force the OnPreInsert events to fire right now so audit stamping works on the proper entity state

            Console.WriteLine("[{0}] Called LoadToSession() on {1}", DateTime.Now, factory.GetType().Name);

        }
    }
}