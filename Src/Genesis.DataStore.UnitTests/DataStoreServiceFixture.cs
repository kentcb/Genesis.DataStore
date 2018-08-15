namespace Genesis.DataStore.UnitTests
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using Builders;
    using DataStore.Internal;
    using global::SQLitePCL.pretty;
    using Internal.Builders;
    using Internal.Mocks;
    using Microsoft.Reactive.Testing;
    using Mocks;
    using PCLMock;
    using Xunit;

    public sealed class DataStoreServiceFixture : IClassFixture<SQLite>
    {
        //[Fact]
        //public void connection_returns_connection()
        //{
        //    var connection = new DatabaseConnectionMock(MockBehavior.Loose);
        //    var sut = new DataStoreServiceBuilder()
        //        .WithConnection(connection)
        //        .Build();

        //    Assert.Same(connection, sut.Connection);
        //}

        [Theory]
        [InlineData(new string[0], new string[0], false)]
        [InlineData(new string[0], new string[] { "0.0.0" }, false)]
        [InlineData(new string[0], new string[] { "1.0.0" }, true)]
        [InlineData(new string[] { "1.0.0" }, new string[] { "1.0.0" }, false)]
        [InlineData(new string[] { "1.1.0" }, new string[] { "2.0.0" }, true)]
        [InlineData(new string[] { "1.58.2387" }, new string[] { "2.0.0" }, true)]
        [InlineData(new string[] { "1.1.0" }, new string[] { "1.2.0" }, true)]
        [InlineData(new string[] { "1.1.0" }, new string[] { "1.1.1" }, true)]
        [InlineData(new string[] { "1.0.9" }, new string[] { "1.1.10" }, true)]
        [InlineData(new string[] { "1.0.0", "2.0.0", "3.0.0" }, new string[] { "2.8.0" }, false)]
        [InlineData(new string[] { "1.0.0", "2.0.0", "3.0.0" }, new string[] { "3.1.0" }, true)]
        public void get_is_upgrade_available_returns_correct_value(string[] existingVersions, string[] availableVersions, bool expected)
        {
            var sut = new DataStoreServiceBuilder()
                .WithDataStoreScheduler(ImmediateScheduler.Instance)
                .WithDataStoreVersionRepository(
                    new DataStoreVersionRepositoryBuilder()
                        .WithVersions(
                            existingVersions
                                .Select(
                                    x =>
                                        new DataStoreVersionEntityBuilder()
                                            .WithVersion(Version.Parse(x))
                                            .Build())
                                .ToArray())
                    .Build())
                .WithUpgradeHandlers(
                    availableVersions
                        .Select(
                            x =>
                                new UpgradeHandlerMockBuilder()
                                    .WithVersion(Version.Parse(x))
                                    .Build())
                        .ToArray())
                .Build();

            Assert.Equal(expected, sut.GetIsUpgradeAvailable().FirstAsync().Wait());
        }

        [Fact]
        public void get_latest_version_returns_zero_version_if_version_repository_indicates_the_data_store_does_not_yet_exist()
        {
            var dataStoreVersionRepository = new DataStoreVersionRepositoryMock(MockBehavior.Loose);
            dataStoreVersionRepository
                .When(x => x.Exists())
                .Return(false);
            var sut = new DataStoreServiceBuilder()
                .WithDataStoreScheduler(ImmediateScheduler.Instance)
                .WithDataStoreVersionRepository(dataStoreVersionRepository)
                .Build();

            var result = sut
                .GetLatestVersion()
                .FirstAsync()
                .Wait();

            Assert.NotNull(result);
            Assert.Equal(0, result.Version.Major);
            Assert.Equal(0, result.Version.Minor);
            Assert.Equal(0, result.Version.Build);
        }

        [Fact]
        public void get_latest_version_forwards_call_onto_version_repository()
        {
            var dataStoreVersionRepository = new DataStoreVersionRepositoryMock(MockBehavior.Loose);
            dataStoreVersionRepository
                .When(x => x.GetLatest())
                .Return(
                    new DataStoreVersionEntityBuilder()
                        .WithMajor(2)
                        .WithMinor(1)
                        .WithBuild(3));
            var sut = new DataStoreServiceBuilder()
                .WithDataStoreScheduler(ImmediateScheduler.Instance)
                .WithDataStoreVersionRepository(dataStoreVersionRepository)
                .Build();

            var result = sut
                .GetLatestVersion()
                .FirstAsync()
                .Wait();

            Assert.Equal(2, result.Version.Major);
            Assert.Equal(1, result.Version.Minor);
            Assert.Equal(3, result.Version.Build);
        }

        [Theory]
        [InlineData(new string[] { "1.0.0" }, new string[] { "1.0.0" }, new[] { false })]
        [InlineData(new string[] { "0.0.1" }, new string[] { "1.0.0" }, new[] { true })]
        [InlineData(new string[] { "0.0.1" }, new string[] { "1.0.0", "1.1.0" }, new[] { true, true })]
        [InlineData(new string[] { "0.0.1" }, new string[] { "0.0.1", "1.0.0" }, new[] { false, true })]
        [InlineData(new string[] { "0.0.1", "1.0.0" }, new string[] { "0.0.1", "1.0.0" }, new[] { false, false })]
        [InlineData(new string[] { "0.0.1", "1.0.0" }, new string[] { "0.0.1", "1.0.0", "1.2.0" }, new[] { false, false, true })]
        public void upgrade_applies_relevant_upgrades_only(string[] existingVersions, string[] availableVersions, bool[] applyExpected)
        {
            Assert.Equal(availableVersions.Length, applyExpected.Length);

            var scheduler = new TestScheduler();
            var versions = existingVersions
                .Select(
                    x =>
                        new DataStoreVersionEntityBuilder()
                            .WithVersion(Version.Parse(x))
                            .Build())
                .ToImmutableList();
            var dataStoreVersionRepository = new DataStoreVersionRepositoryMock(MockBehavior.Loose);
            dataStoreVersionRepository
                .When(x => x.GetAll())
                .Return(versions);
            dataStoreVersionRepository
                .When(x => x.GetLatest())
                .Return(versions.Last());
            var upgradeHandlers = availableVersions
                .Select(
                    x =>
                    {
                        var mock = new UpgradeHandlerMock(MockBehavior.Loose);
                        mock
                            .When(y => y.Version)
                            .Return(Version.Parse(x));
                        return mock;
                    })
                .ToArray();
            var sut = new DataStoreServiceBuilder()
                .WithDataStoreScheduler(scheduler)
                .WithDataStoreVersionRepository(dataStoreVersionRepository)
                .WithUpgradeHandlers(upgradeHandlers)
                .Build();

            sut
                .Upgrade()
                .Subscribe();
            scheduler.AdvanceMinimal();

            for (var i = 0; i < availableVersions.Length; ++i)
            {
                var upgradeHandler = upgradeHandlers[i];

                if (applyExpected[i])
                {
                    upgradeHandler
                        .Verify(x => x.Apply(sut.Connection))
                        .WasCalledExactlyOnce();
                }
                else
                {
                    upgradeHandler
                        .Verify(x => x.Apply(sut.Connection))
                        .WasNotCalled();
                }
            }
        }

        [Fact]
        public void upgrade_aborts_if_any_upgrade_fails()
        {
            var scheduler = new TestScheduler();
            var upgradeHandler1 = new UpgradeHandlerMock(MockBehavior.Loose);
            var upgradeHandler2 = new UpgradeHandlerMock(MockBehavior.Loose);
            upgradeHandler1
                .When(x => x.Version)
                .Return(new Version(1, 0, 0));
            upgradeHandler1
                .When(x => x.Apply(It.IsAny<IDatabaseConnection>()))
                .Throw();
            upgradeHandler2
                .When(x => x.Version)
                .Return(new Version(2, 0, 0));
            var sut = new DataStoreServiceBuilder()
                .WithDataStoreScheduler(scheduler)
                .WithUpgradeHandlers(upgradeHandler1, upgradeHandler2)
                .Build();

            sut
                .Upgrade()
                .Subscribe();
            scheduler.AdvanceMinimal();

            upgradeHandler1
                .Verify(x => x.Apply(sut.Connection))
                .WasCalledExactlyOnce();
            upgradeHandler2
                .Verify(x => x.Apply(It.IsAny<IDatabaseConnection>()))
                .WasNotCalled();
        }

        [Fact]
        public void upgrade_records_versions()
        {
            var scheduler = new TestScheduler();
            var dataStoreVersionRepository = new DataStoreVersionRepositoryMock(MockBehavior.Loose);
            var upgradeHandler1 = new UpgradeHandlerMock(MockBehavior.Loose);
            var upgradeHandler2 = new UpgradeHandlerMock(MockBehavior.Loose);
            upgradeHandler1
                .When(x => x.Version)
                .Return(new Version(1, 0, 0));
            upgradeHandler2
                .When(x => x.Version)
                .Return(new Version(2, 0, 1));
            var sut = new DataStoreServiceBuilder()
                .WithDataStoreScheduler(scheduler)
                .WithDataStoreVersionRepository(dataStoreVersionRepository)
                .WithUpgradeHandlers(upgradeHandler1, upgradeHandler2)
                .Build();

            sut
                .Upgrade()
                .Subscribe();
            scheduler.AdvanceMinimal();

            dataStoreVersionRepository
                .Verify(x => x.Save(It.Matches<DataStoreVersionEntity>(y => y.Major == 1 && y.Minor == 0 && y.Build == 0)))
                .WasCalledExactlyOnce();
            dataStoreVersionRepository
                .Verify(x => x.Save(It.Matches<DataStoreVersionEntity>(y => y.Major == 2 && y.Minor == 0 && y.Build == 1)))
                .WasCalledExactlyOnce();
        }
    }
}