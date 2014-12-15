using System;
using NHibernate.Mapping.ByCode;
using Simple.NH.Querying;

namespace Simple.NH.Mapping.Handlers
{
    public class CollectionHandler
    {
        public void HandleBefore(IModelInspector inspector, PropertyPath member, IBagPropertiesMapper property)
        {
            MapCollection(inspector, member, property);
        }

        public void HandleBefore(IModelInspector inspector, PropertyPath member, ISetPropertiesMapper property)
        {
            MapCollection(inspector, member, property);
        }

        public void HandleBefore(IModelInspector inspector, PropertyPath member, IListPropertiesMapper property)
        {
            MapCollection(inspector, member, property);
        }

        private void MapCollection(IModelInspector inspector, PropertyPath member, ICollectionPropertiesMapper property)
        {
            EntityPropertyInfo propertyInfo = member.LocalMember.ToEntityPropertyInfo();

            var mapping = propertyInfo.GetCollectionMapping(member, inspector);

            if (propertyInfo.GetBackingField() != null)
            {
                property.Access(Accessor.Field);
            }

            property.Inverse(mapping.Inverse);
            property.Key(x =>
            {
                x.Column(mapping.ColumnName);
                x.ForeignKey(mapping.ForeignKeyName);
            });

            var filters = ManageFiltersFeature.GetFiltersForType(propertyInfo.PropertyType.GetEnumeratorType());
            foreach (Tuple<string, Action<IFilterMapper>> filter in filters)
            {
                property.Filter(filter.Item1, filter.Item2);
            }

            Cascade? cascade = mapping.GetCascade();

            CollectionFetchModes? fetchMode = mapping.GetFetchMode();

            if (cascade.HasValue)
                property.Cascade(cascade.Value);

            if (fetchMode.HasValue)
            {
                switch (fetchMode.Value)
                {
                    case CollectionFetchModes.Join:
                        property.Fetch(CollectionFetchMode.Join);
                        break;
                    case CollectionFetchModes.Select:
                        property.Fetch(CollectionFetchMode.Select);
                        break;
                    case CollectionFetchModes.SubSelect:
                        property.Fetch(CollectionFetchMode.Subselect);
                        break;
                }
            }

        }
    }
}