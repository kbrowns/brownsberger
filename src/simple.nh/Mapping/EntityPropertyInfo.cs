using System;
using System.Globalization;
using System.Reflection;

namespace Simple.NH.Mapping
{
    /// <summary>
    /// The purpose of this class is to augment the PropertyInfo object with an additional EntityType property
    /// that will hold the entity type (potentially a subclass) of the declaring type.  Without this the automapping engine
    /// doesn't have an enough information to property create foreignkey names in the FK_[primary-table]_[foreign-key] format
    /// as the DeclaredType could be a base class where the concrete derivative type is what is needed.
    /// This class is constructed ConventionBasedMemberInspector which is a hook provided by NHibernate that
    /// allows us to control the property lookup for a type.  There we have the necessary information to construct this with the
    /// entity type
    /// </summary>
    public class EntityPropertyInfo : PropertyInfo
    {
        private readonly PropertyInfo _innerPropertyInfo;
        private readonly Type _entityType;

        public EntityPropertyInfo(EntityPropertyInfo entityPropertyInfo, Type entityType)
        {
            entityPropertyInfo.CheckArg("entityPropertyInfo");
            _innerPropertyInfo = entityPropertyInfo._innerPropertyInfo;
            _entityType = entityType;
        }

        public EntityPropertyInfo(PropertyInfo innerPropertyInfo, Type entityType)
        {
            innerPropertyInfo.CheckArg("innerPropertyInfo");
            _innerPropertyInfo = innerPropertyInfo;
            _entityType = entityType;
        }

        public Type EntityType { get { return _entityType; } }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _innerPropertyInfo.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _innerPropertyInfo.IsDefined(attributeType, inherit);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            return _innerPropertyInfo.GetValue(obj, invokeAttr, binder, index, culture);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            _innerPropertyInfo.SetValue(obj, value, invokeAttr, binder, index, culture);
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return _innerPropertyInfo.GetAccessors(nonPublic);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return _innerPropertyInfo.GetGetMethod(nonPublic);
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return _innerPropertyInfo.GetSetMethod(nonPublic);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return _innerPropertyInfo.GetIndexParameters();
        }

        public override string Name
        {
            get { return _innerPropertyInfo.Name; }
        }

        public override Type DeclaringType
        {
            get { return _innerPropertyInfo.DeclaringType; }
        }

        public override Type ReflectedType
        {
            get { return _innerPropertyInfo.ReflectedType; }
        }

        public override Type PropertyType
        {
            get { return _innerPropertyInfo.PropertyType; }
        }

        public override PropertyAttributes Attributes
        {
            get { return _innerPropertyInfo.Attributes; }
        }

        public override bool CanRead
        {
            get { return _innerPropertyInfo.CanRead; }
        }

        public override bool CanWrite
        {
            get { return _innerPropertyInfo.CanWrite; }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _innerPropertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override bool Equals(object obj)
        {
            EntityPropertyInfo property = obj as EntityPropertyInfo;

            if (property == null)
                return false;

            return _innerPropertyInfo.Equals(property._innerPropertyInfo);
        }

        protected bool Equals(EntityPropertyInfo other)
        {
            return Equals(_innerPropertyInfo, other._innerPropertyInfo);
        }

        public override int GetHashCode()
        {
            return _innerPropertyInfo.GetHashCode();
        }
    }
}