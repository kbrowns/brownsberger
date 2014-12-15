using System;
using System.Linq;
using System.Reflection;
using NHibernate.Mapping.ByCode;
using Simple.NH.Exceptions;
using Simple.NH.ExtensionMethods;

namespace Simple.NH.Mapping
{
    public sealed class CollectionMappingAttribute : Attribute, ICollectionMapping
    {
        private Cascade _cascade;
        private bool _inverse;
        private CollectionTypes _collectionType;
        private CollectionFetchModes _fetchMode;

        public CollectionTypes CollectionType
        {
            get { return _collectionType; }
            set
            {
                _collectionType = value;
                CollectionTypeSpecified = true;
            }
        }

        internal bool CollectionTypeSpecified { get; set; }

        public CollectionAssociationTypes Association { get; set; }

        public string ColumnName { get; set; }

        public string ForeignKeyName { get; set; }

        public bool Inverse
        {
            get { return _inverse; }
            set
            {
                _inverse = value;
                InverseSpecified = true;
            }
        }

        internal bool InverseSpecified { get; set; }

        public Cascade Cascade
        {
            get { return _cascade; }
            set
            {
                _cascade = value;
                CascadeSpecified = true;
            }
        }

        internal bool CascadeSpecified { get; set; }

        public Cascade? GetCascade()
        {
            if (CascadeSpecified)
                return this.Cascade;

            return null;
        }

        public CollectionFetchModes FetchMode
        {
            get { return _fetchMode; }
            set
            {
                _fetchMode = value;
                FetchModeSpecified = true;
            }
        }

        internal bool FetchModeSpecified { get; set; }

        public CollectionFetchModes? GetFetchMode()
        {
            if (FetchModeSpecified)
                return FetchMode;

            return null;
        }

        internal void FillDefaults(DefaultCollectionMapping defaults)
        {
            if (!this.CascadeSpecified)
            {
                Cascade? cascade = defaults.GetCascade();

                if (cascade.HasValue)
                    this.Cascade = cascade.Value;
            }

            if (!this.CollectionTypeSpecified)
                this.CollectionType = defaults.CollectionType;

            if (!this.InverseSpecified)
                this.Inverse = defaults.Inverse;

            if (this.ColumnName == null)
                this.ColumnName = defaults.ColumnName;

            if (this.ForeignKeyName == null)
                this.ForeignKeyName = defaults.ForeignKeyName;

            if (!this.FetchModeSpecified)
            {
                CollectionFetchModes? fetchMode = defaults.GetFetchMode();

                if (fetchMode.HasValue)
                    this.FetchMode = fetchMode.Value;
            }
        }
    }

    public interface ICollectionMapping
    {
        bool Inverse { get; }
        CollectionTypes CollectionType { get; }
        string ColumnName { get; }
        string ForeignKeyName { get; }
        Cascade? GetCascade();
        CollectionFetchModes? GetFetchMode();
    }

    public class DefaultCollectionMapping : ICollectionMapping
    {
        private readonly EntityPropertyInfo _property;
        private readonly PropertyPath _member;
        private readonly IModelInspector _inspector;

        public DefaultCollectionMapping(EntityPropertyInfo property, PropertyPath member, IModelInspector inspector)
        {
            _property = property;
            _member = member;
            _inspector = inspector;
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            Type enumeratorType = _property.PropertyType.GetEnumeratorType();
            Type declaringType = _property.EntityType;

            EntityPropertyInfo[] backsideManyToOnes = FindBacksideManyToOneAssociations(declaringType, enumeratorType);

            if (backsideManyToOnes.Length < 1) // no backside assocications found
            {
                NameBasedContainingEntity();
            }
            else
            {
                if (backsideManyToOnes.Length > 1) // more than one backside many-to-one
                    BlowUpAndAskTheUserToOverrideAsWeCantInferTheirIntent(declaringType, enumeratorType, backsideManyToOnes);

                NameBasedOnBacksideManyToOne(backsideManyToOnes[0]);
            }
        }

        private void NameBasedOnBacksideManyToOne(EntityPropertyInfo backsideManyToOne)
        {
            var inverseAssociationMapping = GetAssociationMapping(backsideManyToOne);
            this.ColumnName = inverseAssociationMapping.ColumnName;
            this.ForeignKeyName = inverseAssociationMapping.ForeignKeyName;
        }

        private static void BlowUpAndAskTheUserToOverrideAsWeCantInferTheirIntent(Type declaringType, Type enumeratorType,
                                                                                  PropertyInfo[] backsideManyToOnes)
        {
            throw new SimpleNHException(
                "When trying to map a one-to-many collection, the attempt to find the collection item's reference to this containing type was found more than one match.  Expected to find a single property of type '{0}' on type '{1}', but {2} were found.  This requires that you override the One-to-Many property with a CollectionMapping attribute and specify the column and foreign key to use."
                    .FormatWith(declaringType, enumeratorType, backsideManyToOnes.Length));
        }

        private void NameBasedContainingEntity()
        {
            // coulnd't find any many-to-one's pointed at this, so read it off the containing type
            Type containingEntity = _member.GetContainerEntity(_inspector);
            Type manyItemType = _member.LocalMember.GetMemberType().GetEnumeratorType();

            this.ColumnName = "{0}_id".FormatWith(containingEntity.ToDbSchemaName());
            this.ForeignKeyName = new ForeignKeyName(manyItemType, containingEntity).ToString();
        }

        private EntityPropertyInfo[] FindBacksideManyToOneAssociations(Type declaringType, Type enumeratorType)
        {
            return enumeratorType.GetProperties()
                                    .Where(x => x.PropertyType == declaringType)
                                    .Select(x => new EntityPropertyInfo(x, enumeratorType))
                                    .ToArray();
        }

        private IAssociationMapping GetAssociationMapping(EntityPropertyInfo property)
        {
            AssociationMappingAttribute specifiedMapping = property.GetAttribute<AssociationMappingAttribute>(false);
            DefaultAssociationMapping defaultMapping = new DefaultAssociationMapping(property);
            IAssociationMapping mapping;

            if (specifiedMapping == null)
            {
                mapping = defaultMapping;
            }
            else
            {
                specifiedMapping.Initialize(defaultMapping);
                mapping = specifiedMapping;
            }
            return mapping;
        }

        private bool _initialized = false;
        private string _columnName;
        private string _foreignKeyName;
        public bool Inverse { get { return true; } }

        public CollectionTypes CollectionType { get { return CollectionTypes.Bag; } }

        public string ColumnName
        {
            get
            {
                Initialize();

                return _columnName;
            }
            private set { _columnName = value; }
        }

        public string ForeignKeyName
        {
            get
            {
                Initialize();

                return _foreignKeyName;
            }
            private set { _foreignKeyName = value; }
        }

        public Cascade? GetCascade()
        {
            return Cascade.All;
        }

        public CollectionFetchModes? GetFetchMode()
        {
            return null;
        }
    }
        
    public enum CollectionFetchModes
    {
        Select,
        Join,
        SubSelect
    }
    public enum CollectionAssociationTypes
    {
        OneToMany,
        ManyToMany
    }
    public enum CollectionTypes
    {
        Bag,
        Set,
        List
    }
}
