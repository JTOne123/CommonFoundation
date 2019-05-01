﻿using Beyova.Api;
using Newtonsoft.Json;

namespace Beyova
{
    /// <summary>
    /// Class RegionalServiceEndpoint.
    /// </summary>
    /// <typeparam name="TRegion">The type of the t region.</typeparam>
    public class RegionalServiceEndpoint<TRegion> : ApiEndpoint
    {
        /// <summary>
        /// Gets or sets the region.
        /// </summary>
        /// <value>The region.</value>
        [JsonProperty("region")]
        public TRegion Region { get; set; }
    }

    /// <summary>
    /// Class RegionalServiceEndpoint.
    /// </summary>
    public class RegionalServiceEndpoint : RegionalServiceEndpoint<string>
    {
    }
}