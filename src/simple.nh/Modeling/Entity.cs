using System;

namespace Simple.NH.Modeling
{
    /// <summary>
    /// Base class for persisted entities.  The generic parameter defines the type of the entity's identifier.
    /// </summary>
    [Serializable]
    public abstract class Entity<TId> : IEntity
    {
        /// <summary>
        /// Unique identifier for an entity.
        /// </summary>
        public virtual TId Id { get; set; }

        /// <summary>
        /// Optional reference identifier for downstream integration scenarios where this entity is stored in multiple places.
        /// </summary>
        public virtual string ReferenceId { get; set; }

        /// <summary>
        /// Optional logical key intended to be a concatenation of other members that make up the natural key of the entity.
        /// </summary>
        /// <returns></returns>
        public virtual string GetLogicalKey()
        {
            return null;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Entity<TId>);
        }

        /// <summary>
        /// Helper property to determine if this instance has been persisted yet.
        /// </summary>
        public virtual bool IsNew()
        {
            return Equals(Id, default(TId));
        }

        private static bool IsTransient(Entity<TId> obj)
        {
            return obj != null && obj.IsNew();
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        /// <summary>
        /// Determines whether two entity instances are equal.
        /// </summary>
        /// <param name="other">The entity to compare with the current entity.</param>
        /// <returns>true if the specified entity is equal to the current entity; otherwise, false. </returns>
        public virtual bool Equals(Entity<TId> other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) && !IsTransient(other) && Equals(Id, other.Id))
            {
                var thisType = GetUnproxiedType();
                var otherType = other.GetUnproxiedType();

                return thisType.IsAssignableFrom(otherType) || otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return IsNew() ? base.GetHashCode() : Id.GetHashCode();
        }
    }
}