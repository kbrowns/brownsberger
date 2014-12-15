using System;

namespace Simple.NH.Querying
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AttachFilterAttribute : Attribute
    {
        public AttachFilterAttribute(Type filterType)
        {
            filterType.CheckArg("filterType");
            FilterType = filterType;
        }

        public Type FilterType { get; private set; }
    }
}