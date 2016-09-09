namespace Genesis.DataStore
{
    using System;
    using Genesis.Ensure;
    using SQLitePCL.pretty;

    /// <summary>
    /// A default implementation of <see cref="IUpgradeHandler"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations of <see cref="IUpgradeHandler"/> can extend this class to reduce effort.
    /// </para>
    /// </remarks>
    public abstract class UpgradeHandler : IUpgradeHandler
    {
        /// <summary>
        /// Gets the version of the upgrade handler.
        /// </summary>
        public abstract Version Version
        {
            get;
        }

        /// <summary>
        /// Applies the upgrade handler against the specified database connection.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        public void Apply(IDatabaseConnection connection)
        {
            Ensure.ArgumentNotNull(connection, nameof(connection));
            this.ApplyCore(connection);
        }

        /// <summary>
        /// Applies the upgrade handler against the specified database connection.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        protected abstract void ApplyCore(IDatabaseConnection connection);
    }
}