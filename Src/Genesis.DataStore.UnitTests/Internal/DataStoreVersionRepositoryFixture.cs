namespace Genesis.DataStore.UnitTests.Internal
{
    using System;
    using System.Linq;
    using Builders;
    using UnitTests.Builders;
    using Xunit;

    public sealed class DataStoreVersionRepositoryFixture
    {
        [Fact]
        public void exists_returns_false_if_no_versions_table_exists()
        {
            var sut = new DataStoreVersionRepositoryBuilder()
                .WithConnection(
                    new ConnectionBuilder()
                        .WithNoUpgradeHandlers()
                        .Build())
                .Build();

            Assert.False(sut.Exists());
        }

        [Fact]
        public void exists_returns_true_if_versions_table_exists()
        {
            var sut = new DataStoreVersionRepositoryBuilder().Build();

            Assert.True(sut.Exists());
        }

        [Theory]
        [InlineData(new string[0], "0.0.0")]
        [InlineData(new string[] { "1.0.0" }, "1.0.0")]
        [InlineData(new string[] { "1.0.0", "1.1.0" }, "1.1.0")]
        [InlineData(new string[] { "1.1.0", "1.0.0" }, "1.1.0")]
        [InlineData(new string[] { "1.0.2370", "1.0.2371", "1.0.2369" }, "1.0.2371")]
        [InlineData(new string[] { "1.2346.2370", "1.1230.34867", "2.0.0" }, "2.0.0")]
        public void get_latest_version_returns_correct_value(string[] existingVersions, string expectedLatestVersionString)
        {
            var expectedLatestVersion = Version.Parse(expectedLatestVersionString);
            var sut = new DataStoreVersionRepositoryBuilder()
                .WithVersions(
                    existingVersions
                        .Select(
                            x =>
                                new DataStoreVersionEntityBuilder()
                                    .WithVersion(Version.Parse(x))
                                    .Build())
                        .ToArray())
                .Build();

            var latestVersion = sut.GetLatest();

            Assert.Equal(expectedLatestVersion.Major, latestVersion.Major);
            Assert.Equal(expectedLatestVersion.Minor, latestVersion.Minor);
            Assert.Equal(expectedLatestVersion.Build, latestVersion.Build);
        }

        [Fact]
        public void get_all_gets_all_versions_in_correct_order()
        {
            var sut = new DataStoreVersionRepositoryBuilder()
                .WithVersion(
                    new DataStoreVersionEntityBuilder()
                        .WithMajor(1)
                        .Build())
                .WithVersion(
                    new DataStoreVersionEntityBuilder()
                        .WithMajor(2)
                        .WithMinor(7)
                        .Build())
                .WithVersion(
                    new DataStoreVersionEntityBuilder()
                        .WithMajor(1)
                        .WithMinor(1)
                        .Build())
                .WithVersion(
                    new DataStoreVersionEntityBuilder()
                        .WithMajor(2)
                        .Build())
                .Build();

            var allVersions = sut
                .GetAll()
                .ToList();

            Assert.Equal(4, allVersions.Count);
            Assert.Equal(2, allVersions[0].Major);
            Assert.Equal(7, allVersions[0].Minor);
            Assert.Equal(0, allVersions[0].Build);
            Assert.Equal(2, allVersions[1].Major);
            Assert.Equal(0, allVersions[1].Minor);
            Assert.Equal(0, allVersions[1].Build);
            Assert.Equal(1, allVersions[2].Major);
            Assert.Equal(1, allVersions[2].Minor);
            Assert.Equal(0, allVersions[2].Build);
            Assert.Equal(1, allVersions[3].Major);
            Assert.Equal(0, allVersions[3].Minor);
            Assert.Equal(0, allVersions[3].Build);
        }
    }
}