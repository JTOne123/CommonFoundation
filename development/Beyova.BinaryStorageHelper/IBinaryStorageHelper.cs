﻿using System.Collections.Generic;

namespace Beyova.Binary
{
    /// <summary>
    /// Interface IBinaryStorageHelper.
    /// </summary>
    public interface IBinaryStorageHelper<TCloudContainer, TCloudBlobObject, TBinaryStorageMetaData, TBinaryStorageCriteria>
        : IBinaryStorageHelper<TBinaryStorageMetaData, TBinaryStorageCriteria>
        where TBinaryStorageMetaData : BinaryStorageMetaData, new()
        where TBinaryStorageCriteria : BinaryStorageMetaDataCriteria, new()
    {
        /// <summary>
        /// Queries the BLOB.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="md5">The MD5.</param>
        /// <param name="length">The length.</param>
        /// <param name="limitCount">The limit count.</param>
        /// <returns>IEnumerable&lt;TCloudBlobObject&gt;.</returns>
        IEnumerable<TCloudBlobObject> QueryBlob(TCloudContainer container, string contentType, string md5, long? length, int limitCount);
    }
}