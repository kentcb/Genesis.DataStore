namespace Genesis.DataStore.UnitTests.Builders
{
    using System.Collections.Generic;
    using global::Genesis.Util;
    using global::SQLitePCL.pretty;

    public sealed class ConnectionBuilder : IBuilder
    {
        private readonly IDatabaseConnection connection;
        private List<IUpgradeHandler> upgradeHandlers;

        public ConnectionBuilder()
        {
            this.connection = SQLite3.OpenInMemory();
            this.upgradeHandlers = new List<IUpgradeHandler>();

            this.WithUpgradeHandlers(new UpgradeHandler_0_0_1());
        }

        public ConnectionBuilder WithNoUpgradeHandlers() =>
            this.WithoutAll(ref this.upgradeHandlers);

        public ConnectionBuilder WithUpgradeHandler(IUpgradeHandler upgradeHandler) =>
            this.With(ref this.upgradeHandlers, upgradeHandler);

        public ConnectionBuilder WithUpgradeHandlers(params IUpgradeHandler[] upgradeHandlers) =>
            this.With(ref this.upgradeHandlers, upgradeHandlers);

        public IDatabaseConnection Build()
        {
            foreach (var upgradeHandler in this.upgradeHandlers)
            {
                upgradeHandler.Apply(this.connection);
            }

            return this.connection;
        }
    }
}