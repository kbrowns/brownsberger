using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;
using Simple.NH.Exceptions;
using Simple.NH.Mapping.Handlers;
using Simple.NH.Modeling;

namespace Simple.NH.Mapping
{
    public class SimpleModelMapper : ModelMapper
    {
        public SimpleModelMapper(IModelConfig config)
            : base(new ConventionBasedModeInspector(),
                    new ExplicitlyDeclaredModel(),
                    new CustomizersHolder(),
                    new ConventionBasedMemberInspector())
        {
            this.BeforeMapSubclass += new SubClassHandler().HandleBefore;
            this.BeforeMapJoinedSubclass += new JoinedSubClassHandler(config).HandleBefore;
            this.BeforeMapUnionSubclass += new UnionSubClassHandler(config).HandleBefore;
            this.BeforeMapClass += new ClassHandler(config).HandleBefore;
            this.BeforeMapBag += new CollectionHandler().HandleBefore;
            this.BeforeMapList += new CollectionHandler().HandleBefore;
            this.BeforeMapSet += new CollectionHandler().HandleBefore;
            this.BeforeMapManyToOne += new ManyToOneHandler().HandleBefore;
            this.BeforeMapOneToOne += new OneToOneHandler().HandleBefore;
            this.BeforeMapProperty += new PropertyHandler().HandleBefore;
        }
    }

    public class ConventionBasedModeInspector : IModelInspector
    {
        public bool IsRootEntity(Type type)
        {
            return InternalIsRootEntity(type);
        }

        internal bool InternalIsRootEntity(Type type)
        {
            if (!IsEntity(type))
                return false;

            if (type.BaseType == null || type.BaseType == typeof(object))
                return true;

            if (type.BaseType.IsAbstract && !type.BaseType.HasAttribute<InheritanceRootAttribute>())
                return true;

            return false;
        }

        public bool IsComponent(Type type)
        {
            return InternalIsComponent(type);
        }

        internal static bool InternalIsComponent(Type type)
        {
            if (type.HasAttribute<IgnoreInspectionAttribute>())
                return false;

            if (type.HasAttribute<TreatAsPropertyAttribute>())
                return false;

            if (type.IsDataComponent())
                return true;

            return false;
        }

        public bool IsEntity(Type type)
        {
            return InternalIsEntity(type);
        }

        internal static bool InternalIsEntity(Type type)
        {
            if (type.HasAttribute<IgnoreInspectionAttribute>())
                return false;

            if (type.IsDataEntity())
                return true;

            return false;
        }

        public bool IsTablePerClass(Type type)
        {
            return InternalIsTablePerClass(type);
        }

        internal static bool InternalIsTablePerClass(Type type)
        {
            var attribute = type.BaseType != null ? type.BaseType.GetAttribute<InheritanceRootAttribute>(false) : null;

            if (attribute == null)
                return false;

            if (attribute.Scheme == InheritanceMappingSchemes.TablePerClass)
                return true;

            return false;
        }

        public bool IsTablePerClassSplit(Type type, object splitGroupId, MemberInfo member)
        {
            return false;
        }

        public bool IsTablePerClassHierarchy(Type type)
        {
            return InternalIsTablePerClassHierarchy(type);
        }

        internal static bool InternalIsTablePerClassHierarchy(Type type)
        {
            var attribute = type.BaseType != null ? type.BaseType.GetAttribute<InheritanceRootAttribute>(false) : null;

            if (attribute == null)
                return false;

            if (attribute.Scheme == InheritanceMappingSchemes.TablePerClassHierarchy)
                return true;

            return false;
        }

        public bool IsTablePerConcreteClass(Type type)
        {
            return InternalIsTablePerConcreteClass(type);
        }

        internal static bool InternalIsTablePerConcreteClass(Type type)
        {
            var attribute = type.BaseType != null ? type.BaseType.GetAttribute<InheritanceRootAttribute>(false) : null;

            if (attribute == null)
                return false;

            if (attribute.Scheme == InheritanceMappingSchemes.TablePerConcreteClass)
                return true;

            return false;
        }

        public bool IsOneToOne(MemberInfo member)
        {
            return InternalIsOneToOne(member);
        }

        internal static bool InternalIsOneToOne(MemberInfo member)
        {
            return IsAssociationOf(AssociationTypes.OneToOne, member);
        }

        public bool IsManyToOne(MemberInfo member)
        {
            return InternalIsManyToOne(member);
        }

        internal static bool InternalIsManyToOne(MemberInfo member)
        {
            return IsAssociationOf(AssociationTypes.ManyToOne, member);
        }
        private static bool IsAssociationOf(AssociationTypes match, MemberInfo member)
        {
            if (!member.IsDataEntity())
                return false;

            var property = member.ToEntityPropertyInfo();

            var mapping = property.GetAssociationMapping();

            return mapping.AssociationType == match;
        }

