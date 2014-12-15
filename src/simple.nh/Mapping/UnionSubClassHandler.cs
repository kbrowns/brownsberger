using System;
using NHibernate.Mapping.ByCode;
using Simple.NH.ExtensionMethods;

namespace Simple.NH.Mapping.Handlers
{
    public class UnionSubClassHandler
    {
        private readonly string _defaultSchema;

        public UnionSubClassHandler(IModelConfig config)
        {
            _defaultSchema = config.DefaultSchema;
        }

        public void HandleBefore(IModelInspector modelinspector, Type type, IUnionSubclassAttributesMapper customizer)
        {
            customizer.Table(type.ToDbSchemaName());

            IClassMapping mapping = type.GetClassMapping();
            var schema = mapping.SchemaName ?? _defaultSchema;
            if (!schema.IsNullOrEmpty())
                customizer.Schema(schema);
        }
    }
}