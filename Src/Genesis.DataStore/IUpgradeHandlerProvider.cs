namespace Genesis.DataStore
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of an upgrade handler provider.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Your application should define an implementation of this interface (or just use <see cref="UpgradeHandlerProvider"/>)  that returns
    /// all upgrade handlers that have been created over the lifetime of the application's development. With each release requiring database
    /// changes, a new <see cref="IUpgradeHandler"/> should be created and added to those returned by <see cref="Handlers"/>.
    /// </para>
    /// </remarks>
    public interface IUpgradeHandlerProvider
    {
        /// <summary>
        /// Gets all the upgrade handlers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Gets all upgrade handlers for the database. Note that you should be sure to always at least include <see cref="UpgradeHandler_0_0_1"/>
        /// in the enumerable.
        /// </para>
        /// </remarks>
        IEnumerable<IUpgradeHandler> Handlers
        {
            get;
        }
    }
}