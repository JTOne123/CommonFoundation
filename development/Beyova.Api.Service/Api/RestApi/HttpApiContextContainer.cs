﻿using System;
using System.Collections.Generic;
using System.IO;
using Beyova.ApiTracking;
using Beyova.ExceptionSystem;
using Beyova.Http;

namespace Beyova.Api.RestApi
{
    /// <summary>
    /// Class HttpContextContainer
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public abstract class HttpApiContextContainer<TRequest, TResponse> : HttpContextContainer<TRequest, TResponse>, IHttpResponseActions, IHttpRequestCookieActions
    {
        #region Runtime Context properties

        /// <summary>
        /// Gets the runtime context.
        /// </summary>
        /// <value>
        /// The runtime context.
        /// </value>
        public RuntimeContext RuntimeContext { get; internal set; }

        /// <summary>
        /// Gets or sets the entry stamp.
        /// </summary>
        /// <value>
        /// The entry stamp.
        /// </value>
        public DateTime? EntryStamp { get; set; }

        /// <summary>
        /// Gets or sets the exit stamp.
        /// </summary>
        /// <value>
        /// The exit stamp.
        /// </value>
        public DateTime? ExitStamp { get; set; }

        /// <summary>
        /// Gets or sets the API event.
        /// </summary>
        /// <value>
        /// The API event.
        /// </value>
        public ApiEventLog ApiEvent { get; protected set; }

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public RestApiSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the trace identifier.
        /// </summary>
        /// <value>
        /// The trace identifier.
        /// </value>
        public string TraceId { get; set; }

        /// <summary>
        /// Gets or sets the trace sequence.
        /// </summary>
        /// <value>
        /// The trace sequence.
        /// </value>
        public int? TraceSequence { get; set; }

        /// <summary>
        /// Gets or sets the base exception.
        /// </summary>
        /// <value>
        /// The base exception.
        /// </value>
        public BaseException BaseException { get; set; }

        #endregion Runtime Context properties

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpApiContextContainer{TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="options">The options.</param>
        protected HttpApiContextContainer(TRequest request, TResponse response, HttpContextOptions<TRequest> options)
            : base(request, response, options)
        {
        }

        /// <summary>
        /// Collects the tracking information.
        /// </summary>
        public void CollectTrackingInfo(long bodyLength)
        {
            if (this.Settings != null && this.Settings.ApiTracking != null && this.Settings.TrackingEvent && this.RuntimeContext != null && !this.RuntimeContext.OmitApiEvent)
            {
                this.ApiEvent = new ApiEventLog
                {
                    RawUrl = this.RawUrl,
                    EntryStamp = this.EntryStamp,
                    UserAgent = this.UserAgent,
                    TraceId = this.TraceId,
                    // If request came from ApiTransport or other proxy ways, ORIGINAL stands for the IP ADDRESS from original requester.
                    IpAddress = this.TryGetRequestHeader(this.Settings?.OriginalIpAddressHeaderKey.SafeToString(HttpConstants.HttpHeader.ORIGINAL)).SafeToString(this.ClientIpAddress),
                    CultureCode = this.UserLanguages.SafeFirstOrDefault(),
                    ContentLength = bodyLength,
                    OperatorCredential = ContextHelper.CurrentCredential as BaseCredential,
                    Protocol = this.NetworkProtocol,
                    ServiceIdentifier = RuntimeContext.ApiInstance?.GetType()?.Name,
                    ApiFullName = this.RuntimeContext.ApiMethod?.Name,
                    ResourceName = this.RuntimeContext.ResourceName,
                    ServerIdentifier = EnvironmentCore.ServerName,
                    ModuleName = this.RuntimeContext.OperationParameters.ModuleName,
                    ReferrerUrl = this.TryGetRequestHeader(HttpConstants.HttpHeader.Referrer),
                    ResourceEntityKey = this.RuntimeContext.IsActionUsed ? this.RuntimeContext.Parameter2 : this.RuntimeContext.Parameter1
                };

                var clientIdentifierHeaderKey = this.Settings.ClientIdentifierHeaderKey;
                if (!string.IsNullOrWhiteSpace(clientIdentifierHeaderKey))
                {
                    this.ApiEvent.ClientIdentifier = this.TryGetRequestHeader(clientIdentifierHeaderKey);
                }
            }

            if (!string.IsNullOrWhiteSpace(this.TraceId))
            {
                ApiTraceContext.Initialize(this.TraceId, this.TraceSequence, this.EntryStamp);
            }
        }
    }
}