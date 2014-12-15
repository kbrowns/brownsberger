namespace Simple.NH
{
    /// <summary>
    /// Represents the supported instance policy for creating persistent sessions
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
    public struct ContextInstancePolicy
    {
        private readonly string _value;

        /// <summary>
        /// Constructor requiring the value to be passed to NHibernate for it's current_session_context_class property.  
        /// The following values are supported natively by NHibernate, but this can also be a fully qualified type 
        /// name that NHibernate will construct with reflection.
        /// 
        /// Known values:
        /// * thread_static
        /// * managed_web
        /// * wcf_operation
        /// 
        /// Note, if a type name is specified, NHibernate requires that a constructor accepting ISessionFactoryImplementor as an
        /// argument is defined.
        /// </summary>
        /// <param name="value"></param>
        public ContextInstancePolicy(string value)
        {
            _value = value;
        }

        /// <summary>
        /// This is the same as thread_static but segmented by session factory.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public static readonly ContextInstancePolicy isolated_thread_static = new ContextInstancePolicy(typeof(IsolatedThreadStaticSessionContext).AssemblyQualifiedName);
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Denotes one instance per Http Request as dictated by HttpContext.  This is deprecated going forward in favor of "web"
        /// Future version of NHibernate do not support this.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public static readonly ContextInstancePolicy managed_web = new ContextInstancePolicy("managed_web"); // it's vital this is kept to match the NHibernate setting name
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Denotes one instance per Http Request as dictated by HttpContext.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public static readonly ContextInstancePolicy web = new ContextInstancePolicy("web"); // it's vital this is kept to match the NHibernate setting name
        // ReSharper restore InconsistentNaming

        /// <summary>
        /// Denotes one instance per WCF operation as dictated by OperationContext
        /// </summary>
// ReSharper disable InconsistentNaming
        public static readonly ContextInstancePolicy wcf_operation = new ContextInstancePolicy("wcf_operation"); // it's vital this is kept to match the NHibernate setting name
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Will return the specified property value that was provided in the constructor.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _value;
        }
    }
}