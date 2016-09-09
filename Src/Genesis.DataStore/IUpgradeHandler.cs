namespace Genesis.DataStore
{
    using System;
    using SQLitePCL.pretty;

    /// <summary>
    /// Defines the behavior of an upgrade handler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An upgrade handler is capable of upgrading the database from one version to the next. With each application release that requires
    /// database changes, create a new instance of this interface (normally by extending <see cref="UpgradeHandler"/>) and returning it
    /// from your <see cref="IUpgradeHandlerProvider"/>. Your upgrade handler needs to make any database modifications to take the
    /// database from the prior version to the version specified by <see cref="Version"/>.
    /// </para>
    /// </remarks>
    public interface IUpgradeHandler
    {
        /// <summary>
        /// Gets the version of the upgrade.
        /// </summary>
        Version Version
        {
            get;
        }

        /// <summary>
        /// Applies the upgrade to the given database connection.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        void Apply(IDatabaseConnection connection);
    }
}