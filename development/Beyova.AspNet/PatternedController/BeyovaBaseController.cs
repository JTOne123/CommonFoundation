﻿using System;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Beyova.Api.RestApi;
using Beyova.ExceptionSystem;

namespace Beyova.Web
{
    /// <summary>
    /// Class BeyovaBaseController.
    /// </summary>
    [RestApiContextConsistence]
    public abstract class BeyovaBaseController : Controller
    {
        #region ThreadStatic fields

        /// <summary>
        /// The current controller
        /// </summary>
        [ThreadStatic]
        public static string CurrentController;

        /// <summary>
        /// The current action
        /// </summary>
        [ThreadStatic]
        public static string CurrentAction;

        /// <summary>
        /// Gets or sets the current environment.
        /// </summary>
        /// <value>The current environment.</value>
        [ThreadStatic]
        public static string CurrentEnvironment;

        #endregion

        /// <summary>
        /// The setting
        /// </summary>
        protected static RestApiSettings setting = new RestApiSettings()
        {
            OmitExceptionDetail = true,
            EnableContentCompression = true
        };

        /// <summary>
        /// Gets the error partial view.
        /// </summary>
        /// <value>The error partial view.</value>
        protected abstract string ErrorPartialView { get; }

        /// <summary>
        /// Gets the error view.
        /// </summary>
        /// <value>The error view.</value>
        protected abstract string ErrorView { get; }

        /// <summary>
        /// The API tracking
        /// </summary>
        protected IApiTracking apiTracking = null;

        /// <summary>
        /// The return exception as friendly
        /// </summary>
        protected bool returnExceptionAsFriendly;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeyovaBaseController" /> class.
        /// </summary>
        /// <param name="apiTracking">The API tracking.</param>
        /// <param name="returnExceptionAsFriendly">if set to <c>true</c> [return exception as friendly].</param>
        protected BeyovaBaseController(IApiTracking apiTracking = null, bool returnExceptionAsFriendly = false)
        {
            this.apiTracking = apiTracking ?? Framework.ApiTracking;
            this.returnExceptionAsFriendly = returnExceptionAsFriendly;
        }

        /// <summary>
        /// Returns as json.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="exceptionObject">The exception object.</param>
        /// <param name="returnObject">The return object.</param>
        /// <param name="hint">The hint.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        /// <returns>JsonResult.</returns>
        public virtual JsonResult ReturnAsJson(Exception ex, object exceptionObject, object returnObject, FriendlyHint hint = null,
            [CallerMemberName] string operationName = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var baseExceptionInfo = ex == null ? null : GetException(ex, exceptionObject, hint, operationName, sourceFilePath, sourceLineNumber);
            return Json(baseExceptionInfo == null ? returnObject ?? string.Empty : baseExceptionInfo);
        }

        /// <summary>
        /// Handles the exception to partial view.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="exceptionObject">The exception object.</param>
        /// <param name="hint">The hint.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        /// <returns>PartialViewResult.</returns>
        public virtual PartialViewResult HandleExceptionToPartialView(Exception ex, object exceptionObject, FriendlyHint hint = null,
            [CallerMemberName] string operationName = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var baseExceptionInfo = GetException(ex, exceptionObject, hint, operationName, sourceFilePath, sourceLineNumber);
            return PartialView(ErrorPartialView, baseExceptionInfo);
        }

        /// <summary>
        /// Handles the exception to redirection.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="exceptionObject">The exception object.</param>
        /// <param name="hint">The hint.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        /// <returns>RedirectToRouteResult.</returns>
        public virtual RedirectToRouteResult HandleExceptionToRedirection(Exception ex, object exceptionObject = null, FriendlyHint hint = null,
            [CallerMemberName] string operationName = null,
            [CallerFilePath] string sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var baseExceptionInfo = GetException(ex, exceptionObject, hint, operationName, sourceFilePath, sourceLineNumber);
            return RedirectToAction("Index", "Error", new { code = (int)(baseExceptionInfo.Code.Major) });
        }

        /// <summary>
        /// Renders as not found page.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ActionResult.</returns>
        public ActionResult RenderAsNotFoundPage(string message = null)
        {
            return View(ErrorView, new ExceptionInfo
            {
                Code = new ExceptionCode
                {
                    Major = ExceptionCode.MajorCode.ResourceNotFound
                },
                Message = message.SafeToString("Resource is not found")
            });
        }

        /// <summary>
        /// Redirects to not found page.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>RedirectResult.</returns>
        public RedirectResult RedirectToNotFoundPage(string message = null)
        {
            return Redirect("~/Error/?code=404&message=" + message.SafeToString("Resource is not found").ToUrlPathEncodedText());
        }

