namespace Genesis.DataStore.UnitTests.Internal.Builders
{
    using System;
    using DataStore.Internal;
    using global::Genesis.TestUtil;

    internal sealed class DataStoreVersionEntityBuilder : IBuilder
    {
        private int? id;
        private int build;
        private int major;
        private int minor;
        private long timestamp;

        public DataStoreVersionEntityBuilder WithId(int? id) =>
            this.With(ref this.id, id);

        public DataStoreVersionEntityBuilder WithMajor(int major) =>
            this.With(ref this.major, major);

        public DataStoreVersionEntityBuilder WithMinor(int minor) =>
            this.With(ref this.minor, minor);

        public DataStoreVersionEntityBuilder WithBuild(int build) =>
            this.With(ref this.build, build);

        public DataStoreVersionEntityBuilder WithVersion(int major, int minor, int build) =>
            this
                .WithMajor(major)
                .WithMinor(minor)
                .WithBuild(build);

        public DataStoreVersionEntityBuilder WithVersion(Version version) =>
            this
                .WithVersion(version.Major, version.Minor, version.Build);

        public DataStoreVersionEntityBuilder WithTimestamp(long timestamp) =>
            this.With(ref this.timestamp, timestamp);

        public DataStoreVersionEntityBuilder WithTimestamp(DateTime timestamp) =>
            this.With(ref this.timestamp, timestamp.ToUnixTime());

        public DataStoreVersionEntity Build() =>
            new DataStoreVersionEntity(
                this.id,
                this.major,
                this.minor,
                this.build,
                this.timestamp);

        public static implicit operator DataStoreVersionEntity(DataStoreVersionEntityBuilder builder) =>
            builder.Build();
    }
}