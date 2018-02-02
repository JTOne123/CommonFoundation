﻿using System;

namespace Beyova
{
    /// <summary>
    /// Class BeyovaComponentInfo.
    /// </summary>
    public class BeyovaComponentInfo : Attribute
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public string Version { get; protected set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; protected set; }

        /// <summary>
        /// Gets or sets the type of the API tracking.
        /// </summary>
        /// <value>The type of the API tracking.</value>
        public Type ApiTrackingType { get; protected set; }

        /// <summary>
        /// Gets or sets the retired stamp.
        /// </summary>
        /// <value>
        /// The retired stamp.
        /// </value>
        internal DateTime? RetiredStamp { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeyovaComponentInfo" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="apiTrackingType">Type of the API tracking, which should implement <see cref="IApiTracking" /> with 0 parameter constructor.</param>
        /// <param name="retiredStamp">The retired stamp.</param>
        public BeyovaComponentInfo(string id, string version, Type apiTrackingType = null, DateTime? retiredStamp = null)
        {
            this.Version = version;
            this.Id = id;
            this.ApiTrackingType = apiTrackingType;
            this.RetiredStamp = retiredStamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BeyovaComponentAttribute"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="version">The version.</param>
        /// <param name="retiredStamp">The retired stamp.</param>
        public BeyovaComponentInfo(string id, string version, DateTime retiredStamp) : this(id, version, null, retiredStamp)
        {
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Id.SafeToString("Not specified"), Version);
        }

        /// <summary>
        /// Gets the API tracking instance.
        /// </summary>
        /// <returns>IApiTracking.</returns>
        internal IApiTracking GetApiTrackingInstance()
        {
            if (this.ApiTrackingType != null)
            {
                try
                {
                    return Activator.CreateInstance(this.ApiTrackingType) as IApiTracking;
                }
                catch { }
            }

            return null;
        }
    }
}