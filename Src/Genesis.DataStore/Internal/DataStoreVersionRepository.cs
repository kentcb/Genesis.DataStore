namespace Genesis.DataStore.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Genesis.Repository;
    using SQLitePCL.pretty;

    internal sealed class DataStoreVersionRepository : Repository<int, DataStoreVersionEntity>, IDataStoreVersionRepository
    {
        private const string existsSql = @"
SELECT 1
FROM sqlite_master
WHERE type='table'
    AND name='version'";

        private const string getLatestVersionSql = @"
SELECT id, major, minor, build, timestamp
FROM version
ORDER BY major DESC, minor DESC, build DESC
LIMIT 1";

        private readonly Lazy<IStatement> existsStatement;
        private readonly Lazy<IStatement> getLatestStatement;

        public DataStoreVersionRepository(IDatabaseConnection connection)
            : base(connection)
        {
            this.existsStatement = new Lazy<IStatement>(() => connection.PrepareStatement(existsSql));
            this.getLatestStatement = new Lazy<IStatement>(() => connection.PrepareStatement(getLatestVersionSql));
        }

        protected override string TableName => "version";

        protected override IEnumerable<Column> Columns =>
            new[]
            {
                new Column("id", true),
                new Column("major", sortOrder: SortOrder.Descending),
                new Column("minor", sortOrder: SortOrder.Descending),
                new Column("build", sortOrder: SortOrder.Descending),
                new Column("timestamp")
            };

        protected override DataStoreVersionEntity ValuesToEntity(IReadOnlyList<IResultSetValue> resultSet) =>
            resultSet.ToVersionEntity();

        protected override IEnumerable<object> EntityToValues(DataStoreVersionEntity entity) =>
            new object[]
            {
                entity.Id,
                entity.Major,
                entity.Minor,
                entity.Build,
                DateTime.UtcNow.ToUnixTime()
            };

        public bool Exists() =>
            this
                .Connection
                .EnsureRunInTransaction(
                    _ =>
                        this
                            .existsStatement
                            .Value
                            .Query()
                            .FirstOrDefault() != null);

        public DataStoreVersionEntity GetLatest() =>
            this
                .Connection
                .EnsureRunInTransaction(
                    _ =>
                        this
                            .getLatestStatement
                            .Value
                            .Query()
                            .FirstOrDefault()
                            .ToVersionEntity());

        protected override DataStoreVersionEntity OnEntitySaved(DataStoreVersionEntity entity, IDatabaseConnection connection) =>
            entity.WithId((int)connection.LastInsertedRowId);
    }
}