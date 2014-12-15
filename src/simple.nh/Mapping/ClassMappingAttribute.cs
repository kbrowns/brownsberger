using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using Simple.NH.ExtensionMethods;

namespace Simple.NH.Mapping
{
    public enum IdentifierStrategies
    {
        HiloIdGenerator,
        Assigned,
        Identity,
        Custom,
        GuidComb
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ClassMappingAttribute : Attribute, IClassMapping
    {
        private IdentifierStrategies _identifierStrategy;
        private EntityCacheUsage _cacheStrategy;
        private EntityPropertyInfo _concurrencyLockProperty;
        private bool _isMutable;
        private string _identifierColumnName;

        public ClassMappingAttribute()
        {
            DynamicUpdate = DefaultClassMapping.DefaultDynamicUpdate;
        }

        public void Initialize(DefaultClassMapping defaults)
        {
            if (TableName.IsNullOrEmpty())
                TableName = defaults.TableName;

            if (SchemaName.IsNullOrEmpty())
                SchemaName = defaults.SchemaName;

            if (!this.IdentifierStrategySpecified)
                this.IdentifierStrategy = defaults.IdentifierStrategy;

            if (!IsMutableSpecified)
                IsMutable = defaults.IsMutable;

            _identifierColumnName = defaults.GetIdentifierColumnName();
            _concurrencyLockProperty = defaults.GetConcurrencyLockProperty();
        }

        public void Finalize(IClassAttributesMapper classMapper)
        {

        }

        public EntityPropertyInfo GetConcurrencyLockProperty()
        {
            return _concurrencyLockProperty;
        }

        public string GetIdentifierColumnName()
        {
            return _identifierColumnName;
        }

        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public bool DynamicUpdate { get; set; }
        public Type CustomIdGeneratorType { get; set; }

        public EntityCacheUsage CacheStrategy
        {
            get { return _cacheStrategy; }
            set
            {
                _cacheStrategy = value;
                CacheStrategySpecified = true;
            }
        }
        internal bool CacheStrategySpecified { get; set; }

        /// <summary>
        /// Name of the cache region defined in your app or web config
        /// Config Documentation: http://docs.huihoo.com/hibernate/nhibernate-reference-1.2.0/caches.html
        /// </summary>
        public string CacheRegion { get; set; }

        public IdentifierStrategies IdentifierStrategy
        {
            get { return _identifierStrategy; }
            set
            {
                _identifierStrategy = value;
                this.IdentifierStrategySpecified = true;
            }
        }

        internal bool IdentifierStrategySpecified { get; set; }

        internal bool IsMutableSpecified { get; set; }

        public bool IsMutable
        {
            get { return _isMutable; }
            set
            {
                _isMutable = value;
                IsMutableSpecified = true;
            }
        }

        public SchemaAction SchemaAction { get; set; }

        IGeneratorDef IClassMapping.CreateGenerator()
        {
            return DefaultClassMapping.CreateGenerator(this.IdentifierStrategy, CustomIdGeneratorType);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ConcurrencyLockAttribute : Attribute
    {
        public bool IsDatabaseGenerated { get; set; }
    }

    public class DefaultClassMapping : IClassMapping
    {
        internal const bool DefaultDynamicUpdate = true;

        private readonly Type _type;

        public DefaultClassMapping(Type type)
        {
            _type = type;

            // default the table to the entity name in (foo_bar_baz) with "Entity" trimmed off.
            TableName = _type.ToDbSchemaName();
            SchemaName = null;
            DynamicUpdate = DefaultDynamicUpdate;

            var isReferenceEntity = _type.IsReferenceEntity();
            IdentifierStrategy = isReferenceEntity ? IdentifierStrategies.Assigned : GetDefaultIdentifierStrategy();

            IsMutable = !isReferenceEntity;
        }

        private IdentifierStrategies GetDefaultIdentifierStrategy()
        {
            return IdentifierStrategies.HiloIdGenerator;
        }

        public string TableName { get; private set; }
        public string SchemaName { get; private set; }
        public bool DynamicUpdate { get; private set; }
        public static Type CustomIdGeneratorType { get; set; }
        public IdentifierStrategies IdentifierStrategy { get; private set; }
        public bool IsMutable { get; private set; }

        public SchemaAction SchemaAction { get { return SchemaAction.All; } }

        public IGeneratorDef CreateGenerator()
        {
            return CreateGenerator(this.IdentifierStrategy, CustomIdGeneratorType);
        }

        public void Initialize(DefaultClassMapping defaultMapping)
        {

        }

        public void Finalize(IClassAttributesMapper classMapper)
        {
        }

        public EntityPropertyInfo GetConcurrencyLockProperty()
        {
            var property = _type.GetProperties().SingleOrDefault(p => p.HasAttribute<ConcurrencyLockAttribute>());

            if (property == null)
                return null;

            return new EntityPropertyInfo(property, _type);
        }

        public string GetIdentifierColumnName()
        {
            return ConventionBasedModeInspector.GetIdentifierColumnName(_type);
        }

        public static IGeneratorDef CreateGenerator(IdentifierStrategies strategy, Type customIdGeneratorType)
        {
            switch (strategy)
            {
                case IdentifierStrategies.Assigned:
                    return new AssignedGeneratorDef();
                case IdentifierStrategies.Identity:
                    return new IdentityGeneratorDef();
                case IdentifierStrategies.Custom:
                    return (IGeneratorDef)Activator.CreateInstance(customIdGeneratorType);
                case IdentifierStrategies.GuidComb:
                    return new GuidCombGeneratorDef();
                default:
                    return new HighLowGeneratorDef();
            }
        }
    }

    public interface IClassMapping
    {
        string TableName { get; }
        string SchemaName { get; }
        bool DynamicUpdate { get; }
        IdentifierStrategies IdentifierStrategy { get; }
        bool IsMutable { get; }
        SchemaAction SchemaAction { get; }
        IGeneratorDef CreateGenerator();
        void Initialize(DefaultClassMapping defaultMapping);
        void Finalize(IClassAttributesMapper classMapper);
        EntityPropertyInfo GetConcurrencyLockProperty();
        string GetIdentifierColumnName();
    }
}