        public bool IsManyToMany(MemberInfo member)
        {
            return InternalIsManyToMany(member);
        }

        internal static bool InternalIsManyToMany(MemberInfo member)
        {
            if (InspectForCollection(member, false, c => c.Association == CollectionAssociationTypes.ManyToMany))
                return true;

            return false;
        }

        public bool IsOneToMany(MemberInfo member)
        {
            return InternalIsOneToMany(member);
        }

        internal static bool InternalIsOneToMany(MemberInfo member)
        {
            if (InspectForCollection(member, true, c => c.Association == CollectionAssociationTypes.OneToMany))
                return true;

            return false;
        }

        public bool IsManyToAny(MemberInfo member)
        {
            return InternalIsManyToAny(member);
        }

        internal static bool InternalIsManyToAny(MemberInfo member)
        {
            return false;
        }


        public bool IsAny(MemberInfo member)
        {
            return false;
        }

        internal static string GetIdentifierColumnName(Type type)
        {
            type.CheckArg("type");

            foreach (var property in type.GetProperties())
            {
                if (InternalIsPersistentId(property.CreateEntityPropertyInfo()))
                {
                    var mapping = property.GetAttribute<IdentifierMappingAttribute>();

                    if (mapping == null)
                        return "id";

                    return mapping.ColumnName;
                }
            }

            throw new SimpleNHException("Type {0} has no identifier".FormatWith(type.FullName));
        }

        internal static bool InternalIsPersistentId(MemberInfo member)
        {
            EntityPropertyInfo entityPropertyInfo = member as EntityPropertyInfo;

            if (entityPropertyInfo == null)
                throw new ArgumentException("member '{0}' on type '{1}' must be of type {2}".FormatWith(member.Name, member.DeclaringType, typeof(EntityPropertyInfo).Name));

            if (entityPropertyInfo.HasAttribute<IgnoreInspectionAttribute>())
                return false;

            if (entityPropertyInfo.HasAttribute<IdentifierMappingAttribute>())
                return true;

            if (entityPropertyInfo.EntityType != null)
            {
                if (entityPropertyInfo.EntityType.GetProperties().Any(x => x.HasAttribute<IdentifierMappingAttribute>()))
                    return false;
            }

            if (entityPropertyInfo.Name == "Id")
                return true;

            return false;
        }

        public bool IsPersistentId(MemberInfo member)
        {
            return InternalIsPersistentId(member);
        }

        public bool IsMemberOfComposedId(MemberInfo member)
        {
            return false;
        }

        public bool IsVersion(MemberInfo member)
        {
            return false;
        }

        public bool IsMemberOfNaturalId(MemberInfo member)
        {
            return false;
        }

        public bool IsPersistentProperty(MemberInfo member)
        {
            var property = member as EntityPropertyInfo;
            if (property == null)
                return false;

            return IsPersistentProperty(property);
        }

        internal static bool IsAssociation(MemberInfo member)
        {
            return InternalIsManyToOne(member) || InternalIsOneToOne(member);
        }

        internal static bool IsCollection(MemberInfo member)
        {
            return InternalIsBag(member) || InternalIsList(member) || InternalIsSet(member);
        }

        internal static bool IsPersistentProperty(EntityPropertyInfo property)
        {
            if (property.HasAttribute<ConcurrencyLockAttribute>())
                return false;

            if (property.HasAttribute<IgnoreInspectionAttribute>())
                return false;

            if (property.IsExplicitInterfaceImplementation())
                return false;

            bool hasBackingField = property.GetBackingField() != null;

            if (!hasBackingField && property.GetSetMethod(true) == null)
                return false;

            return !InternalIsPersistentId(property);
        }

        public bool IsSet(MemberInfo role)
        {
            return InternalIsSet(role);
        }

        internal static bool InternalIsSet(MemberInfo role)
        {
            if (InspectForCollection(role, false, c => c.CollectionType == CollectionTypes.Set))
                return true;

            return false;
        }

        public bool IsBag(MemberInfo role)
        {
            return InternalIsBag(role);
        }

        internal static bool InternalIsBag(MemberInfo role)
        {
            if (InspectForCollection(role, true, c => c.CollectionType == CollectionTypes.Bag))
                return true;

            return false;
        }

        public bool IsList(MemberInfo role)
        {
            return InternalIsList(role);
        }

        internal static bool InternalIsList(MemberInfo role)
        {
            if (InspectForCollection(role, false, c => c.CollectionType == CollectionTypes.List))
                return true;

            return false;
        }

        private static bool InspectForCollection(MemberInfo role, bool @default, Func<CollectionMappingAttribute, bool> comparison)
        {
            var isEnumerable = role.IsEnumerableDataEntity();

            if (!isEnumerable)
                return false;

            var attribute = role.GetAttribute<CollectionMappingAttribute>(false);

            if (attribute == null)
                return @default;

            return comparison == null ? @default : comparison(attribute);
        }

