﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beyova.ProgrammingIntelligence;
using Newtonsoft.Json.Linq;

namespace Beyova.Configuration
{
    /// <summary>
    /// Class BaseJsonConfigurationReader.
    /// </summary>
    /// <seealso cref="Beyova.IConfigurationReader" />
    public abstract class BaseJsonConfigurationReader : IConfigurationReader
    {
        /// <summary>
        /// Class ConfigurationItem.
        /// </summary>
        protected internal class RuntimeConfigurationItem
        {
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>The value.</value>
            public object Value { get; set; }

            /// <summary>
            /// Gets or sets the assembly.
            /// </summary>
            /// <value>The assembly.</value>
            public string Assembly { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the environment.
            /// </summary>
            /// <value>The environment.</value>
            public string Environment { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="RuntimeConfigurationItem"/> is encrypted.
            /// </summary>
            /// <value><c>true</c> if encrypted; otherwise, <c>false</c>.</value>
            public bool Encrypted { get; set; }

            /// <summary>
            /// Gets or sets the minimum component version required.
            /// </summary>
            /// <value>The minimum component version required.</value>
            public string MinComponentVersionRequired { get; set; }

            /// <summary>
            /// Gets or sets the maximum component version limited.
            /// </summary>
            /// <value>The maximum component version limited.</value>
            public string MaxComponentVersionLimited { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
            public bool IsActive { get; set; }
        }

        #region Constants

        /// <summary>
        /// The configuration key primary SQL connection
        /// </summary>
        private const string ConfigurationKey_PrimarySqlConnection = "PrimarySqlConnection";

        #endregion Constants

        /// <summary>
        /// The settings
        /// </summary>
        protected Dictionary<string, RuntimeConfigurationItem> _settings = null;

        /// <summary>
        /// Gets the settings count.
        /// </summary>
        /// <value>The settings count.</value>
        public int Count
        {
            get
            {
                return _settings == null ? 0 : _settings.Count;
            }
        }

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <value>The hash.</value>
        public string Hash
        {
            get
            {
                if (_settings.Keys.HasItem())
                {
                    List<Byte[]> hashValues = new List<byte[]>(_settings.Count * 2);
                    Parallel.ForEach(_settings, x =>
                    {
                        hashValues.Add(Encoding.UTF8.GetBytes(x.Key).ToMD5Bytes());
                        hashValues.Add(Encoding.UTF8.GetBytes(x.Value.Value.ToString()).ToMD5Bytes());
                    });

                    var resultBytes = (new byte[16]).ByteWiseSumWith(hashValues.ToArray());
                    return resultBytes.ToHex();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets the SQL connection.
        /// </summary>
        /// <value>The SQL connection.</value>
        public string SqlConnection
        {
            get { return GetConfiguration(ConfigurationKey_PrimarySqlConnection); }
        }

        /// <summary>
        /// Gets the primary SQL connection.
        /// </summary>
        /// <value>
        /// The primary SQL connection.
        /// </value>
        public string PrimarySqlConnection
        {
            get { return GetConfiguration(ConfigurationKey_PrimarySqlConnection); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConfigurationReader" /> class.
        /// </summary>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        protected BaseJsonConfigurationReader(bool throwException = false)
        {
            _settings = Initialize(throwException);
        }

        #region Public method

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>T.</returns>
        public T GetConfiguration<T>(string key, T defaultValue = default(T))
        {
            RuntimeConfigurationItem configuration = null;

            if (_settings.SafeTryGetValue(key, out configuration) && configuration.IsActive)
            {
                return (T)configuration.Value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets the configuration as object.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Object.</returns>
        protected object GetConfigurationAsObject(string key, object defaultValue = null)
        {
            RuntimeConfigurationItem configuration = null;
            return (_settings.SafeTryGetValue(key, out configuration) && configuration.IsActive) ? configuration.Value : defaultValue;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>System.String.</returns>
        public string GetConfiguration(string key, string defaultValue = null)
        {
            return GetConfigurationAsObject(key, defaultValue).SafeToString();
        }

        #endregion Public method

        #region Initialization

        /// <summary>
        /// Initializes the specified throw exception.
        /// </summary>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        /// <returns>Dictionary&lt;System.String, ConfigurationItem&gt;.</returns>
        protected abstract Dictionary<string, RuntimeConfigurationItem> Initialize(bool throwException = false);

        /// <summary>
        /// Fills the object collection.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="componentAttribute">The component attribute.</param>
        /// <param name="itemNode">The item node.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="configurationSourceName">Name of the configuration source.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        protected static void FillObjectCollection(IDictionary<string, RuntimeConfigurationItem> container, BeyovaComponentAttribute componentAttribute, JProperty itemNode, string assemblyName, string configurationSourceName, string environment, bool throwException = false)
        {
            try
            {
                container.CheckNullObject(nameof(container));
                itemNode.CheckNullObject(nameof(itemNode));

                FillObjectCollection(
                       container,
                       componentAttribute?.UnderlyingObject.Version,
                       itemNode.Name,
                       itemNode.Value.ToObject<ConfigurationRawItem>(),
                       assemblyName,
                       configurationSourceName,
                       environment,
                       throwException);
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw ex.Handle(data: new { itemNode, assemblyName, configurationSourceName, environment });
                }
            }
        }

        /// <summary>
        /// Fills the object collection.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="coreComponentVersion">The core component version.</param>
        /// <param name="key">The key.</param>
        /// <param name="configurationRawItem">The configuration raw item.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="configurationSourceName">Name of the configuration source.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        protected static void FillObjectCollection(IDictionary<string, RuntimeConfigurationItem> container, string coreComponentVersion, string key, ConfigurationRawItem configurationRawItem, string assemblyName, string configurationSourceName, string environment, bool throwException = false)
        {
            try
            {
                container.CheckNullObject(nameof(container));
                configurationRawItem.CheckNullObject(nameof(configurationRawItem));

                var typeFullName = configurationRawItem.Type;
                var encrypted = (configurationRawItem.Encrypted) ?? false;
                var minVersion = configurationRawItem.MinComponentVersionRequired;
                var maxVersion = configurationRawItem.MaxComponentVersionLimited;

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(typeFullName))
                {
                    var objectType = ReflectionExtension.SmartGetType(typeFullName, false) ?? ReflectionExtension.SmartGetType(typeFullName, true);

                    if (objectType != null)
                    {
                        var valueJsonObject = encrypted ? DecryptObject(configurationRawItem.Value.ToObject<string>()) : configurationRawItem.Value;
                        var valueObject = valueJsonObject.ToObject(objectType);
                        container.Merge(key, new RuntimeConfigurationItem
                        {
                            Value = valueObject,
                            IsActive = IsActive(coreComponentVersion, minVersion, maxVersion),
                            Assembly = assemblyName,
                            MaxComponentVersionLimited = maxVersion,
                            MinComponentVersionRequired = minVersion,
                            Encrypted = encrypted,
                            Environment = environment,
                            Name = configurationSourceName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if (throwException)
                {
                    throw ex.Handle(data: new { coreComponentVersion, key, configurationRawItem, assemblyName, configurationSourceName, environment });
                }
            }
        }

        /// <summary>
        /// Initializes the settings.
        /// </summary>
        /// <param name="settingContainer">The setting container.</param>
        /// <param name="componentAttribute">The component attribute.</param>
        /// <param name="configurationDetail">The configuration detail.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="throwException">if set to <c>true</c> [throw exception].</param>
        protected virtual void InitializeSettings(IDictionary<string, RuntimeConfigurationItem> settingContainer, BeyovaComponentAttribute componentAttribute, ConfigurationDetail configurationDetail, string assemblyName, bool throwException = false)
        {
            settingContainer.Clear();

            try
            {
                configurationDetail.CheckNullObject(nameof(configurationDetail));

                var name = configurationDetail.Name.SafeToString(configurationDetail.Version);
                var environment = configurationDetail.Environment;

                foreach (JProperty one in configurationDetail.Configurations.Children())
                {
                    FillObjectCollection(settingContainer, componentAttribute, one, assemblyName, name, environment, throwException);
                }
            }
            catch (Exception ex)
            {
                var exception = ex.Handle(new { configurationDetail, assemblyName });
                if (throwException)
                {
                    throw exception;
                }
            }
        }

        /// <summary>
        /// Decrypts the object.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>Newtonsoft.Json.Linq.JToken.</returns>
        protected static JToken DecryptObject(string jsonString)
        {
            return string.IsNullOrWhiteSpace(jsonString) ? null : jsonString.DecryptR3DES();
        }

        /// <summary>
        /// Encrypts the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>System.String.</returns>
        protected static string EncryptObject(object obj)
        {
            return obj == null ? null : obj.ToJson(false).EncryptR3DES();
        }

        /// <summary>
        /// Determines whether the specified version value is active.
        /// </summary>
        /// <param name="versionValue">The version value.</param>
        /// <param name="minRequired">The minimum required.</param>
        /// <param name="maxLimited">The maximum limited.</param>
        /// <returns>System.Boolean.</returns>
        protected static bool IsActive(string versionValue, string minRequired, string maxLimited)
        {
            Version version = null, min = null, max = null;

            try
            {
                version = string.IsNullOrWhiteSpace(versionValue) ? null : new Version(versionValue);
                min = string.IsNullOrWhiteSpace(minRequired) ? null : new Version(minRequired);
                max = string.IsNullOrWhiteSpace(maxLimited) ? null : new Version(maxLimited);
            }
            catch { }

            if (version == null)
            {
                return min == null && max == null;
            }
            else
            {
                if ((min != null && min > version)
                    || (max != null && max < version))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion Initialization

        /// <summary>
        /// Refreshes the settings.
        /// </summary>
        public abstract void RefreshSettings();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        public IEnumerable<KeyValuePair<string, object>> GetValues()
        {
            var result = new Dictionary<string, object>();

            _settings.Where(result, (k, v) => { return v?.IsActive ?? false; }, x => x.Value);
            return result;
        }
    }
}