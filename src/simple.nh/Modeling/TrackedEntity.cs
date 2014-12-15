using System;
using Simple.NH.Mapping;
using Simple.NH.Querying;

namespace Simple.NH.Modeling
{
    [Serializable]
    [AttachFilter(typeof(SoftDeleteFilter))]
    public abstract class TrackedEntity : ITrackedEntity, IComparable<TrackedEntity>, IEquatable<TrackedEntity>
    {
        private DateTimeOffset _rowUpdated;

        protected TrackedEntity()
        {
            Id = GenerateNewId();

            // this is just to get past the nullable checks on save
            // these will be overwritten by the events on pre-insert|update
            IsActive = true;
            RowCreated = Now.Current.GetOffsetNow();
            RowCreatedUser = 0;
            RowUpdatedUser = 0;
        }

        private static long GenerateNewId()
        {
            return -Math.Abs(Guid.NewGuid().GetHashCode());
        }

        /// <summary>
        /// Identifier for the entity
        /// </summary>
        [Order]
        [PropertyMapping(IsNullable = false)]
        public virtual long Id { get; set; }

        /// <summary>
        /// The indicator of soft deletes.  False indicates the RootEntitySpecification is not active.  True indictates it is active.
        /// </summary>
        [Order]
        [PropertyMapping(IsNullable = false)]
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// The timestamp captured when the RootEntitySpecification was first created.
        /// </summary>
        [Order]
        [PropertyMapping(IsNullable = false, IgnoreOnUpdate = true)]
        public virtual DateTimeOffset RowCreated { get; protected set; }

        /// <summary>
        /// The identifier of the user responsible for creating the RootEntitySpecification.
        /// </summary>
        [PropertyMapping(IsNullable = false, IgnoreOnUpdate = true)]
        [Order]
        public virtual int RowCreatedUser { get; protected set; }

        /// <summary>
        /// The timestamp captured when the RootEntitySpecification is updated.  This field is only populated if it has been modified.
        /// </summary>
        [Order]
        public virtual DateTimeOffset RowUpdated
        {
            get { return _rowUpdated; }
            protected set
            {
                _rowUpdated = value;
            }
        }

        [ConcurrencyLock]
        [PropertyMapping(IsNullable = false)]
        public virtual int Version { get; protected set; }

        /// <summary>
        /// The identifier of the user responsible for modifying the RootEntitySpecification.
        /// </summary>
        [Order]
        public virtual int RowUpdatedUser { get; protected set; }

        /// <summary>
        /// Determines if object RootEntitySpecification has been persisted or not.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsNew()
        {
            return Id < 1;
        }

        protected virtual string GetReferenceId()
        {
            return null;
        }

        void ITrackedEntity.SetCreated(DateTimeOffset timestamp, int user, SetPropertiesHandler setProperties)
        {
            RowCreated = timestamp;
            RowCreatedUser = user;
            RowUpdated = timestamp;
            RowUpdatedUser = user;

            if (setProperties != null)
                setProperties(x => x.RowCreated, x => x.RowCreatedUser, x => x.RowUpdated, x => x.RowUpdatedUser);
        }

        void ITrackedEntity.SetUpdated(DateTimeOffset timestamp, int user, SetPropertiesHandler setProperties)
        {
            RowUpdated = timestamp;
            RowUpdatedUser = user;

            setProperties(x => x.RowUpdated, x => x.RowUpdatedUser);
        }

        void ITrackedEntity.SetVersion(int version)
        {
            Version = version;
        }

        void ITrackedEntity.SetDeleted(DateTimeOffset timestamp, int user)
        {
            RowUpdated = timestamp;
            RowUpdatedUser = user;
            IsActive = false;
        }

        bool ITrackedEntity.ShouldAudit()
        {
            return true;
        }

        #region Equality
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            // Safely compare types when proxies are in the mix
            if (this.GetUnproxiedType() != obj.GetUnproxiedType())
                return false;

            return Id == ((TrackedEntity)obj).Id;
        }

        public static bool operator ==(TrackedEntity left, TrackedEntity right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TrackedEntity left, TrackedEntity right)
        {
            return !Equals(left, right);
        }

        bool IEquatable<TrackedEntity>.Equals(TrackedEntity other)
        {
            return Equals(other);
        }

        int IComparable<TrackedEntity>.CompareTo(TrackedEntity other)
        {
            return Id.CompareTo((long)other.Id);
        }
        #endregion
    }
}