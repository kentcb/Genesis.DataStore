namespace Genesis.DataStore.Internal
{
    using Genesis.Repository;

    internal interface IDataStoreVersionRepository : IRepository<int, DataStoreVersionEntity>
    {
        // does the version table even exist?
        bool Exists();

        DataStoreVersionEntity GetLatest();
    }
}