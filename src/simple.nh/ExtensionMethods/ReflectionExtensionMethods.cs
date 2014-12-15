using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using Simple.NH.Exceptions;
using Simple.NH.Mapping;
using Simple.NH.Modeling;
using IPropertyMapping = Simple.NH.Mapping.IPropertyMapping;

public static class ReflectionExtensionMethods
{
    public static IEnumerable<T> GetAttributes<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttributes<T>(true);
    }

    public static IEnumerable<T> GetAttributes<T>(this Type type, bool inherit) where T : Attribute
    {
        return type.GetCustomAttributes<T>(inherit);
    }

    public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
    {
        return member.HasAttribute<T>(true);
    }

    public static bool HasAttribute<T>(this MemberInfo member, bool inherit) where T : Attribute
    {
        return member.GetAttribute<T>(inherit) != null;
    }
    public static T GetAttribute<T>(this MemberInfo member) where T : Attribute
    {
        return member.GetAttribute<T>(true);
    }

    public static T GetAttribute<T>(this MemberInfo member, bool inherit) where T : Attribute
    {
        if (member == null)
            return null;

        var attributes = member.GetCustomAttributes(typeof(T), inherit);

        if (attributes.Length > 0)
            return (T)attributes[0];

        return null;
    }

    public static T GetAttribute<T>(this Type type) where T : Attribute
    {
        return GetAttribute<T>(type, true);
    }

    public static T GetAttribute<T>(this Type type, bool inherit) where T : Attribute
    {
        if (type == null)
            return null;

        var attributes = type.GetCustomAttributes(typeof(T), inherit);

        if (attributes.Length > 0)
            return (T)attributes[0];

        return null;


    }

    public static bool IsString(this Type type)
    {
        return type == typeof(string);
    }

    public static bool IsConcreteNonStringClass(this Type type)
    {
        if (type == null)
            return false;

        return type.IsClass && !type.IsAbstract && !type.IsString() && !type.IsArray;
    }

    public static bool IsNonStringClass(this Type type)
    {
        if (type == null)
            return false;

        return type.IsClass && !type.IsString();
    }

    public static T CreateInstanceAs<T>(this Type type) where T : class
    {
        try
        {
            var instance = Activator.CreateInstance(type);

            T casted = instance as T;

            if (casted == null)
                throw new ApplicationException("Was able to construct type {0}, but it does not implement the expected interface or base class {1}.".FormatWith(type, typeof(T)));

            return casted;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to construct a valid object from type {0}.  See the inner exception for details.".FormatWith(type), ex);
        }
    }

    public static bool IsEnumerable(this Type type)
    {
        return (typeof(IEnumerable).IsAssignableFrom(type));
    }

    public static bool IsCollection(this Type type)
    {
        return (typeof(ICollection).IsAssignableFrom(type));
    }

    public static string GetFriendlyName(this Type type, bool displayFullInnerNames = false)
    {
        if (type == null)
            return null;

        if (type.IsGenericType)
        {
            return BuildGenericFriendlyName(type.Name, type.GetGenericArguments(), displayFullInnerNames);
        }

        return type.Name;
    }

    public static string GetFriendlyFullName(this Type type, bool displayFullInnerNames = false)
    {
        if (type == null)
            return null;

        if (type.IsGenericType)
        {
            return BuildGenericFriendlyName(type.FullName, type.GetGenericArguments(), displayFullInnerNames);
        }

        return type.FullName + ", " + type.Assembly.GetName().Name;
    }

    private static string BuildGenericFriendlyName(string name, Type[] args, bool displayFullInnerNames)
    {
        StringBuilder buffer = new StringBuilder();
        buffer.Append(name.Substring(0, name.IndexOf("`", StringComparison.Ordinal)));
        buffer.Append("<");


        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0) buffer.Append(", ");

            if (displayFullInnerNames)
                buffer.Append(args[i].FullName);
            else
                buffer.Append(args[i].Name);
        }

        buffer.Append(">");

        return buffer.ToString();
    }

    public static Type GetEnumeratorType(this Type type)
    {
        if (!typeof(IEnumerable).IsAssignableFrom(type))
            throw new ArgumentException("Invalid GetEnumeratorType() usage - {0} must implement interface {1}.".FormatWith(type.FullName, typeof(IEnumerable).FullName));

        if (type.IsArray)
            return type.GetElementType();

        Type currentType = type;

        while (currentType != null && currentType != typeof(object) && typeof(IEnumerable).IsAssignableFrom(currentType))
        {
            if (currentType.IsGenericType)
            {
                Type[] genericArguments = currentType.GetGenericArguments();

                if (genericArguments.Length == 1)
                    return genericArguments[0];
            }

            currentType = currentType.BaseType;
        }

        return typeof(object);
    }

    public static Type GetMemberType(this MemberInfo member)
    {
        PropertyInfo property = member as PropertyInfo;

        if (property != null)
            return property.PropertyType;

        FieldInfo field = member as FieldInfo;

        if (field != null)
            return field.FieldType;

        throw new ApplicationException(String.Format("Member mapping is only supported for fields or properties. '{0}' declared on '{1}' is an unrecognized MemberInfo type.", member.GetType(), member.DeclaringType));
    }

    public static bool IsConcreteClass(this Type type)
    {
        if (type == null)
            return false;

        return type.IsClass && !type.IsAbstract;
    }

    public static string SimpleAssemblyQualifiedName(this Type type)
    {
        if (type == null)
            return null;

        return String.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
    }

    public static bool IsNullableDataType(this PropertyInfo property)
    {
        return property.PropertyType.IsNullableDataType();
    }

    public static bool IsNullableDataType(this FieldInfo field)
    {
        return field.FieldType.IsNullableDataType();
    }

    public static bool IsNullableDataType(this Type type)
    {
        if (type.IsEnum)
            return type.HasZeroValue();

        if (type.IsClass)
            return true;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return true;

        return false;
    }

    public static bool HasZeroValue(this Type type)
    {
        var values = Enum.GetValues(type);

        if (values.Cast<object>().Any(value => 0 == (int)value))
        {
            return true;
        }

        return false;
    }

    public static bool IsOfType<T>(this PropertyInfo property)
    {
        if (property.PropertyType == typeof(T))
            return true;

        return false;
    }

    public static bool IsOfType<T>(this PropertyInfo property, bool includeNullables) where T : struct
    {
        if (property.PropertyType == typeof(T))
            return true;

        if (includeNullables && property.PropertyType == typeof(T?))
            return true;

        return false;
    }

    public static bool IsOfType<T>(this FieldInfo field)
    {
        if (field.FieldType == typeof(T))
            return true;

        return false;
    }

    public static bool IsOfType<T>(this FieldInfo field, bool includeNullables) where T : struct
    {
        if (field.FieldType == typeof(T))
            return true;

        if (includeNullables && field.FieldType == typeof(T?))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a type can be assigned to the destination generic type definition
    /// </summary>
    public static bool IsAssignableTo(this Type sourceType, Type genericDestinationType)
    {
        return sourceType.MakeClosedGenericType(genericDestinationType) != null;
    }

    /// <summary>
    /// Checks if a type can be assigned to the destination generic type definition
    /// </summary>
    public static Type MakeClosedGenericType(this Type sourceType, Type genericDestinationType)
    {
        if (genericDestinationType.IsAssignableFrom(sourceType))
            return genericDestinationType;

        // Handle generic types as the destination, e.g. is List<int> assignable to List<>
        var interfaceTypes = sourceType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericDestinationType)
                return it;
        }

        if (sourceType.IsGenericType && sourceType.GetGenericTypeDefinition() == genericDestinationType)
            return sourceType;

        Type baseType = sourceType.BaseType;
        if (baseType == null)
            return null;

        return MakeClosedGenericType(baseType, genericDestinationType);
    }

    public static bool IsExplicitInterfaceImplementation(this EntityPropertyInfo property)
    {
        // At least one accessor must exists, I arbitrary check first for
        // "get" one. Note that in Managed C++ (not C++ CLI) these methods
        // are logically separated so they may follow different rules (one of them
        // is explicit and the other one is not). It's a pretty corner case
        // so we may just ignore it.
        if (property.GetGetMethod(true) != null)
            return IsExplicitInterfaceImplementation(property.GetGetMethod(true), property.EntityType);

        return IsExplicitInterfaceImplementation(property.GetSetMethod(true), property.EntityType);
    }

    public static bool IsExplicitInterfaceImplementation(this MemberInfo member)
    {
        var property = member as EntityPropertyInfo;
        if (property != null)
            return property.IsExplicitInterfaceImplementation();

        var method = member as MethodInfo;
        if (method != null)
            return IsExplicitInterfaceImplementation((MemberInfo)method);

        return false;
    }

    public static bool IsExplicitInterfaceImplementation(this MethodInfo method, Type declaringType = null)
    {
        // see http://stackoverflow.com/a/17854048/808818

        if (method == null)
            return false;

        if (declaringType == null)
        {
            if (method.DeclaringType == null)
                return false;

            // ReSharper disable once PossibleMistakenCallToGetType.2
            declaringType = method.DeclaringType.GetType();
        }

        // Check all interfaces implemented in the type that declares
        // the method we want to check, with this we'll exclude all methods
        // that don't implement an interface method
        foreach (var implementedInterface in declaringType.GetInterfaces())
        {
            var mapping = declaringType.GetInterfaceMap(implementedInterface);

            // If interface isn't implemented in the type that owns
            // this method then we can ignore it (for sure it's not
            // an explicit implementation)
            if (mapping.TargetType != declaringType)
                continue;

            // Is this method the implementation of this interface?
            int methodIndex = Array.IndexOf(mapping.TargetMethods, method);
            if (methodIndex == -1)
                continue;

            // Is it true for any language? Can we just skip this check?
            if (!method.IsFinal || !method.IsVirtual)
                return false;

            // It's not required in all languages to implement every method
            // in the interface (if the type is abstract)
            string methodName = "";
            if (mapping.InterfaceMethods[methodIndex] != null)
                methodName = mapping.InterfaceMethods[methodIndex].Name;

            // If names don't match then it's explicit
            if (!method.Name.Equals(methodName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static FieldInfo GetBackingField(this EntityPropertyInfo property)
    {
        if (property.EntityType == null)
            return null;

        string fieldName = String.Format("_{0}{1}", property.Name[0].ToString().ToLower(), property.Name.Substring(1));
        FieldInfo field = property.EntityType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        return field;
    }

    public static string GetColumnName(this PropertyInfo property)
    {
        var mapping = new EntityPropertyInfo(property, property.DeclaringType).GetPropertyMapping();
        return mapping.ColumnName;
    }

    public static EntityPropertyInfo CreateEntityPropertyInfo(this MemberInfo member)
    {
        return CreateEntityPropertyInfo(member, null);
    }

    public static EntityPropertyInfo CreateEntityPropertyInfo(this MemberInfo member, EntityType entityType)
    {
        PropertyInfo property = member as PropertyInfo;

        if (property == null)
            throw new ArgumentException("member must be a PropertyInfo");

        return CreateEntityPropertyInfo(property, entityType);
    }

    public static EntityPropertyInfo CreateEntityPropertyInfo(this PropertyInfo property)
    {
        return CreateEntityPropertyInfo(property, null);
    }

    public static EntityPropertyInfo CreateEntityPropertyInfo(this PropertyInfo property, Type entityType)
    {
        return new EntityPropertyInfo(property, entityType ?? property.DeclaringType);
    }

    public static EntityPropertyInfo ToEntityPropertyInfo(this MemberInfo member)
    {
        EntityPropertyInfo entityPropertyInfo = member as EntityPropertyInfo;

        if (entityPropertyInfo == null)
            throw new SimpleNHException(String.Format("Only entity property members are supported for auto mapping.  Type {0} it attempting to auto map a non property member: {1}.  Ensure you're using the auto mapping API's in the prescribed way.", member.DeclaringType.FullName, member.Name));

        return entityPropertyInfo;
    }

    public static int GetOrderAttributeValue(this MemberInfo member, int defaultValue = -1)
    {
        if (member == null)
            return defaultValue;

        var order = member.GetAttribute<OrderAttribute>();

        if (order == null)
            return defaultValue;

        return order.Order;
    }

    private const BindingFlags PersistentPropertiesBindingFlag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    public static IEnumerable<EntityPropertyInfo> GetPersistentProperties(this Type type, bool includeId = false)
    {
        return type.GetProperties(PersistentPropertiesBindingFlag)
                    .Where(x => NotIgnoredAndNotId(includeId, x))
                    .OrderBy(GetOrdinal)
                    .Select(x => new EntityPropertyInfo(x, type));
    }

    private static int GetOrdinal(PropertyInfo x)
    {
        var order = x.GetAttribute<OrderAttribute>(true);

        return order != null ? order.Order : Int32.MaxValue;
    }

    private static bool NotIgnoredAndNotId(bool includeId, PropertyInfo x)
    {
        if (x.HasAttribute<IgnoreInspectionAttribute>())
            return false;

        if (!includeId && x.Name == "Id")
            return false;

        return true;
    }

    public static TEnumeration[] GetEnumerations<TEnumeration>(this Type enumerationType)
    {
        return enumerationType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                              .Where(info => enumerationType.IsAssignableFrom(info.FieldType))
                              .Select(info => (TEnumeration)info.GetValue(null))
                              .ToArray();
    }

    public static IEnumerable<PropertyInfo> GetEntityProperties(this Type type)
    {
        return type.GetProperties().Where(x => !x.HasAttribute<IgnoreInspectionAttribute>()).Select(x => new EntityPropertyInfo(x, type));
    }

    public static ICollectionMapping GetCollectionMapping(this EntityPropertyInfo property, PropertyPath member, IModelInspector inspector)
    {
        CollectionMappingAttribute specifiedMapping = property.GetAttribute<CollectionMappingAttribute>(false);
        DefaultCollectionMapping defaultMapping = new DefaultCollectionMapping(property, member, inspector);
        ICollectionMapping mapping;

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
    public static IAssociationMapping GetAssociationMapping(this EntityPropertyInfo property)
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

    public static object GetDefault(this Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }

    public static bool HasDefaultConstructor(this Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) != null;
    }

    public static IPropertyMapping GetPropertyMapping(this EntityPropertyInfo propertyInfo)
    {
        PropertyMappingAttribute specifiedMapping = propertyInfo.GetAttribute<PropertyMappingAttribute>(false);
        IPropertyMapping defaultMapping = new DefaultPropertyMapping(propertyInfo);
        IPropertyMapping mapping;

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

    public static IClassMapping GetClassMapping(this Type type)
    {
        var classMappingAttributes = type.GetCustomAttributes(false).OfType<IClassMapping>().ToArray();

        if (classMappingAttributes.Length > 1)
            throw new SimpleNHException("Multiple attributes implementing interface {0} were found on type.  Optional attributes of this nature can only occur once.".FormatWith(typeof(IClassMapping), type));

        IClassMapping specifiedMapping = classMappingAttributes.Length == 0 ? null : classMappingAttributes[0];
        var defaultMapping = new DefaultClassMapping(type);

        if (specifiedMapping == null)
            specifiedMapping = defaultMapping;
        else
            specifiedMapping.Initialize(defaultMapping);

        return specifiedMapping;
    }

    public static bool TryGetEnumMemberAttributeOf<T>(this Type type, string name, out T attribute) where T : Attribute
    {
        if (!type.IsEnum)
            throw new ArgumentException("Argument type must be an enum, but {0} is not an enumeration.".FormatWith(type.FullName));

        MemberInfo[] members = type.GetMember(name);

        if (members.Length == 1)
        {
            attribute = members[0].GetAttribute<T>(false);

            return attribute != null;
        }

        attribute = null;
        return false;
    }

    public static bool IsEnumerableDataEntity(this MemberInfo member)
    {
        var type = member.GetMemberType();

        if (!type.IsEnumerable())
            return false;

        return type.GetEnumeratorType().IsDataEntity();
    }

    public static bool IsDataEntity(this MemberInfo member)
    {
        return member.GetMemberType().IsDataEntity();
    }

    public static bool IsDataComponent(this Type type)
    {
        return !type.IsDataEntity() && type.IsConcreteNonStringClass();
    }

    public static bool IsReferenceEntity(this Type type)
    {
        return type.IsDataEntity() && typeof(IReferenceEntity).IsAssignableFrom(type);
    }

    public static bool IsDataEntity(this Type type)
    {
        if (type == null)
            return false;

        if (!type.IsNonStringClass())
            return false;

        if (!(typeof(IDomainEntity).IsAssignableFrom(type) || typeof(IReferenceEntity).IsAssignableFrom(type)))
            return false;

        if (type.IsAbstract && type.HasAttribute<InheritanceRootAttribute>())
            return true;

        return !type.IsAbstract;
    }
}