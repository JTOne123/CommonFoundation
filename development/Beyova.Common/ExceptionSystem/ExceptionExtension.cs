﻿using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Beyova.Diagnostic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Beyova
{
    /// <summary>
    /// Class ExceptionExtension.
    /// </summary>
    public static partial class ExceptionExtension
    {
        private const char indentChar = '-';

        /// <summary>
        /// To the HTTP operation exception.
        /// </summary>
        /// <param name="webException">The web exception.</param>
        /// <param name="httpWebRequest">The HTTP web request.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        public static HttpOperationException ToHttpOperationException(this WebException webException, HttpWebRequest httpWebRequest, out CookieCollection cookies, out WebHeaderCollection headers)
        {
            HttpOperationException result = null;
            cookies = null;
            headers = null;

            if (webException != null)
            {
                var webResponse = (HttpWebResponse)webException.Response;
                headers = webResponse.Headers;
                var destinationMachine = headers?.Get(HttpConstants.HttpHeader.SERVERNAME);
                cookies = webResponse.Cookies;

                var responseText = webResponse.ReadAsText();

                result = new HttpOperationException(httpWebRequest.RequestUri.ToString(),
                    httpWebRequest.Method,
                    webException.Message,
                    responseText,
                    webResponse.StatusCode,
                    webException.Status, destinationMachine);
            }

            return result;
        }

        /// <summary>
        /// Converts the exception to base exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="data">The data.</param>
        /// <param name="friendlyHint">The friendly hint.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        /// <returns>
        /// BaseException.
        /// </returns>
        public static BaseException ConvertExceptionToBaseException(this Exception exception, object data = null, FriendlyHint friendlyHint = null,
            [CallerMemberName] string memberName = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return exception == null ? null : ((exception as BaseException) ?? exception.Handle(new ExceptionScene
            {
                FilePath = sourceFilePath,
                LineNumber = sourceLineNumber,
                MethodName = memberName
            }, data, hint: friendlyHint));
        }

        /// <summary>
        /// Roots the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>Exception.</returns>
        public static Exception RootException(this Exception exception)
        {
            if (exception != null)
            {
                return exception.InnerException == null ? exception : exception.InnerException.RootException() as Exception;
            }

            return null;
        }

        /// <summary>
        /// Formats to string.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>System.String.</returns>
        public static string FormatToString(this Exception exception)
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (exception != null)
            {
                stringBuilder.AppendLine("-----------------------  Exception  -----------------------");
                stringBuilder.AppendLine("-----------------------  " + DateTime.UtcNow.ToFullDateTimeString());
                stringBuilder.AppendLine("-----------------------  Thread ID: " + Thread.CurrentThread.ManagedThreadId.ToString());

                FormatToString(stringBuilder, exception, 0);

                stringBuilder.AppendLine("---------------------------  End  ---------------------------");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Formats to string.
        /// </summary>
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="level">The level.</param>
        private static void FormatToString(StringBuilder stringBuilder, Exception exception, int level)
        {
            if (stringBuilder != null && exception != null)
            {
                BaseException baseException = exception as BaseException;

                stringBuilder.AppendLineWithFormat(level, "Exception Type: {0}", exception.GetType().ToString());

                if (baseException != null)
                {
                    stringBuilder.AppendLineWithFormat(level, "Exception Code: {0}({1})", baseException.Code.ToString(), (int)baseException.Code);
                }

                stringBuilder.AppendLineWithFormat(level, "Exception Message: {0}", exception.Message);
                stringBuilder.AppendLineWithFormat(level, "Source: {0}", exception.Source);
                stringBuilder.AppendLineWithFormat(level, "Site: {0}", exception.TargetSite);
                stringBuilder.AppendLineWithFormat(level, "StackTrace: {0}", exception.StackTrace);

                if (baseException != null)
                {
                    stringBuilder.AppendLineWithFormat(level, "Exception Code: {0}({1})", baseException.Code.ToString(), (int)baseException.Code);
                    stringBuilder.AppendLineWithFormat(level, "Operator Credential: {0}", baseException.OperatorCredential.ToJson());
                    stringBuilder.AppendLineWithFormat(level, "Scene: {0}", baseException.Scene.ToJson());
                    stringBuilder.AppendLineWithFormat(level, "Hint: {0}", baseException.Hint.ToJson());
                }

                stringBuilder.AppendLineWithFormat(level, "Data Reference: {0}", GenerateDataString(baseException?.ReferenceData?.ToJson()));

                if (exception.InnerException != null)
                {
                    level++;
                    stringBuilder.AppendLine(level, "--------------------  Inner Exception  --------------------");
                    FormatToString(stringBuilder, exception.InnerException, level);
                }
            }
        }

        /// <summary>
        /// Appends the indent.
        /// Returns  the same <see cref="StringBuilder"/> instance to make sure it supports chain based actions.
        /// </summary>
        /// <param name="stringBuilder">The string builder.</param>
        /// <param name="level">The level.</param>
        /// <returns>System.Text.StringBuilder.</returns>
        private static StringBuilder AppendIndent(this StringBuilder stringBuilder, int level)
        {
            if (stringBuilder != null)
            {
                stringBuilder.AppendIndent(indentChar, level * 2);
                stringBuilder.Append(" ");
            }

            return stringBuilder;
        }

        #region GenerateDataString

        /// <summary>
        /// Generates the data string.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>System.String.</returns>
        private static string GenerateDataString(object obj)
        {
            string result = "<null>";

            if (obj != null)
            {
                result = JsonConvert.SerializeObject(obj, JsonExtension.SafeJsonSerializationSettings);
            }

            return result;
        }

        #endregion GenerateDataString

        #region Handle exception

        /// <summary>
        /// Handles the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="data">The data.</param>
        /// <param name="hint">The hint.</param>
        /// <param name="minorCode">The minor code.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        /// <returns>
        /// Beyova.Diagnostic.BaseException.
        /// </returns>
        public static BaseException Handle(this Exception exception, object data = null, FriendlyHint hint = null, string minorCode = null,
                    [CallerMemberName] string operationName = null,
                    [CallerFilePath] string sourceFilePath = null,
                    [CallerLineNumber] int sourceLineNumber = 0)
        {
            return Handle(exception, new ExceptionScene
            {
                MethodName = operationName,
                FilePath = sourceFilePath,
                LineNumber = sourceLineNumber
            }, data, hint, minorCode);
        }

        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="scene">The scene.</param>
        /// <param name="data">The data.</param>
        /// <param name="hint">The hint.</param>
        /// <param name="minorCode">The minor code.</param>
        /// <returns>
        /// BaseServiceException.
        /// </returns>
        internal static BaseException Handle(this Exception exception, ExceptionScene scene, object data = null, FriendlyHint hint = null, string minorCode = null)
        {
            TargetInvocationException targetInvocationException = exception as TargetInvocationException;

            if (targetInvocationException != null)
            {
                return targetInvocationException.InnerException.Handle(scene, data, hint, minorCode);
            }
            else
            {
                var baseException = exception as BaseException;
                var operationName = scene?.MethodName;

                if (baseException != null)
                {
                    if (string.IsNullOrWhiteSpace(operationName))
                    {
                        return baseException;
                    }
                    else
                    {
                        switch (baseException.Code.Major)
                        {
                            case ExceptionCode.MajorCode.UnauthorizedOperation:
                                return new UnauthorizedOperationException(baseException, minorCode.SafeToString(baseException.Code.Minor), data, scene: scene) as BaseException;

                            case ExceptionCode.MajorCode.OperationForbidden:
                                return new OperationForbiddenException(operationName, minorCode.SafeToString(baseException.Code?.Minor), baseException, data, scene: scene) as BaseException;

                            case ExceptionCode.MajorCode.NullOrInvalidValue:
                            case ExceptionCode.MajorCode.DataConflict:
                            case ExceptionCode.MajorCode.NotImplemented:
                            case ExceptionCode.MajorCode.ResourceNotFound:
                            case ExceptionCode.MajorCode.CreditNotAfford:
                            case ExceptionCode.MajorCode.ServiceUnavailable:
                                return baseException;

                            default:
                                break;
                        }
                    }
                }
                else
                {
                    var sqlException = exception as SqlException;

                    if (sqlException != null)
                    {
                        return new OperationFailureException(exception, data, hint: hint, scene: scene, minor: minorCode) as BaseException;
                    }
                    else
                    {
                        var notImplementException = exception as NotImplementedException;

                        if (notImplementException != null)
                        {
                            return new UnimplementedException(operationName, notImplementException);
                        }
                    }
                }

                return new OperationFailureException(exception, data, scene: scene, minor: minorCode) as BaseException;
            }
        }

        #endregion Handle exception

        /// <summary>
        /// To the exception information.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="key">The key.</param>
        /// <param name="eventKey">The event key.</param>
        /// <returns>
        /// ExceptionInfo.
        /// </returns>
        public static ExceptionInfo ToExceptionInfo(this Exception exception, Guid? key = null, string eventKey = null)
        {
            var result = InternalToExceptionInfo(exception);
            if (result != null)
            {
                var baseException = exception as BaseException;
                if (key == null)
                {
                    key = baseException?.Key ?? Guid.NewGuid();
                }

                var uniqueIdentifier = ContextHelper.ApiContext.UniqueIdentifier;
                result.ServerIdentifier = EnvironmentCore.MachineName;
                result.ServiceIdentifier = EnvironmentCore.ProductName;
                result.HttpMethod = uniqueIdentifier?.HttpMethod;
                result.Path = uniqueIdentifier?.Path;
                result.Key = key;
                result.EventKey = eventKey;
            }

            return result;
        }

        /// <summary>
        /// Internals to exception information.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        private static ExceptionInfo InternalToExceptionInfo(this Exception exception)
        {
            if (exception != null)
            {
                var baseException = exception as BaseException;

                var exceptionInfo = new ExceptionInfo
                {
                    ExceptionType = exception.GetType()?.GetFullName(),
                    Message = exception.Message,
                    Source = exception.Source,
                    TargetSite = exception.TargetSite.SafeToString(),
                    Code = baseException == null ? new ExceptionCode { Major = ExceptionCode.MajorCode.OperationFailure } : baseException.Code,
                    StackTrace = exception.StackTrace,
                    Data = baseException?.ReferenceData,
                    Scene = baseException?.Scene,
                    OperatorCredential = (baseException?.OperatorCredential) ?? (Framework.CurrentOperatorCredential),
                    Hint = baseException?.Hint
                };

                if (exception.InnerException != null)
                {
                    exceptionInfo.InnerException = InternalToExceptionInfo(exception.InnerException);
                }

                return exceptionInfo;
            }

            return null;
        }

        /// <summary>
        /// To the simple exception information.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="eventKey">The event key.</param>
        /// <returns>
        /// Beyova.Diagnostic.ExceptionInfo.
        /// </returns>
        public static ExceptionInfo ToSimpleExceptionInfo(this BaseException exception, string eventKey = null)
        {
            return exception == null ? null : new ExceptionInfo
            {
                ExceptionType = exception.GetType()?.GetFullName(),
                Message = exception.Hint != null ? exception.Hint.Message : exception.Message,
                Code = exception.Hint != null ? new ExceptionCode
                {
                    Major = exception.Code.Major,
                    Minor = exception.Hint.HintCode
                } : exception.Code,
                EventKey = eventKey,
                Data = (exception.Hint != null && exception.Hint.CauseObjects != null) ? JToken.FromObject(exception.Hint.CauseObjects) : exception.ReferenceData
            };
        }

        /// <summary>
        /// To the exception.
        /// </summary>
        /// <param name="exceptionInfo">The exception information.</param>
        /// <returns>System.Exception.</returns>
        public static Exception ToException(this ExceptionInfo exceptionInfo)
        {
            Exception result = null;

            if (exceptionInfo != null)
            {
                var innerException = exceptionInfo.InnerException;

                switch (exceptionInfo.ExceptionType)
                {
                    case "Beyova.Diagnostic.HttpOperationException":
                        return new HttpOperationException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);

                    case "Beyova.Diagnostic.SqlStoredProcedureException":
                        return new SqlStoredProcedureException(exceptionInfo.Message, exceptionInfo.Code);

                    default:
                        break;
                }

                switch (exceptionInfo.Code.Major)
                {
                    case ExceptionCode.MajorCode.CreditNotAfford:
                        result = new CreditNotAffordException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.DataConflict:
                        result = new DataConflictException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.NotImplemented:
                        result = new UnimplementedException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.NullOrInvalidValue:
                        result = new InvalidObjectException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.OperationFailure:
                        result = new OperationFailureException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.OperationForbidden:
                        result = new OperationForbiddenException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.ResourceNotFound:
                        result = new ResourceNotFoundException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.ServiceUnavailable:
                        result = new ServiceUnavailableException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.UnauthorizedOperation:
                        result = new UnauthorizedOperationException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.HttpBlockError:
                        result = new HttpOperationException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    case ExceptionCode.MajorCode.Unsupported:
                        result = new UnsupportedException(exceptionInfo.Key ?? Guid.NewGuid(), exceptionInfo.CreatedStamp ?? DateTime.UtcNow, exceptionInfo.Message, exceptionInfo.Scene, exceptionInfo.Code, ToException(innerException), exceptionInfo.OperatorCredential, exceptionInfo.Data, exceptionInfo.Hint);
                        break;

                    default:
                        result = new Exception(exceptionInfo.Message);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts to.
        /// </summary>
        /// <param name="sqlException">The SQL exception.</param>
        /// <returns>Beyova.Diagnostic.BaseException.</returns>
        public static BaseException ConvertTo(SqlStoredProcedureException sqlException)
        {
            BaseException result = null;

            if (sqlException != null)
            {
                switch (sqlException.Code.Major)
                {
                    case ExceptionCode.MajorCode.NullOrInvalidValue:
                        result = new InvalidObjectException(sqlException, reason: sqlException.Code.Minor);
                        break;

                    case ExceptionCode.MajorCode.UnauthorizedOperation:
                        result = new UnauthorizedOperationException(sqlException, minorCode: sqlException.Code.Minor);
                        break;

                    case ExceptionCode.MajorCode.OperationForbidden:
                        result = new OperationForbiddenException(sqlException.Code.Minor, sqlException);
                        break;

                    case ExceptionCode.MajorCode.DataConflict:
                        result = new DataConflictException(sqlException.Code.Minor, innerException: sqlException);
                        break;

                    default:
                        result = sqlException;
                        break;
                }
            }

            return result;
        }

        #region Http To Exception Scene

        /// <summary>
        /// To the exception scene.
        /// </summary>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <param name="controllerOrServiceName">Name of the controller or service.</param>
        /// <returns>Beyova.Diagnostic.ExceptionScene.</returns>
        public static ExceptionScene ToExceptionScene(this HttpRequestMessage httpRequest, string controllerOrServiceName = null)
        {
            return httpRequest == null ? null : new ExceptionScene
            {
                MethodName = string.Format("{0}: {1}", httpRequest.Method, httpRequest.RequestUri),
                FilePath = controllerOrServiceName
            };
        }

        #endregion Http To Exception Scene
    }
}