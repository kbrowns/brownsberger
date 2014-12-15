using System;

namespace Simple.NH.Mapping
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ForeignKeyAliasAttribute : Attribute
    {
        private readonly string _alias;

        public ForeignKeyAliasAttribute(string @alias)
        {
            _alias = alias;
        }

        public string Alias
        {
            get { return _alias; }
        }
    }
}