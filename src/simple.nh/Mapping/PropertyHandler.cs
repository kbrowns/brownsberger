using System;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using Simple.NH.Modeling;

namespace Simple.NH.Mapping.Handlers
{
    public class PropertyHandler
    {
        public void HandleBefore(IModelInspector modelInspector, PropertyPath member, IPropertyMapper propertyMapper)
        {
            var propertyInfo = member.LocalMember.ToEntityPropertyInfo();

            IPropertyMapping mapping = propertyInfo.GetPropertyMapping();

            bool mapColumn = true;

            if (mapping.PropertyType != null)
            {
                propertyMapper.Type(mapping.GetPropertyType());
            }
            else
            {
                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ReferenceTo<>))
                {
                    Type referenceType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    Type referenceToType = typeof(ReferenceToType<>).MakeGenericType(referenceType);

                    propertyMapper.Type(referenceToType, null);

                    AbstractReferenceToType.MapColumns(referenceType, propertyMapper);

                    mapColumn = false;
                }

                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(BitwiseMask<>))
                {
                    Type referenceType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    Type referenceToType = typeof(BitwiseMaskType<>).MakeGenericType(referenceType);

                    propertyMapper.Type(referenceToType, null);
                }

                if (propertyInfo.PropertyType == typeof(DateTimeOffset) || propertyInfo.PropertyType == typeof(DateTimeOffset?))
                    propertyMapper.Type<DateTimeOffsetType>();
            }

            if (mapColumn)
            {
                propertyMapper.Update(!mapping.IgnoreOnUpdate);

                propertyMapper.Column(x =>
                {
                    x.Unique(mapping.IsUnique);
                    SetColumnName(member, propertyInfo, x, mapping);
                    x.NotNullable(!mapping.IsNullable);

                    if (mapping.Default != null)
                        x.Default(mapping.Default);

                    if (mapping.LengthSpecified)
                        x.Length(mapping.Length);
                });
            }
        }

        private static void SetColumnName(PropertyPath path, EntityPropertyInfo property, IColumnMapper columnMapper, IPropertyMapping propertyMapping)
        {
            if (property.EntityType.IsDataComponent())
                SetComponentColumnName(path, columnMapper, propertyMapping);
            else
                columnMapper.Name(propertyMapping.ColumnName);
        }

        private static void SetComponentColumnName(PropertyPath member, IColumnMapper columnMapper, IPropertyMapping propertyMapping)
        {
            var componentProperty = member.PreviousPath.LocalMember.ToEntityPropertyInfo().GetPropertyMapping();
            columnMapper.Name("{0}{1}".FormatWith(componentProperty.ComponentColumnPrefix, propertyMapping.ColumnName));
        }
    }
}