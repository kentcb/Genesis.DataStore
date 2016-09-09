namespace Genesis.DataStore.Internal
{
    using Genesis.Repository;

    internal sealed class DataStoreVersionEntity : IEntity<int>
    {
        public static readonly DataStoreVersionEntity None = new DataStoreVersionEntity(null, 0, 0, 0, 0);

        private readonly int? id;
        private readonly int major;
        private readonly int minor;
        private readonly int build;
        private readonly long timestamp;

        public DataStoreVersionEntity(
            int? id,
            int major,
            int minor,
            int build,
            long timestamp)
        {
            this.id = id;
            this.major = major;
            this.minor = minor;
            this.build = build;
            this.timestamp = timestamp;
        }

        public int? Id => this.id;

        public int Major => this.major;

        public int Minor => this.minor;

        public int Build => this.build;

        public long Timestamp => this.timestamp;

        public DataStoreVersionEntity WithId(int? id) =>
            new DataStoreVersionEntity(
                id,
                this.major,
                this.minor,
                this.build,
                this.timestamp);
    }
}