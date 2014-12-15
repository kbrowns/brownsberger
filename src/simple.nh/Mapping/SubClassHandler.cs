using System;
using NHibernate.Mapping.ByCode;

namespace Simple.NH.Mapping.Handlers
{
    public class SubClassHandler
    {
        public void HandleBefore(IModelInspector modelinspector, Type type, ISubclassAttributesMapper customizer)
        {
            ISubClassMapping mapping = GetSubClassMapping(type);
            customizer.DiscriminatorValue(mapping.DiscriminatorValue);
        }

        private static ISubClassMapping GetSubClassMapping(Type type)
        {
            SubClassMappingAttribute specifiedMapping = type.GetAttribute<SubClassMappingAttribute>(false);
            DefaultSubClassMapping defaultMapping = new DefaultSubClassMapping(type);
            ISubClassMapping mapping;

            if (specifiedMapping == null)
            {
                mapping = defaultMapping;
            }
            else
            {
                specifiedMapping.FillDefaults(defaultMapping);
                mapping = specifiedMapping;
            }
            return mapping;
        }
    }
}