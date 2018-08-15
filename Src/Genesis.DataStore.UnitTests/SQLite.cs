namespace Genesis.DataStore.UnitTests
{
    using System;

    public sealed class SQLite : IDisposable
    {
        public SQLite() => global::SQLitePCL.Batteries_V2.Init();

        public void Dispose()
        {
        }
    }
}