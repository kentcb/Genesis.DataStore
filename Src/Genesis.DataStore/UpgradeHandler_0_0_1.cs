namespace Genesis.DataStore
{
    using System;
    using SQLitePCL.pretty;

    /// <summary>
    /// Implements the initial, required upgrade handler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Creates the version table the data store service requires to track database versions. All implementations of
    /// <see cref="IUpgradeHandler"/> must be certain to return this upgrade handler.
    /// </para>
    /// </remarks>
    public sealed class UpgradeHandler_0_0_1 : UpgradeHandler
    {
        private const string createVersionTableSql = @"
CREATE TABLE version
(
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    major INTEGER NOT NULL,
    minor INTEGER NOT NULL,
    build INTEGER NOT NULL,
    timestamp INTEGER NOT NULL
);";

        /// <inheritdoc/>
        public override Version Version => new Version(0, 0, 1);

        /// <inheritdoc/>
        protected override void ApplyCore(IDatabaseConnection connection) =>
            connection.Execute(createVersionTableSql);
    }
}