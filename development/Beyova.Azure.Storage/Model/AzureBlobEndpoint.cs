﻿namespace Beyova.Azure
{
    /// <summary>
    /// Class AzureBlobEndpoint.
    /// </summary>
    public class AzureBlobEndpoint : RegionalServiceEndpoint<AzureServiceProviderRegion>
    {
        /// <summary>
        /// Froms the connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>AzureBlobEndpoint.</returns>
        public static AzureBlobEndpoint FromConnectionString(string connectionString)
        {
            return AzureStorageExtension.AsAzureBlobEndpoint(connectionString);
        }
    }
}