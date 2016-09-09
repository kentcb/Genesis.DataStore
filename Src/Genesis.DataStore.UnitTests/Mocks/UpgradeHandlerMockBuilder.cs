namespace Genesis.DataStore.UnitTests.Mocks
{
    using System;
    using PCLMock;

    public sealed class UpgradeHandlerMockBuilder
    {
        private readonly UpgradeHandlerMock upgradeHandler;

        public UpgradeHandlerMockBuilder()
        {
            this.upgradeHandler = new UpgradeHandlerMock(MockBehavior.Loose);
        }

        public UpgradeHandlerMockBuilder WithVersion(Version version)
        {
            this
                .upgradeHandler
                .When(x => x.Version)
                .Return(version);
            return this;
        }

        public UpgradeHandlerMock Build() =>
            this.upgradeHandler;

        public static implicit operator UpgradeHandlerMock(UpgradeHandlerMockBuilder builder) =>
            builder.Build();
    }
}
