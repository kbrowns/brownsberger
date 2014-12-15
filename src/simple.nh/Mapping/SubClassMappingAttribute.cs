using System;

namespace Simple.NH.Mapping
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SubClassMappingAttribute : Attribute, ISubClassMapping
    {
        private object _discriminatorValue;

        public object DiscriminatorValue
        {
            get { return _discriminatorValue; }
            set
            {
                _discriminatorValue = value;
                DiscriminatorValueSpecified = true;
            }
        }

        internal bool DiscriminatorValueSpecified { get; set; }

        internal void FillDefaults(DefaultSubClassMapping defaults)
        {
            if (!this.DiscriminatorValueSpecified)
                this.DiscriminatorValue = defaults.DiscriminatorValue;
        }
    }

    public class DefaultSubClassMapping : ISubClassMapping
    {
        public DefaultSubClassMapping(Type type)
        {
            DiscriminatorValue = type.Name;
        }

        public object DiscriminatorValue { get; set; }
    }

    public interface ISubClassMapping
    {
        object DiscriminatorValue { get; set; }
    }
}
