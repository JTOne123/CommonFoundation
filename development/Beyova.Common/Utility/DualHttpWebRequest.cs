﻿//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Net;
//using System.Text;
//using Beyova.Diagnostic;
//using Beyova.Http;

//namespace Beyova
//{
//    /// <summary>
//    /// Class DualHttpWebRequest. This class is used when you want to deliver same http request to two destination Uri.
//    /// </summary>
//    public class DualHttpWebRequest
//    {
//        #region Properties

//        /// <summary>
//        /// Gets or sets the primary URI.
//        /// </summary>
//        /// <value>The primary URI.</value>
//        public Uri PrimaryUri { get; protected set; }

//        /// <summary>
//        /// Gets or sets the secondary URI.
//        /// </summary>
//        /// <value>The secondary URI.</value>
//        public Uri SecondaryUri { get; protected set; }

//        /// <summary>
//        /// Gets or sets the method.
//        /// </summary>
//        /// <value>The method.</value>
//        public string Method { get; protected set; }

//        /// <summary>
//        /// Gets or sets the cookie collection.
//        /// </summary>
//        /// <value>The cookie collection.</value>
//        public CookieCollection CookieCollection { get; protected set; }

//        /// <summary>
//        /// Gets or sets the headers.
//        /// </summary>
//        /// <value>The headers.</value>
//        public NameValueCollection Headers { get; protected set; }

//        /// <summary>
//        /// Gets or sets the body.
//        /// </summary>
//        /// <value>The body.</value>
//        public byte[] Body { get; protected set; }

//        /// <summary>
//        /// Gets or sets the type of the content.
//        /// </summary>
//        /// <value>The type of the content.</value>
//        public string ContentType { get; set; }

//        #endregion Properties

//        #region Constructor

//        /// <summary>
//        /// Initializes a new instance of the <see cref="DualHttpWebRequest"/> class.
//        /// </summary>
//        /// <param name="primaryUri">The primary URI.</param>
//        /// <param name="secondaryUri">The secondary URI.</param>
//        /// <param name="method">The method.</param>
//        /// <param name="cookieCollection">The cookie collection.</param>
//        /// <param name="headers">The headers.</param>
//        public DualHttpWebRequest(Uri primaryUri, Uri secondaryUri, string method, CookieCollection cookieCollection = null, NameValueCollection headers = null)
//        {
//            this.PrimaryUri = primaryUri;
//            this.SecondaryUri = secondaryUri;
//            this.Method = method;
//            this.CookieCollection = cookieCollection;
//            this.Headers = headers;
//        }

//        #endregion Constructor

//        #region Fill Body

//        /// <summary>
//        /// Fills the body.
//        /// </summary>
//        /// <param name="bytes">The bytes.</param>
//        /// <param name="contentType">Type of the content.</param>
//        protected void FillBody(byte[] bytes, string contentType)
//        {
//            this.Body = bytes;
//            this.ContentType = contentType;
//        }

//        /// <summary>
//        /// Fills the bytes as body.
//        /// </summary>
//        /// <param name="bytes">The bytes.</param>
//        /// <param name="contentType">Type of the content.</param>
//        public void FillBytesAsBody(byte[] bytes, string contentType = null)
//        {
//            FillBody(bytes, contentType);
//        }

//        /// <summary>
//        /// Fills the form as body.
//        /// </summary>
//        /// <param name="form">The form.</param>
//        /// <param name="contentType">Type of the content.</param>
//        public void FillFormAsBody(Dictionary<string, string> form, string contentType = HttpConstants.ContentType.FormSubmit)
//        {
//            FillBody(HttpExtension.FormDataToBytes(form), contentType);
//        }

//        /// <summary>
//        /// Fills the json as body.
//        /// </summary>
//        /// <param name="jsonString">The json string.</param>
//        /// <param name="contentType">Type of the content.</param>
//        public void FillJsonAsBody(string jsonString, string contentType = HttpConstants.ContentType.Json)
//        {
//            FillBody(Encoding.UTF8.GetBytes(jsonString), contentType);
//        }

//        #endregion Fill Body

//        /// <summary>
//        /// Internals the read as text.
//        /// </summary>
//        /// <param name="destination">The destination.</param>
//        /// <param name="method">The method.</param>
//        /// <param name="body">The body.</param>
//        /// <param name="contentType">Type of the content.</param>
//        /// <param name="encoding">The encoding.</param>
//        /// <returns>
//        /// System.String.
//        /// </returns>
//        protected HttpActionResult<string> InternalReadAsText(Uri destination, string method, byte[] body, string contentType, Encoding encoding)
//        {
//            destination.CheckNullObject(nameof(destination));

//            var httpRequest = destination.CreateHttpWebRequest(method);
//            if (body != null)
//            {
//                httpRequest.FillData(body, contentType);
//            }

//            return httpRequest.ReadResponseAsText(encoding);
//        }

//        /// <summary>
//        /// Reads as text.
//        /// </summary>
//        /// <param name="encoding">The encoding.</param>
//        /// <returns>
//        /// System.String.
//        /// </returns>
//        public HttpActionResult<string> ReadAsText(Encoding encoding = null)
//        {
//            try
//            {
//                return InternalReadAsText(this.PrimaryUri, this.Method, this.Body, this.ContentType, encoding);
//            }
//            catch (HttpOperationException httpEx)
//            {
//                if (this.SecondaryUri != null)
//                {
//                    return InternalReadAsText(this.SecondaryUri, this.Method, this.Body, this.ContentType, encoding);
//                }
//                else
//                {
//                    throw httpEx.Handle(new { PrimaryUri = this.PrimaryUri?.ToString(), SecondaryUri = this.SecondaryUri?.ToString() });
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex.Handle(new { PrimaryUri = this.PrimaryUri?.ToString(), SecondaryUri = this.SecondaryUri?.ToString() });
//            }
//        }
//    }
//}