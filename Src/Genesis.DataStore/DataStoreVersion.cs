namespace Genesis.DataStore
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Encapsulates data about a data store version.
    /// </summary>
    [DebuggerDisplay("V{Version}")]
    public sealed class DataStoreVersion
    {
        private readonly Version version;
        private readonly DateTime timestamp;

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <param name="timestamp">
        /// The timestamp.
        /// </param>
        public DataStoreVersion(
            Version version,
            DateTime timestamp)
        {
            this.version = version;
            this.timestamp = timestamp;
        }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The version of the data store.
        /// </para>
        /// </remarks>
        public Version Version => this.version;

        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The time at which the version was applied to the data store.
        /// </para>
        /// </remarks>
        public DateTime Timestamp => this.timestamp;
    }
}