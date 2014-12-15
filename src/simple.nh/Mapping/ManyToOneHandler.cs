using NHibernate.Mapping.ByCode;

namespace Simple.NH.Mapping.Handlers
{
    public class ManyToOneHandler
    {
        public void HandleBefore(IModelInspector inspector, PropertyPath member, IManyToOneMapper propertyMapper)
        {
            var property = member.LocalMember.ToEntityPropertyInfo();

            IAssociationMapping mapping = property.GetAssociationMapping();

            LazyRelation lazyRelation = mapping.GetLazyRelation();

            propertyMapper.Lazy(lazyRelation);

            string columnName = mapping.ColumnName;
            string foreignKeyName = mapping.ForeignKeyName;

            if (mapping.NoForeignKey)
                propertyMapper.ForeignKey("none");
            else
                propertyMapper.ForeignKey(foreignKeyName);

            propertyMapper.Column(k =>
            {
                k.Name(columnName);
                k.NotNullable(!mapping.IsNullable);
            });

            if (lazyRelation == LazyRelation.NoLazy)
                propertyMapper.Fetch(FetchKind.Join);

            propertyMapper.Cascade(mapping.Cascade);
        }
    }
}