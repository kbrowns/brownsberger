using NHibernate.Engine;

namespace Simple.NH.Querying
{
    internal class BridgeFilterDefinition : FilterDefinition
    {
        public BridgeFilterDefinition(Filter definition)
            : base(definition.Name.ToString(), definition.DefaultCondition, definition.GetParameterTypes(), definition.UseOnManyToOne)
        {
            this.Definition = definition;
        }

        internal Filter Definition { get; private set; }
    }
}