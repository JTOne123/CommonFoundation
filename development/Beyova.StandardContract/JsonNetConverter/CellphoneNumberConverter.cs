﻿using System;
using Newtonsoft.Json;

namespace Beyova
{
    /// <summary>
    ///
    /// </summary>
    public class CellphoneNumberConverter : JsonConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellphoneNumberConverter"/> class.
        /// </summary>
        public CellphoneNumberConverter() : base() { }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CellphoneNumber);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value == null ? null : CellphoneNumber.FromString(reader.Value.ToString());
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var cellphoneNumber = value as CellphoneNumber;
            if (cellphoneNumber != null)
            {
                writer.WriteValue(cellphoneNumber.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}