namespace Genesis.DataStore
{
    using System;
    using System.Reactive;
    using SQLitePCL.pretty;

    /// <summary>
    /// Manages a data store.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instances of this interface can be used to manage a data store behind a given database connection. See <see cref="DataStoreService"/>
    /// for an implementation.
    /// </para>
    /// </remarks>
    public interface IDataStoreService
    {
        /// <summary>
        /// Gets the database connection in which the data store resides.
        /// </summary>
        IDatabaseConnection Connection
        {
            get;
        }

        /// <summary>
        /// Gets the details of the latest version record for the data store.
        /// </summary>
        /// <returns>
        /// The version details.
        /// </returns>
        IObservable<DataStoreVersion> GetLatestVersion();

        /// <summary>
        /// Gets a value indicating whether the data store can be upgraded.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if there is an upgrade available, otherwise <see langword="false"/>.
        /// </returns>
        IObservable<bool> GetIsUpgradeAvailable();

        /// <summary>
        /// Upgrades the data store to the latest version.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If multiple upgrade handlers are outstanding for the target database, they will each be executed in version order.
        /// </para>
        /// </remarks>
        /// <returns>
        /// An observable that ticks when the upgrade completes.
        /// </returns>
        IObservable<Unit> Upgrade();
    }
}