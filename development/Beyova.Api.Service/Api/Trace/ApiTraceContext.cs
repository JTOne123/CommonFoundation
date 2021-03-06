﻿using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Beyova.Api.RestApi;
using Beyova.Diagnostic;
using Beyova.Http;

namespace Beyova.Api
{
    /// <summary>
    /// Class ApiTraceContext.
    /// </summary>
    public static partial class ApiTraceContext
    {
        /// <summary>
        /// The _current
        /// </summary>
        [ThreadStatic]
        private static ApiTraceStep _current;

        /// <summary>
        /// The _root
        /// </summary>
        [ThreadStatic]
        private static ApiTraceLog _root;

        /// <summary>
        /// The debuggable trace identifier. It is set by Gravity protocol.
        /// </summary>
        [ThreadStatic]
        internal static string DebuggableTraceId = null;

        /// <summary>
        /// Gets a value indicating whether [debug current].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [debug current]; otherwise, <c>false</c>.
        /// </value>
        private static bool DebugCurrent
        {
            get
            {
                return DebuggableTraceId.SafeEquals(_root?.TraceId);
            }
        }

        /// <summary>
        /// Gets the root.
        /// </summary>
        /// <value>The root.</value>
        internal static ApiTraceLog Root { get { return _root; } }

        /// <summary>
        /// Gets the trace identifier.
        /// </summary>
        /// <value>The trace identifier.</value>
        internal static string TraceId { get { return _root?.TraceId; } }

        /// <summary>
        /// Gets the trace sequence.
        /// </summary>
        /// <value>The trace sequence.</value>
        internal static int? TraceSequence { get { return _root?.TraceSequence; } }

