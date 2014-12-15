using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Type;
using Simple.NH.Modeling;

namespace Simple.NH.Querying
{
    public struct FilterName
    {
        private readonly string _name;

        public FilterName(Type type)
        {
            _name = type.CheckArg("type").FullName.Replace(".", "");
        }

        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var filter = (FilterName)obj;

            return filter._name.Equals(_name);
        }

        public override string ToString()
        {
            return _name;
        }

        public static implicit operator string(FilterName filterName)
        {
            return filterName.ToString();
        }
    }

    public abstract class Filter
    {
        private readonly IDictionary<string, IType> _parameterTypes = new Dictionary<string, IType>();

        protected Filter()
        {
            this.Name = new FilterName(this.GetType());
        }

        public FilterName Name { get; private set; }

        public string DefaultCondition { get { return null; } }

        public bool UseOnManyToOne { get { return true; } }

        public abstract string GetCondition();

        public abstract void LoadParameters(IFilter filter);

        public virtual IDictionary<string, IType> GetParameterTypes()
        {
            return _parameterTypes;
        }

        protected void AddParameterType(string name, IType type)
        {
            if (!_parameterTypes.ContainsKey(name))
                _parameterTypes.Add(name, type);
        }
    }

    public class SoftDeleteFilter : Filter
    {
        public SoftDeleteFilter()
        {
            AddParameterType("is_active", NHibernateUtil.Boolean);
        }

        public override string GetCondition()
        {
            var column = For.Type<TrackedEntity>().GetProperty(x => x.IsActive).GetColumnName();

            return string.Format("({0} IS NULL OR {0} = :is_active)", column);
        }

        public override void LoadParameters(IFilter filter)
        {
            filter.SetParameter("is_active", true);
        }
    }
}
