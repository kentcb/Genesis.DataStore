namespace Genesis.DataStore.UnitTests.Internal.Builders
{
    using System.Collections.Generic;
    using DataStore.Internal;
    using DataStore.UnitTests.Builders;
    using global::Genesis.TestUtil;
    using global::SQLitePCL.pretty;

    internal sealed class DataStoreVersionRepositoryBuilder : IBuilder
    {
        private List<DataStoreVersionEntity> versions;
        private IDatabaseConnection connection;

        public DataStoreVersionRepositoryBuilder()
        {
            this.versions = new List<DataStoreVersionEntity>();
            this.connection = new ConnectionBuilder().Build();
        }

        public DataStoreVersionRepositoryBuilder WithConnection(IDatabaseConnection connection) =>
            this.With(ref this.connection, connection);

        public DataStoreVersionRepositoryBuilder WithVersion(DataStoreVersionEntity version) =>
            this.With(ref this.versions, version);

        public DataStoreVersionRepositoryBuilder WithVersions(params DataStoreVersionEntity[] versions) =>
            this.With(ref this.versions, versions);

        public DataStoreVersionRepository Build()
        {
            var dataStoreVersionRepository = new DataStoreVersionRepository(this.connection);

            foreach (var version in this.versions)
            {
                dataStoreVersionRepository.Save(version);
            }

            return dataStoreVersionRepository;
        }

        public static implicit operator DataStoreVersionRepository(DataStoreVersionRepositoryBuilder builder) =>
            builder.Build();
    }
}