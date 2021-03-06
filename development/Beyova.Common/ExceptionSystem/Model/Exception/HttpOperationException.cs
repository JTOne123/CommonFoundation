﻿using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace Beyova.Diagnostic
{
    /// <summary>
    /// Class HttpOperationException.
    /// </summary>
    public class HttpOperationException : BaseException
    {
        /// <summary>
        /// Class HttpExceptionReference.
        /// </summary>
        public class HttpExceptionReference
        {
            /// <summary>
            /// Gets or sets the destination URL.
            /// </summary>
            /// <value>The destination URL.</value>
            public string DestinationUrl { get; set; }

            /// <summary>
            /// Gets or sets the HTTP method.
            /// </summary>
            /// <value>The HTTP method.</value>
            public string HttpMethod { get; set; }

            /// <summary>
            /// Gets or sets the web exception status.
            /// </summary>
            /// <value>The web exception status.</value>
            public WebExceptionStatus? WebExceptionStatus { get; set; }

            /// <summary>
            /// Gets or sets the response text.
            /// </summary>
            /// <value>The response text.</value>
            public string ResponseText { get; set; }

            /// <summary>
            /// Gets or sets the HTTP status code.
            /// </summary>
            /// <value>The HTTP status code.</value>
            public HttpStatusCode? HttpStatusCode { get; set; }
        }

        /// <summary>
        /// Gets the exception reference.
        /// </summary>
        /// <value>The exception reference.</value>
        public HttpExceptionReference ExceptionReference
        {
            get
            {
                return ReferenceData == null ? null : ReferenceData.ToObject<HttpExceptionReference>();
            }
        }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpOperationException" /> class. For broken requests.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="message">The message.</param>
        /// <param name="responseText">The response text.</param>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="webExceptionStatus">The web exception status.</param>
        /// <param name="serverIdentifier">The server identifier.</param>
        public HttpOperationException(string destinationUrl, string httpMethod, string message, string responseText, HttpStatusCode? httpStatusCode, WebExceptionStatus? webExceptionStatus, string serverIdentifier = null)
            : base(string.Format("Failed to request destination URL [{0}] using method [{1}]. Respond within code [{2}], status [{3}], message: [{4}]. [{5}]", destinationUrl, httpMethod, httpStatusCode.HasValue ? httpStatusCode.Value : 0, webExceptionStatus.ToString(), message, string.IsNullOrWhiteSpace(serverIdentifier) ? string.Empty : "Machine Name: " + serverIdentifier),
                  httpStatusCode.ConvertHttpStatusCodeToExceptionCode(webExceptionStatus),
                  data: new HttpExceptionReference
                  {
                      DestinationUrl = destinationUrl,
                      HttpMethod = httpMethod,
                      WebExceptionStatus = webExceptionStatus,
                      ResponseText = responseText,
                      HttpStatusCode = httpStatusCode
                  })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpOperationException" /> class.
        /// </summary>
        /// <param name="destinationUrl">The destination URL.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="serverIdentifier">The server identifier.</param>
        public HttpOperationException(string destinationUrl, string httpMethod, HttpStatusCode? httpStatusCode, BaseException innerException, string serverIdentifier = null)
                    : base(string.Format("Failed to request destination URL [{0}] using method [{1}]. Responed within code [{2}]. [{3}]", destinationUrl, httpMethod, (int)httpStatusCode, string.IsNullOrWhiteSpace(serverIdentifier) ? string.Empty : "Machine Name: " + serverIdentifier),
                          new ExceptionCode { Major = ExceptionCode.MajorCode.OperationFailure, Minor = innerException?.Code.Minor }, innerException: innerException,
                          data: new HttpExceptionReference
                          {
                              DestinationUrl = destinationUrl,
                              HttpMethod = httpMethod,
                              WebExceptionStatus = null,
                              ResponseText = null,
                              HttpStatusCode = httpStatusCode
                          })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpOperationException" /> class. For restore from <c>ExceptionInfo</c>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="createdStamp">The created stamp.</param>
        /// <param name="message">The message.</param>
        /// <param name="scene">The scene.</param>
        /// <param name="code">The code.</param>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="operatorCredential">The operator credential.</param>
        /// <param name="data">The data.</param>
        /// <param name="hint">The hint.</param>
        internal HttpOperationException(Guid key, DateTime createdStamp, string message, ExceptionScene scene, ExceptionCode code, Exception innerException, BaseCredential operatorCredential, JToken data, FriendlyHint hint)
          : base(key, createdStamp, message, scene, code, innerException, operatorCredential, data, hint)
        {
        }

        #endregion Constructor
    }
}