using System;

namespace Simple.NH.Querying
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DetachFilterAttribute : Attribute
    {
        public DetachFilterAttribute(Type filterType)
        {
            filterType.CheckArg("filterType");
            FilterType = filterType;
        }

        public Type FilterType { get; private set; }
    }
}