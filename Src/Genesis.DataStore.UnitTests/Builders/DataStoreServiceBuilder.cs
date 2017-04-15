namespace Genesis.DataStore.UnitTests.Builders
{
    using System.Reactive.Concurrency;
    using System.Reactive.Concurrency.Mocks;
    using Genesis.Logging.Mocks;
    using global::Genesis.DataStore.Internal;
    using global::Genesis.Logging;
    using global::Genesis.TestUtil;
    using global::SQLitePCL.pretty;
    using Internal.Mocks;
    using Mocks;
    using PCLMock;

    public sealed class DataStoreServiceBuilder : IBuilder
    {
        private IDatabaseConnection connection;
        private IScheduler dataStoreScheduler;
        private ILogger logger;
        private IUpgradeHandlerProvider upgradeHandlerProvider;
        private IDataStoreVersionRepository dataStoreVersionRepository;

        public DataStoreServiceBuilder()
        {
            this.connection = SQLite3.OpenInMemory();
            this.dataStoreScheduler = new SchedulerMock(MockBehavior.Loose);
            this.logger = new LoggerMock(MockBehavior.Loose);
            this.upgradeHandlerProvider = new UpgradeHandlerProviderMock(MockBehavior.Loose);
            this.dataStoreVersionRepository = new DataStoreVersionRepositoryMock(MockBehavior.Loose);
        }

        public DataStoreServiceBuilder WithConnection(IDatabaseConnection connection) =>
            this.With(ref this.connection, connection);

        public DataStoreServiceBuilder WithDataStoreScheduler(IScheduler dataStoreScheduler) =>
            this.With(ref this.dataStoreScheduler, dataStoreScheduler);

        internal DataStoreServiceBuilder WithDataStoreVersionRepository(IDataStoreVersionRepository dataStoreVersionRepository) =>
            this.With(ref this.dataStoreVersionRepository, dataStoreVersionRepository);

        public DataStoreServiceBuilder WithLogger(ILogger logger) =>
            this.With(ref this.logger, logger);

        public DataStoreServiceBuilder WithUpgradeHandlers(params IUpgradeHandler[] upgradeHandlers)
        {
            var upgradeHandlerProvider = new UpgradeHandlerProviderMock(MockBehavior.Loose);
            upgradeHandlerProvider
                .When(x => x.Handlers)
                .Return(upgradeHandlers);

            return this
                .With(ref this.upgradeHandlerProvider, upgradeHandlerProvider);
        }

        public DataStoreService Build() =>
            new DataStoreService(
                this.connection,
                this.dataStoreScheduler,
                this.logger,
                () => this.upgradeHandlerProvider,
                () => this.dataStoreVersionRepository);

        public static implicit operator DataStoreService(DataStoreServiceBuilder builder) =>
            builder.Build();
    }
}