        public bool IsIdBag(MemberInfo role)
        {
            return false;
        }


        public bool IsArray(MemberInfo role)
        {
            return false;
        }

        public bool IsDictionary(MemberInfo role)
        {
            return false;
        }

        public bool IsProperty(MemberInfo member)
        {
            return InternalIsProperty(member);
        }

        internal static bool InternalIsProperty(MemberInfo member)
        {
            if (member.HasAttribute<TreatAsPropertyAttribute>())
                return true;

            if (!(member is PropertyInfo))
                return false;

            if (InternalIsPersistentId(member))
                return false;

            Type propertyType = member.ToEntityPropertyInfo().PropertyType;

            if (propertyType == typeof(byte[]))
                return true;

            if (InternalIsComponent(propertyType))
                return false;

            if (InternalIsManyToOne(member))
                return false;

            if (InternalIsOneToOne(member))
                return false;

            if (InternalIsManyToMany(member))
                return false;

            if (InternalIsManyToAny(member))
                return false;

            if (InternalIsList(member))
                return false;

            if (InternalIsSet(member))
                return false;

            if (InternalIsBag(member))
                return false;

            return true;
        }

        public bool IsDynamicComponent(MemberInfo member)
        {
            return false;
        }

        public Type GetDynamicComponentTemplate(MemberInfo member)
        {
            return null;
        }

        public IEnumerable<string> GetPropertiesSplits(Type type)
        {
            return Enumerable.Empty<string>();
        }
    }

    public class ConventionBasedMemberInspector : ICandidatePersistentMembersProvider
    {
        internal const BindingFlags SubClassPropertiesBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        internal const BindingFlags RootClassPropertiesBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        internal const BindingFlags ComponentPropertiesBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        internal const BindingFlags ClassFieldsBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        internal const BindingFlags FlattenHierarchyBindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public IEnumerable<MemberInfo> GetEntityMembersForPoid(Type entityClass)
        {
            var properties = entityClass.GetProperties();

            foreach (var property in properties)
            {
                if (property.HasAttribute<IdentifierMappingAttribute>())
                    return new[] { new EntityPropertyInfo(property, entityClass) };

                if (property.Name == "Id")
                    return new[] { new EntityPropertyInfo(property, entityClass) };
            }

            return Enumerable.Empty<MemberInfo>();
        }

        private static IEnumerable<MemberInfo> GetProperties(Type type)
        {
            var result = new Dictionary<string, EntityPropertyInfo>();

            if (type.IsClass)
            {
                if (type.IsAssignableTo(typeof(ITrackedEntity)))
                {
                    var allProperties = type.GetProperties();

                    AddPropertiesMatchingThatOf<TrackedEntity>(type, allProperties, result);
                }

                var hierarchy = LoadTopDownStack(type);

                foreach (var node in hierarchy)
                {
                    foreach (var property in node.GetPersistentProperties())
                    {
                        // if the property has to been ignored and is already added to the result, do not update the look up
                        if (!PropertySetToBeIgnored(result, property))
                            result[property.Name] = new EntityPropertyInfo(property, type);
                    }
                }
            }

            return result.Values;
        }

        private static void AddPropertiesMatchingThatOf<T>(Type type, PropertyInfo[] properties, Dictionary<string, EntityPropertyInfo> result)
        {
            // use the TrackedEntity type here instead of the interface so that can we respect IgnoreInspection where desired.
            var candidates = typeof(T).GetPersistentProperties();

            foreach (var candidate in candidates)
            {
                var match = properties.Single(x => x.Name == candidate.Name);
                result.Add(match.Name, new EntityPropertyInfo(match, type));
            }
        }

        private static bool PropertySetToBeIgnored(Dictionary<string, EntityPropertyInfo> result, PropertyInfo property)
        {
            return (result.ContainsKey(property.Name) && result[property.Name].HasAttribute<IgnoreInspectionAttribute>());
        }

        private static IEnumerable<Type> LoadTopDownStack(Type type)
        {
            var hierarchy = new Stack<Type>();

            hierarchy.Push(type);

            var analizing = type;

            while (analizing.BaseType != null && analizing.BaseType != typeof(object))
            {
                hierarchy.Push(analizing.BaseType);
                analizing = analizing.BaseType;
            }

            return hierarchy;
        }

        public IEnumerable<MemberInfo> GetRootEntityMembers(Type entityClass)
        {
            return GetProperties(entityClass);
        }

        public IEnumerable<MemberInfo> GetSubEntityMembers(Type entityClass, Type entitySuperclass)
        {
            return entityClass.GetPersistentProperties();
        }

        public IEnumerable<MemberInfo> GetComponentMembers(Type componentClass)
        {
            return GetProperties(componentClass);
        }
    }

}
