using System;

namespace Simple.NH.Mapping
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class InheritanceRootAttribute : Attribute
    {
        public InheritanceRootAttribute(InheritanceMappingSchemes scheme)
        {
            Scheme = scheme;
            DiscriminatorColumn = "discriminator";
            DiscriminatorColumnType = DiscriminatorColumnTypes.AnsiString;
        }

        public string DiscriminatorColumn { get; set; }

        public InheritanceMappingSchemes Scheme { get; set; }

        public string TableNameFormatExpression { get; set; }

        public DiscriminatorColumnTypes DiscriminatorColumnType { get; set; }
    }
}