        /// <summary>
        /// Initializes the specified trace identifier.
        /// </summary>
        /// <param name="traceId">The trace identifier.</param>
        /// <param name="traceSequence">The trace sequence.</param>
        /// <param name="entryStamp">The entry stamp.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void Initialize(string traceId, int? traceSequence, DateTime? entryStamp = null, [CallerMemberName] string methodName = null)
        {
            if (!string.IsNullOrWhiteSpace(traceId))
            {
                if (!entryStamp.HasValue)
                {
                    entryStamp = DateTime.UtcNow;
                }

                _root = new ApiTraceLog()
                {
                    TraceId = traceId,
                    TraceSequence = traceSequence.HasValue ? (traceSequence.Value + 1) : 0,
                    EntryStamp = entryStamp
                };
                var current = new ApiTraceStep(_root, methodName, entryStamp);
                _root.InnerTraces.Add(current);
                _current = current;
            }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public static void Dispose()
        {
            _current = null;
            _root = null;
        }

        /// <summary>
        /// Gets the current trace log.
        /// </summary>
        /// <param name="dispose">if set to <c>true</c> [dispose].</param>
        /// <returns>ApiTraceLog.</returns>
        public static ApiTraceLog GetCurrentTraceLog(bool dispose = true)
        {
            try
            {
                if (_root != null)
                {
                    _root.ExitStamp = _root.InnerTraces.Last().ExitStamp;
                }
                return _root;
            }
            catch (Exception ex)
            {
                throw ex.Handle(dispose);
            }
            finally
            {
                if (dispose)
                {
                    Dispose();
                }
            }
        }

        #region Enter

        /// <summary>
        /// Enters the specified method.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="setNameAsMajor">The set name as major.</param>
        public static void Enter(string prefix = null, [CallerMemberName] string methodName = null, bool setNameAsMajor = false)
        {
            Enter(new ApiTraceStep(_current, string.IsNullOrWhiteSpace(prefix) ? methodName : string.Format("{0}.{1}", prefix, methodName), DateTime.UtcNow), setNameAsMajor);
        }

        /// <summary>
        /// Enters the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entryStamp">The entry stamp.</param>
        /// <param name="setNameAsMajor">The set name as major.</param>
        internal static void Enter(RuntimeContext context, DateTime? entryStamp = null, bool setNameAsMajor = false)
        {
            Enter(context.ToApiTraceStep(_current, entryStamp ?? DateTime.UtcNow), setNameAsMajor);
        }

        ///// <summary>
        ///// Enters the specified method call information.
        ///// </summary>
        ///// <param name="methodCallInfo">The method call information.</param>
        ///// <param name="entryStamp">The entry stamp.</param>
        ///// <param name="setNameAsMajor">if set to <c>true</c> [set name as major].</param>
        //internal static void Enter(MethodCallInfo methodCallInfo, DateTime? entryStamp = null, bool setNameAsMajor = false)
        //{
        //    Enter(methodCallInfo.ToTraceLog(_current, entryStamp ?? DateTime.UtcNow), setNameAsMajor);
        //}

        /// <summary>
        /// Enters the specified trace log.
        /// </summary>
        /// <param name="traceStep">The trace step.</param>
        /// <param name="setNameAsMajor">The set name as major.</param>
        internal static void Enter(ApiTraceStep traceStep, bool setNameAsMajor = false)
        {
            if (traceStep != null)
            {
                if (_root != null)
                {
                    traceStep.Parent = _current;
                    _current.InnerTraces.Add(traceStep);
                    _current = traceStep;

                    if (setNameAsMajor && !string.IsNullOrWhiteSpace(_current.MethodFullName))
                    {
                        _root.MethodFullName = _current.MethodFullName;
                    }
                }
            }
        }

        #endregion Enter

        /// <summary>
        /// Sets the name of the major method.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        internal static void SetMajorMethodName(string methodName)
        {
            if (_root != null && !string.IsNullOrWhiteSpace(methodName)) { }
            {
                _root.MethodFullName = methodName;
            }
        }

        /// <summary>
        /// Exits the specified exception.
        /// </summary>
        /// <param name="exceptionKey">The exception key.</param>
        /// <param name="exitStamp">The exit stamp.</param>
        public static void Exit(Guid? exceptionKey, DateTime? exitStamp = null)
        {
            Exit(_current, exceptionKey, exitStamp);
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public static void WriteLine(string format, params string[] args)
        {
            _current?.DebugInfo?.WriteLine(format, args);
        }

        /// <summary>
        /// Writes the HTTP request raw.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        public static void WriteHttpRequestRaw(HttpRequestRaw httpRequest)
        {
            if (DebugCurrent)
            {
                var debugInfo = _current?.DebugInfo;

                if (debugInfo != null)
                {
                    debugInfo.HttpRequestRaw = httpRequest?.ToString();
                }
            }
        }

        /// <summary>
        /// Writes the HTTP request raw.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        public static void WriteHttpRequestRaw(HttpRequestMessage httpRequest)
        {
            if (DebugCurrent && httpRequest != null)
            {
                var debugInfo = _current?.DebugInfo;

                if (debugInfo != null)
                {
                    debugInfo.HttpRequestRaw = httpRequest.ToString();
                }
            }
        }

        /// <summary>
        /// Writes the HTTP response raw.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">The response.</param>
        public static void WriteHttpResponseRaw<T>(HttpActionResult<T> response)
        {
            if (DebugCurrent && response != null)
            {
                _current?.DebugInfo?.WriteHttpResponseRaw(response.Headers, response.Body.SafeToString());
            }
        }

        /// <summary>
        /// Fills the exit information.
        /// </summary>
        /// <param name="step">The piece.</param>
        /// <param name="exceptionKey">The exception key.</param>
        /// <param name="exitStamp">The exit stamp.</param>
        private static void Exit(ApiTraceStep step, Guid? exceptionKey, DateTime? exitStamp = null)
        {
            if (step != null)
            {
                step.ExceptionKey = exceptionKey;
                step.ExitStamp = exitStamp ?? DateTime.UtcNow;
                _current = step.Parent;
            }

            if (_root != null && !_root.ExceptionKey.HasValue)
            {
                _root.ExceptionKey = exceptionKey;
            }
        }
    }
}