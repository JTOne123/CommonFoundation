﻿using Newtonsoft.Json;
using System;

namespace Beyova.Api
{
    /// <summary>
    /// Class ApiEndpoint.
    /// </summary>
    public class ApiEndpoint : UriEndpoint, ICloneable
    {
        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>The account.</value>
        [JsonProperty("account")]
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>The token.</value>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the secondary token.
        /// </summary>
        /// <value>The secondary token.</value>
        [JsonProperty("secondaryToken")]
        public string SecondaryToken { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new ApiEndpoint
            {
                Account = Account,
                Host = Host,
                Path = Path,
                Port = Port,
                Protocol = Protocol,
                SecondaryToken = SecondaryToken,
                Token = Token
            };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0}api/", base.ToString());
        }

        /// <summary>
        /// To the URI.
        /// </summary>
        /// <returns></returns>
        public override Uri ToUri()
        {
            return new Uri(ToString());
        }

        /// <summary>
        /// To the URI.
        /// </summary>
        /// <param name="appendApiSuffix">if set to <c>true</c> [append API suffix].</param>
        /// <returns></returns>
        public Uri ToUri(bool appendApiSuffix)
        {
            return appendApiSuffix ? ToUri() : new Uri(base.ToString());
        }
    }
}