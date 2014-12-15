using System;
using System.Collections.Generic;
using System.Linq;
using Simple.NH.Mapping;
using Simple.NH.Seeding;

namespace Simple.NH.Modeling
{
    public abstract class ReferenceEntity<TEntity> : IReferenceEntity
        where TEntity : IReferenceEntity, ISeedDataFactory
    {
        private static readonly Lazy<TEntity[]> Enumerations = new Lazy<TEntity[]>(GetEnumerations);

        protected ReferenceEntity() { }

        protected ReferenceEntity(long id, string name)
        {
            Id = id;
            Name = name;
        }

        [PropertyMapping(IsNullable = false)]
        public virtual string Name { get; set; }

        public override string ToString()
        {
            return this.Name;
        }

        public static TEntity[] GetAll()
        {
            return Enumerations.Value;
        }

        public static TEntity[] GetActive()
        {
            return Enumerations.Value.Where(x => x.IsActive).ToArray();
        }

        public static TEntity[] GetInActive()
        {
            return Enumerations.Value.Where(x => !x.IsActive).ToArray();
        }

        public static TEntity FromValue(long id)
        {
            return Parse(id, "id", item => item.Id.Equals(id));
        }

        public static TEntity Parse(string name)
        {
            return Parse(name, "name", item => item.Name == name);
        }

        static bool TryParse(Func<TEntity, bool> predicate, out TEntity result)
        {
            result = GetAll().FirstOrDefault(predicate);
            return result != null;
        }

        private static TEntity Parse(object value, string description, Func<TEntity, bool> predicate)
        {
            TEntity result;

            if (!TryParse(predicate, out result))
            {
                string message = string.Format("'{0}' is not a valid {1} in {2}", value, description, typeof(TEntity));
                throw new ArgumentException(message, "value");
            }

            return result;
        }

        public static bool TryParse(long id, out TEntity result)
        {
            return TryParse(e => e.Id.Equals(id), out result);
        }

        public static bool TryParse(string name, out TEntity result)
        {
            return TryParse(e => e.Name == name, out result);
        }

        private static TEntity[] GetEnumerations()
        {
            return typeof(TEntity).GetEnumerations<TEntity>();
        }

        protected virtual IEnumerable<TEntity> GetSeedData(SeedDataFactoryContext context)
        {
            var seedData = GetEnumerations();
            InitializeSeedData(seedData);
            return seedData;
        }

        protected virtual void InitializeSeedData(IEnumerable<TEntity> seedData) { }

        public long Id { get; private set; }
        
        public bool IsActive { get; set; }
    }

}
