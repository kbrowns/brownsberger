using System;
using System.Reflection;
using NHibernate;
using NHibernate.Type;
using Simple.NH.ExtensionMethods;

namespace Simple.NH.Mapping
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyMappingAttribute : Attribute, IPropertyMapping
    {
        private int _length;
        private bool _lengthSpecified;
        private IType _propertyTypeInstance;
        private bool _isNullable;
        private bool _isNullableSpecified;
        private bool _isUnique;
        private bool _isUniqueSpecified;
        private bool _componentColumnNameSpecified;
        private string _componentColumnPrefix;

        public string ColumnName { get; set; }

        public bool IsNullable
        {
            get { return _isNullable; }
            set
            {
                _isNullable = value;
                _isNullableSpecified = true;
            }
        }

        public bool IsUnique
        {
            get { return _isUnique; }
            set
            {
                _isUnique = value;
                _isUniqueSpecified = true;
            }
        }

        public object Default { get; set; }

        public bool LengthSpecified
        {
            get { return _lengthSpecified; }
        }

        public int Length
        {
            get { return _length; }
            set
            {
                _lengthSpecified = true;
                _length = value;
            }
        }

        public bool IgnoreOnUpdate { get; set; }

        public IType GetPropertyType()
        {
            if (_propertyTypeInstance == null)
            {
                if (PropertyType != null)
                {
                    if (!typeof(IType).IsAssignableFrom(PropertyType) || !PropertyType.HasDefaultConstructor())
                        throw new ArgumentException("{0} is either not assignable to {1} or does not have a default constructor.".FormatWith(PropertyType, typeof(IType)));

                    _propertyTypeInstance = (IType)Activator.CreateInstance(PropertyType);
                }
            }

            return _propertyTypeInstance;
        }

        public Type PropertyType { get; set; }

        public string ComponentColumnPrefix
        {
            get { return _componentColumnPrefix; }
            set
            {
                _componentColumnPrefix = value;
                _componentColumnNameSpecified = true;
            }
        }

        public void FillDefaults(IPropertyMapping defaults)
        {
            if (ColumnName.IsNullOrEmpty())
                ColumnName = defaults.ColumnName;

            if (!_isUniqueSpecified)
                IsUnique = defaults.IsUnique;

            if (!_isNullableSpecified)
                IsNullable = defaults.IsNullable;

            if (defaults.LengthSpecified && !LengthSpecified)
                Length = defaults.Length;

            if (PropertyType == null)
                PropertyType = defaults.PropertyType;

            if (_propertyTypeInstance == null)
                _propertyTypeInstance = defaults.GetPropertyType();

            if (Default == null)
                Default = defaults.Default;

            if (!_componentColumnNameSpecified)
                ComponentColumnPrefix = defaults.ComponentColumnPrefix;
        }
    }

    internal class DefaultPropertyMapping : IPropertyMapping
    {
        private int _length;
        private readonly IType _propertyTypeInstance;
        private readonly Type _propertyType;

        public DefaultPropertyMapping(PropertyInfo property)
        {
            ColumnName = property.Name.ToDbSchemaName();
            IsNullable = CalculateIsNullable(property);

            if (property.PropertyType.IsString())
                Length = 100;

            if (property.PropertyType.IsString())
            {
                _propertyTypeInstance = NHibernateUtil.AnsiString;
                _propertyType = NHibernateUtil.AnsiString.GetType();
            }

            if (property.PropertyType.IsDataComponent())
            {
                ComponentColumnPrefix = "{0}_".FormatWith(ColumnName);
            }
        }

        internal static bool CalculateIsNullable(PropertyInfo propertyInfo)
        {
            bool required = propertyInfo.PropertyType.IsValueType;

            if (propertyInfo.PropertyType.IsNullableDataType())
                required = false;

            return !required;
        }

        public string ColumnName { get; private set; }

        public string ComponentColumnPrefix { get; set; }

        public bool IsNullable { get; private set; }

        public bool LengthSpecified { get; private set; }

        public object Default { get { return null; } }

        public int Length
        {
            get { return _length; }
            set
            {
                LengthSpecified = true;
                _length = value;
            }
        }

        Type IPropertyMapping.PropertyType { get { return _propertyType; } }

        bool IPropertyMapping.IsUnique { get { return false; } }

        public bool IgnoreOnUpdate { get { return false; } }

        public IType GetPropertyType()
        {
            return _propertyTypeInstance;
        }
    }

    public interface IPropertyMapping
    {
        string ColumnName { get; }
        bool IsNullable { get; }
        bool LengthSpecified { get; }
        int Length { get; }
        Type PropertyType { get; }
        bool IsUnique { get; }
        object Default { get; }
        IType GetPropertyType();
        bool IgnoreOnUpdate { get; }
        string ComponentColumnPrefix { get; set; }
    }
}
