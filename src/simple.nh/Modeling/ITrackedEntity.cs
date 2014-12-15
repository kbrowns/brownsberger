using System;

namespace Simple.NH.Modeling
{
    public interface ITrackedEntity : IDomainEntity
    {
        /// <summary>
        /// The indicator of soft deletes.  False indicates the RootEntitySpecification is not active.  True indictates it is active.
        /// 
        /// </summary>
        [Order]
        bool IsActive { get; }

        /// <summary>
        /// The timestamp captured when the RootEntitySpecification was first created.
        /// 
        /// </summary>
        [Order]
        DateTimeOffset RowCreated { get; }

        /// <summary>
        /// The identifier of the user responsible for creating the RootEntitySpecification.
        /// 
        /// </summary>
        [Order]
        int RowCreatedUser { get; }

        // move-to-long
        /// <summary>
        /// The timestamp captured when the RootEntitySpecification is updated.  This field is only populated if it has been modified.
        /// 
        /// </summary>
        [Order]
        DateTimeOffset RowUpdated { get; }

        /// <summary>
        /// The identifier of the user responsible for modifying the RootEntitySpecification.
        /// 
        /// </summary>
        [Order]
        int RowUpdatedUser { get; }

        /// <summary>
        /// Numeric version counter managed by NHibernate
        /// </summary>
        [Order]
        int Version { get; }

        void SetCreated(DateTimeOffset timestamp, int user, SetPropertiesHandler setProperties);

        void SetUpdated(DateTimeOffset timestamp, int user, SetPropertiesHandler setProperties);
        void SetVersion(int version);
        void SetDeleted(DateTimeOffset timestamp, int user);
        bool ShouldAudit();
    }
}