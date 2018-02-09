﻿using System;
using System.Collections.Generic;

namespace Beyova
{
    /// <summary>
    /// Interface IConfigurationReader
    /// </summary>
    public interface IConfigurationReader
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }

        /// <summary>
        /// Gets the primary SQL connection.
        /// </summary>
        /// <value>
        /// The primary SQL connection.
        /// </value>
        string PrimarySqlConnection { get; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>T.</returns>
        T GetConfiguration<T>(string key, T defaultValue = default(T));

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>System.String.</returns>
        string GetConfiguration(string key, string defaultValue = null);

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <returns>IEnumerable&lt;KeyValuePair&lt;System.String, System.Object&gt;&gt;.</returns>
        IEnumerable<KeyValuePair<string, object>> GetValues();

        /// <summary>
        /// Refreshes the settings.
        /// </summary>
        void RefreshSettings();
    }
}