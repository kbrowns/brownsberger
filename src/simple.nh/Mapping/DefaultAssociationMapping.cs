using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping.ByCode;
using Simple.NH.Exceptions;
using Simple.NH.ExtensionMethods;
using Simple.NH.Modeling;

namespace Simple.NH.Mapping
{
    public interface IAssociationMapping
    {
        string ColumnName { get; }
        string ForeignKeyName { get; }
        LazyRelation GetLazyRelation();
        bool IsNullable { get; }
        Cascade Cascade { get; }
        bool NoForeignKey { get; }
        AssociationTypes AssociationType { get; }
    }

    public enum AssociationTypes
    {
        ManyToOne,
        OneToOne
    }

    public enum LazyRelations
    {
        Proxy,
        NoLazy,
        NoProxy
    }

    public struct ForeignKeyName
    {
        private readonly string _name;

        public ForeignKeyName(EntityPropertyInfo property)
        {
            _name = "FK_{0}_{1}".FormatWith(property.EntityType.GetForeignKeyNamePart(), property.PropertyType.GetForeignKeyNamePart());
        }

        public ForeignKeyName(Type type1, Type type2)
        {
            _name = "FK_{0}_{1}".FormatWith(type1.GetForeignKeyNamePart(), type2.GetForeignKeyNamePart());
        }

        public override string ToString()
        {
            return _name;
        }

        public static bool IsTemporaryName(string name)
        {
            return !name.StartsWith("FK_");
        }
    }
    public class DefaultAssociationMapping : IAssociationMapping
    {
        private readonly EntityPropertyInfo _propertyInfo;
        private string _columName;

        public DefaultAssociationMapping(EntityPropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
            ForeignKeyName = new ForeignKeyName(_propertyInfo).ToString();
            Cascade = propertyInfo.PropertyType.IsAssignableTo(typeof(ReferenceEntity<>)) ? Cascade.None : Cascade.Persist;
        }

        public bool IsNullable { get; set; }

        public LazyRelations DefaultLazyRelation { get { return LazyRelations.Proxy; } }

        public string ForeignKeyName { get; private set; }

        public LazyRelation GetLazyRelation()
        {
            return ToLazyRelation(DefaultLazyRelation);
        }

        public string ColumnName
        {
            get
            {
                if (_columName == null)
                    InitColumnName();

                return _columName;
            }
        }

        private void InitColumnName()
        {
            // this code should never be hit if an override is present for the association
            List<PropertyInfo> propertyCollisions = null;

            if (_propertyInfo.EntityType != null)
            {
                propertyCollisions = _propertyInfo.EntityType
                                                  .GetProperties()
                                                  .Where(x => !x.HasAttribute<IgnoreInspectionAttribute>()
                                                    && x.PropertyType == _propertyInfo.PropertyType)
                                                  .ToList();
            }


            if (propertyCollisions != null && propertyCollisions.Count > 1) // greater than 1 because _propertyInfo itself will be in this list
            {
                var buffer = new StringBuilder();

                buffer.Append("The default association naming strategy encountered a problem.  ");
                buffer.Append("The default convention is to name foreign key columns in the ");
                buffer.Append("following form: [foreign-table_id].  When attempting to apply this ");
                buffer.Append("convention, it was detected that type '{0}' has multiple association ".FormatWith(_propertyInfo.EntityType));
                buffer.Append("properties of type '{0}' which if used in conjunction with this convention ".FormatWith(_propertyInfo.PropertyType));
                buffer.Append("would result in a column name collision.  ");
                buffer.Append("For entities of this nature the default convention must be overriden ");
                buffer.Append("and the column and foreign key names must be specified explicitly ");
                buffer.Append("via the AssociationMappingAttribute.  Colliding properties:");
                buffer.AppendLine();

                foreach (var collision in propertyCollisions)
                {
                    buffer.AppendLine("     {0}".FormatWith(collision.Name));
                }

                throw new SimpleNHException(buffer.ToString());
            }

            _columName = "{0}_id".FormatWith(_propertyInfo.PropertyType.Name.ToDbSchemaName());
        }

        public Cascade Cascade { get; private set; }

        public bool NoForeignKey { get { return false; } }

        public AssociationTypes AssociationType { get { return AssociationTypes.ManyToOne; } }

        internal static LazyRelation ToLazyRelation(LazyRelations relation)
        {
            switch (relation)
            {
                case LazyRelations.NoLazy:
                    return LazyRelation.NoLazy;
                case LazyRelations.NoProxy:
                    return LazyRelation.NoProxy;
            }

            return LazyRelation.Proxy;
        }
    }

}
