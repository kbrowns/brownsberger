using System;
using System.Collections.Generic;
using System.Linq;
using Simple.NH;
using Simple.NH.Exceptions;
using Simple.NH.Seeding;

public static class ModelConfigExtensionMethods
{
    public static IEnumerable<ISeedDataFactory> GetSeedDataFactories(this IModelConfig manifest)
    {
        return manifest.CandidateTypes.Where(IsSeedDataFactory)
            .Distinct()
            .Select(ConstructSeedDataFactory);
    }

    private static bool IsSeedDataFactory(Type candidate)
    {
        if (!candidate.IsConcreteClass() || !typeof(ISeedDataFactory).IsAssignableFrom(candidate))
            return false;

        return true;
    }

    private static ISeedDataFactory ConstructSeedDataFactory(Type candidate)
    {
        try
        {
            var seedDataFactory = (ISeedDataFactory)Activator.CreateInstance(candidate, true);

            return seedDataFactory;
        }
        catch (Exception ex)
        {
            throw new SimpleNHException(
                "Failed to construct seed data factory type {0}".FormatWith(candidate), ex);
        }
    }
}