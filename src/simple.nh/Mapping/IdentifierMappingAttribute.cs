using System;

namespace Simple.NH.Mapping
{
    public sealed class IdentifierMappingAttribute : Attribute
    {
        public IdentifierMappingAttribute()
        {
        }

        public IdentifierMappingAttribute(string columnName)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; set; }
    }
}