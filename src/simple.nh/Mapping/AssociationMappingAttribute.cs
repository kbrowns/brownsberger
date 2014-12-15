using System;
using NHibernate.Mapping.ByCode;

namespace Simple.NH.Mapping
{
    public sealed class AssociationMappingAttribute : Attribute, IAssociationMapping
    {
        private LazyRelations _lazyRelation;
        private bool _isNullable;
        private Cascade _cascade;
        private AssociationTypes _associationType;
        public string ColumnName { get; set; }
        public string ForeignKeyName { get; set; }
        public bool NoForeignKey { get; set; }

        public AssociationTypes AssociationType
        {
            get { return _associationType; }
            set
            {
                _associationType = value;
                AssociationTypeSpecified = true;
            }
        }

        internal bool AssociationTypeSpecified { get; set; }

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

        public bool IsNullable
        {
            get { return _isNullable; }
            set
            {
                _isNullable = value;
                IsNullableSpecified = true;
            }
        }

        internal bool IsNullableSpecified { get; set; }
        public LazyRelations LazyRelation
        {
            get { return _lazyRelation; }
            set
            {
                _lazyRelation = value;
                LazyRelationSpecified = true;
            }
        }

        internal bool LazyRelationSpecified { get; set; }

        public LazyRelation GetLazyRelation()
        {
            return DefaultAssociationMapping.ToLazyRelation(LazyRelation);
        }

        public bool GetIsNullable()
        {
            return IsNullable;
        }

        public void Initialize(DefaultAssociationMapping defaultMapping)
        {
            if (!LazyRelationSpecified)
                LazyRelation = defaultMapping.DefaultLazyRelation;

            if (ColumnName == null)
                ColumnName = defaultMapping.ColumnName;

            if (ForeignKeyName == null)
                ForeignKeyName = defaultMapping.ForeignKeyName;

            if (!IsNullableSpecified)
                IsNullable = defaultMapping.IsNullable;

            if (!AssociationTypeSpecified)
                AssociationType = defaultMapping.AssociationType;

            if (!CascadeSpecified)
                Cascade = defaultMapping.Cascade;
        }
    }
}