namespace Genesis.DataStore.UnitTests.Internal.Mocks
{
    using DataStore.Internal;

    partial class DataStoreVersionRepositoryMock
    {
        partial void ConfigureLooseBehavior()
        {
            this
                .When(x => x.Exists())
                .Return(true);
            this
                .When(x => x.GetLatest())
                .Return(DataStoreVersionEntity.None);
        }
    }
}