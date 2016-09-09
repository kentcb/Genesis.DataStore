namespace Genesis.DataStore
{
    using System.Collections.Generic;
    using Genesis.Ensure;

    /// <summary>
    /// A default implementation of <see cref="IUpgradeHandlerProvider"/>.
    /// </summary>
    public sealed class UpgradeHandlerProvider : IUpgradeHandlerProvider
    {
        private readonly IEnumerable<IUpgradeHandler> handlers;

        /// <summary>
        /// Creates a new instance of this class.
        /// </summary>
        /// <param name="handlers">
        /// The upgrade handlers to return.
        /// </param>
        public UpgradeHandlerProvider(IEnumerable<IUpgradeHandler> handlers)
        {
            Ensure.ArgumentNotNull(handlers, nameof(handlers));
            this.handlers = handlers;
        }

        /// <inheritdoc/>
        public IEnumerable<IUpgradeHandler> Handlers => this.handlers;
    }
}