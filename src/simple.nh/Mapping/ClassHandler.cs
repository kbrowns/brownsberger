using System;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using Simple.NH.Querying;

namespace Simple.NH.Mapping.Handlers
{
    public class ClassHandler
    {
        private readonly string _schema;

        public ClassHandler(IModelConfig config)
        {
            _schema = config.DefaultSchema;
        }

        public void HandleBefore(IModelInspector modelinspector, Type type, IClassAttributesMapper classMapper)
        {
            if (!_schema.IsNullOrEmpty())
                classMapper.Schema(_schema);

            MapInheritance(type, classMapper);

            IClassMapping mapping = type.GetClassMapping();

            // need to find a dialect-agnostic way of doing wrapping this:
            classMapper.Table(mapping.TableName);
            classMapper.Id(x =>
            {
                x.Column(mapping.GetIdentifierColumnName());
                x.Generator(mapping.CreateGenerator());
            });

            classMapper.Mutable(mapping.IsMutable);

            MapSchemaInfo(classMapper, mapping);
            MapConcurrency(classMapper, mapping);
            MapFilters(type, classMapper);

            mapping.Finalize(classMapper);
        }

        private void MapSchemaInfo(IClassAttributesMapper classMapper, IClassMapping mapping)
        {
            if (!mapping.SchemaName.IsNullOrEmpty())
                classMapper.Schema(mapping.SchemaName);
            classMapper.SchemaAction(mapping.SchemaAction);
        }

        private void MapConcurrency(IClassAttributesMapper classMapper, IClassMapping mapping)
        {
            var concurrencyLockProperty = mapping.GetConcurrencyLockProperty();

            if (concurrencyLockProperty != null)
            {
                classMapper.Version(concurrencyLockProperty, cfg => ConfigureVersion(concurrencyLockProperty, cfg));
            }
        }

        private static void MapFilters(Type type, IClassAttributesMapper classMapper)
        {
            var filters = ManageFiltersFeature.GetFiltersForType(type);

            foreach (Tuple<string, Action<IFilterMapper>> filter in filters)
            {
                classMapper.Filter(filter.Item1, filter.Item2);
            }
        }

        private static void MapInheritance(Type type, IClassAttributesMapper classMapper)
        {
            var inheritanceRoot = type.GetAttribute<InheritanceRootAttribute>(false);

            if (inheritanceRoot != null && inheritanceRoot.Scheme == InheritanceMappingSchemes.TablePerClassHierarchy)
            {
                classMapper.Discriminator(d =>
                {
                    switch (inheritanceRoot.DiscriminatorColumnType)
                    {
                        case DiscriminatorColumnTypes.Int16:
                            d.Type(NHibernateUtil.Int16);
                            break;
                        case DiscriminatorColumnTypes.Int32:
                            d.Type(NHibernateUtil.Int32);
                            break;
                        case DiscriminatorColumnTypes.Int64:
                            d.Type(NHibernateUtil.Int64);
                            break;
                        default:
                            d.Type(NHibernateUtil.AnsiString);
                            break;
                    }
                    d.Column(c => c.Name(inheritanceRoot.DiscriminatorColumn));
                }
                    );
            }
        }

        private void ConfigureVersion(EntityPropertyInfo concurrencyLockProperty, IVersionMapper cfg)
        {
            var concurrencySettings = concurrencyLockProperty.GetAttribute<ConcurrencyLockAttribute>();
            var mappingSettings = concurrencyLockProperty.GetPropertyMapping();

            Type propertyType = concurrencyLockProperty.PropertyType;

            cfg.Type(GetVersionType(propertyType));
            cfg.Generated(concurrencySettings.IsDatabaseGenerated ? VersionGeneration.Always : VersionGeneration.Never);
            cfg.UnsavedValue(propertyType.GetDefault());

            cfg.Column(x =>
            {
                x.Name(mappingSettings.ColumnName);
                x.NotNullable(!mappingSettings.IsNullable);

                if (mappingSettings.Default != null)
                    x.Default(mappingSettings.Default);

                if (mappingSettings.LengthSpecified)
                    x.Length(mappingSettings.Length);
            });
        }

        private IVersionType GetVersionType(Type type)
        {
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return new TimestampType();

            if (type == typeof(int) || type == typeof(int?))
                return new Int32Type();

            if (type == typeof(long) || type == typeof(long?))
                return new Int64Type();

            throw new NotSupportedException("{0} is not a supported type for ConcurrencyLock".FormatWith(type.Name));
        }
    }
}