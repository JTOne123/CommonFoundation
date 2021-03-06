﻿namespace Beyova
{
    /// <summary>
    /// Class AccessCredential.
    /// </summary>
    public class AccessCredential
    {
        /// <summary>
        /// Gets or sets the access identifier.
        /// </summary>
        /// <value>The access identifier.</value>
        public string AccessIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>The domain.</value>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>The token.</value>
        public string Token { get; set; }

        /// <summary>
        /// Parses the specified credential string.
        /// </summary>
        /// <param name="credentialString">The credential string.</param>
        /// <returns>AccessCredential.</returns>
        public static AccessCredential Parse(string credentialString)
        {
            return credentialString.ParseToAccessCredential();
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="AccessCredential"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator string(AccessCredential credential)
        {
            return credential.ToCredentialString();
        }

        /// <summary>
        /// Performs an explicit conversion from <see cref="System.String"/> to <see cref="AccessCredential"/>.
        /// </summary>
        /// <param name="credential">The credential.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator AccessCredential(string credential)
        {
            return AccessCredential.Parse(credential);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToCredentialString();
        }
    }
}