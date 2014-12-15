using System;
using NHibernate.Mapping.ByCode;
using Simple.NH.ExtensionMethods;

namespace Simple.NH.Mapping.Handlers
{
    public class JoinedSubClassHandler
    {
        private readonly IModelConfig _config;

        public JoinedSubClassHandler(IModelConfig config)
        {
            _config = config;
        }

        public void HandleBefore(IModelInspector modelinspector, Type type, IJoinedSubclassAttributesMapper customizer)
        {
            customizer.Table(type.ToDbSchemaName());

            IClassMapping mapping = type.GetClassMapping();
            
            var schema = mapping.SchemaName ?? _config.DefaultSchema;

            if (!schema.IsNullOrEmpty())
                customizer.Schema(schema);
        }
    }
}