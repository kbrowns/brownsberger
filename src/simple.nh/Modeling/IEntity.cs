namespace Simple.NH.Modeling
{
    /// <summary>
    /// Base class for persisted entities.  The generic parameter defines the type of the entity's identifier.
    /// 
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Helper property to determine if this instance has been persisted yet.
        /// 
        /// </summary>
        bool IsNew();
    }
}