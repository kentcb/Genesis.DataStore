namespace Genesis.DataStore.Internal
{
    using System;
    using System.Collections.Generic;
    using SQLitePCL.pretty;

    internal static class DataStoreVersionEntityExtensions
    {
        public static DataStoreVersion ToContract(this DataStoreVersionEntity @this)
        {
            if (@this == null)
            {
                return null;
            }

            return new DataStoreVersion(
                new Version(@this.Major, @this.Minor, @this.Build),
                @this.Timestamp.ToDateTime());
        }

        public static DataStoreVersionEntity ToVersionEntity(this IReadOnlyList<IResultSetValue> resultSet)
        {
            var none = DataStoreVersionEntity.None;
            var id = none.Id;
            var major = none.Major;
            var minor = none.Minor;
            var build = none.Build;
            var timestamp = 0L;

            if (resultSet != null)
            {
                id = resultSet[0].ToInt();
                major = resultSet[1].ToInt();
                minor = resultSet[2].ToInt();
                build = resultSet[3].ToInt();
                timestamp = resultSet[4].ToInt64();
            }

            return new DataStoreVersionEntity(
                id,
                major,
                minor,
                build,
                timestamp);
        }
    }
}