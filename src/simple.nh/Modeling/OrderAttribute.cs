using System;
using System.Runtime.CompilerServices;

namespace Simple.NH.Modeling
{
    /// <summary>
    /// This entire attribute is a trick to achieve order of declaration 
    /// of properties while using reflection.  The [CallerLineNumber] declaration 
    /// will assign the order argument at compile time and provide something we can 
    /// sort by.  You should be specifying order explicitly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class OrderAttribute : Attribute
    {
        private readonly int _order;

        public OrderAttribute([CallerLineNumber]int order = 0)
        {
            _order = order;
        }

        public int Order { get { return _order; } }
    }
}