        /// <summary>
        /// Renders as action forbidden page.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>ActionResult.</returns>
        public ActionResult RenderAsActionForbiddenPage(string message = null)
        {
            return View(ErrorView, new ExceptionInfo
            {
                Code = new ExceptionCode
                {
                    Major = ExceptionCode.MajorCode.OperationForbidden
                },
                Message = message.SafeToString("Action is forbidden.")
            });
        }

        /// <summary>
        /// APIs the proxy.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cookieTokenKey">The cookie token key.</param>
        /// <returns>System.Object.</returns>
        public object ApiProxy(string method, string url, string cookieTokenKey = null)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var headers = this.Request.Headers;

                if (!string.IsNullOrWhiteSpace(cookieTokenKey))
                {
                    headers.Add(cookieTokenKey, this.Request.Cookies.Get(cookieTokenKey)?.Value);
                }

                var context = (RestApiRouter.FirstInstance as RestApiRouter<HttpRequestBase, HttpResponseBase>)?.ProcessRequestToRuntimeContext(method.SafeToString(HttpConstants.HttpMethod.Get), new Uri(this.Request.Url.GetPrimaryUri() + url.TrimStart(new char[] { '/', ' ' })), headers, true);

                if (context != null)
                {
                    string jsonBody;
                    return ApiHandlerBase.InternalInvoke(context.ApiInstance, context.ApiMethod, Request.InputStream.ReadStreamToBytes(), this.Request.Url, context.EntityKey, out jsonBody);
                }
            }

            return null;
        }

        /// <summary>
        /// Setups the route using URL: {controller}/{action}/{environment}/{key}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="route">The route.</param>
        /// <param name="defaultAction">The default action.</param>
        public static void SetupRoute<T>(RouteCollection route, string defaultAction = "Index")
            where T : BeyovaBaseController
        {
            if (route != null)
            {
                var name = GetRouteName<T>();

                route.MapRoute(
                    name: name,
                    url: (name + "/{action}/{key}"),
                    defaults: new { controller = name, action = defaultAction, key = UrlParameter.Optional }
                );
            }
        }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="exceptionObject">The exception object.</param>
        /// <param name="hint">The hint.</param>
        /// <param name="operationName">Name of the operation.</param>
        /// <param name="sourceFilePath">The source file path.</param>
        /// <param name="sourceLineNumber">The source line number.</param>
        /// <returns>Beyova.ExceptionSystem.BaseException.</returns>
        protected ExceptionInfo GetException(Exception ex, object exceptionObject = null, FriendlyHint hint = null,
                        string operationName = null,
                        string sourceFilePath = null,
                        int sourceLineNumber = 0)
        {
            var baseExceptionInfo = ex.Handle(new ExceptionScene
            {
                MethodName = operationName,
                FilePath = sourceFilePath,
                LineNumber = sourceLineNumber
            }, new
            {
                exceptionObject,
                HttpMethod = Request?.HttpMethod,
                RawUrl = Request?.RawUrl
            }, hint).ToExceptionInfo();

            if (baseExceptionInfo != null && this.apiTracking != null)
            {
                if (RestApiContextConsistenceAttribute.ApiEvent != null && baseExceptionInfo != null)
                {
                    RestApiContextConsistenceAttribute.ApiEvent.ExceptionKey = baseExceptionInfo.Key;
                }

                this.apiTracking.LogException(baseExceptionInfo);
            }

            if (this.returnExceptionAsFriendly)
            {
                hint = hint ?? ((ex as BaseException)?.Hint);

                baseExceptionInfo = new ExceptionInfo
                {
                    Message = string.Format("Error occurred when {0}: {1}", Request.HttpMethod, Request.RawUrl),
                    Code = hint == null ? baseExceptionInfo.Code : new ExceptionCode
                    {
                        Minor = hint.HintCode,
                        Major = baseExceptionInfo.Code.Major
                    },
                    Data = baseExceptionInfo.Data
                };
            }

            return baseExceptionInfo;
        }

        /// <summary>
        /// Gets the name of the route.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.String.</returns>
        private static string GetRouteName<T>()
        {
            var type = typeof(T);
            return type.Name.SubStringBeforeLastMatch("Controller");
        }

        /// <summary>
        /// Packages the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="data">The data.</param>
        /// <param name="ex">The ex.</param>
        protected void PackageResponse(HttpResponseBase response, object data, BaseException ex = null)
        {
            ApiHandlerBase<HttpRequestBase, HttpResponseBase>.PackageResponse(new HttpBaseApiContextContainer(null, response), data, null, ex, settings: setting);
        }
    }
}