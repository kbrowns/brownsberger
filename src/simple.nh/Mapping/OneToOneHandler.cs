using System.Reflection;
using NHibernate.Mapping.ByCode;

namespace Simple.NH.Mapping.Handlers
{
    public class OneToOneHandler
    {
        public void HandleBefore(IModelInspector inspector, PropertyPath member, IOneToOneMapper mapper)
        {
            var property = member.LocalMember as PropertyInfo;

            if (null != property)
            {
                mapper.Lazy(LazyRelation.Proxy);

                var fk = new ForeignKeyName(property.PropertyType, member.GetContainerEntity(inspector));
                mapper.ForeignKey(fk.ToString());

                mapper.Cascade(Cascade.Persist);
            }
        }
    }
}