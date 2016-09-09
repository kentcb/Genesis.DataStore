namespace Genesis.DataStore
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using Genesis.Logging;
    using Internal;
    using SQLitePCL.pretty;

    /// <summary>
    /// A default implementation of <see cref="IDataStoreService"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Instances of this class can be used to manage a data store being stored against a given database connection. To create an instance, you
    /// must provide a means of creating an <see cref="IUpgradeHandlerProvider"/>, which will in turn dictate the maximum version of your
    /// database, and therefore whether it can be upgraded.
    /// </para>
    /// </remarks>
    public sealed class DataStoreService : IDataStoreService
    {
        private readonly IDatabaseConnection connection;
        private readonly IScheduler dataStoreScheduler;
        private readonly ILogger logger;
        private readonly Lazy<IUpgradeHandlerProvider> upgradeHandlerProvider;
        private readonly Lazy<IDataStoreVersionRepository> dataStoreVersionRepository;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="dataStoreScheduler">
        /// A scheduler with which all database operations will be performed.
        /// </param>
        /// <param name="logger">
        /// A logger.
        /// </param>
        /// <param name="upgradeHandlerProviderFactory">
        /// A factory to create an instance of <see cref="IUpgradeHandlerProvider"/>.
        /// </param>
        public DataStoreService(
            IDatabaseConnection connection,
            IScheduler dataStoreScheduler,
            ILogger logger,
            Func<IUpgradeHandlerProvider> upgradeHandlerProviderFactory)
            : this(connection, dataStoreScheduler, logger, upgradeHandlerProviderFactory, () => new DataStoreVersionRepository(connection))
        {
        }

        internal DataStoreService(
            IDatabaseConnection connection,
            IScheduler dataStoreScheduler,
            ILogger logger,
            Func<IUpgradeHandlerProvider> upgradeHandlerProviderFactory,
            Func<IDataStoreVersionRepository> dataStoreVersionRepositoryFactory)
        {
            Ensure.ArgumentNotNull(connection, nameof(connection));
            Ensure.ArgumentNotNull(dataStoreScheduler, nameof(dataStoreScheduler));
            Ensure.ArgumentNotNull(logger, nameof(logger));
            Ensure.ArgumentNotNull(upgradeHandlerProviderFactory, nameof(upgradeHandlerProviderFactory));
            Ensure.ArgumentNotNull(dataStoreVersionRepositoryFactory, nameof(dataStoreVersionRepositoryFactory));

            this.dataStoreScheduler = dataStoreScheduler;
            this.connection = connection;
            this.logger = logger;
            this.upgradeHandlerProvider = new Lazy<IUpgradeHandlerProvider>(upgradeHandlerProviderFactory);
            this.dataStoreVersionRepository = new Lazy<IDataStoreVersionRepository>(dataStoreVersionRepositoryFactory);
        }

        /// <inheritdoc/>
        public IDatabaseConnection Connection => this.connection;

        /// <inheritdoc/>
        public IObservable<bool> GetIsUpgradeAvailable() =>
            this
                .GetLatestVersion()
                .Select(
                    currentVersion =>
                        this
                            .upgradeHandlerProvider
                            .Value
                            .Handlers
                            .Any(x => x.Version > currentVersion.Version));

        /// <inheritdoc/>
        public IObservable<DataStoreVersion> GetLatestVersion() =>
            Observable
                .Defer(
                    () =>
                        Observable
                            .Start(
                                () =>
                                {
                                    if (!this.dataStoreVersionRepository.Value.Exists())
                                    {
                                        this.logger.Warn("No version table detected - assuming version is at 0.0.0.");
                                        return DataStoreVersionEntity.None.ToContract();
                                    }

                                    return this
                                        .dataStoreVersionRepository
                                        .Value
                                        .GetLatest()
                                        .ToContract();
                                },
                                this.dataStoreScheduler));

        /// <inheritdoc/>
        public IObservable<Unit> Upgrade() =>
            this
                .GetLatestVersion()
                .Do(
                    currentVersion =>
                    {
                        var upgradeHandlers = this
                            .upgradeHandlerProvider
                            .Value
                            .Handlers
                            .Where(x => x.Version > currentVersion.Version)
                            .OrderBy(x => x.Version);

                        foreach (var upgradeHandler in upgradeHandlers)
                        {
                            this.logger.Info("Applying upgrade to version {0}.", upgradeHandler.Version);

                            try
                            {
                                this
                                    .Connection
                                    .RunInTransaction(
                                        _ =>
                                        {
                                            upgradeHandler.Apply(this.Connection);
                                            this.InsertVersion(upgradeHandler.Version);
                                        },
                                        TransactionMode.Exclusive);
                            }
                            catch (Exception ex)
                            {
                                this.logger.Error(ex, "Failed to upgrade the data store. Upgrade handler with version {0} failed.", upgradeHandler.Version);
                                break;
                            }
                        }
                    })
                .Select(_ => Unit.Default);

        private void InsertVersion(Version version) =>
            this
                .dataStoreVersionRepository
                .Value
                .Save(
                    new DataStoreVersionEntity(
                        -1,
                        version.Major,
                        version.Minor,
                        version.Build,
                        DateTime.UtcNow.ToUnixTime()));
    }
}