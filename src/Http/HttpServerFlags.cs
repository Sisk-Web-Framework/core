﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerFlags.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;

namespace Sisk.Core.Http {
    /// <summary>
    /// Provides advanced fields for Sisk server behavior.
    /// </summary>
    public sealed class HttpServerFlags {
        /// <summary>
        /// Determines if the HTTP server should drop requests which has content body in GET, OPTIONS, HEAD and TRACE methods.
        ///     <para>
        ///         Default value: <see langword="true"></see>
        ///     </para>
        /// </summary>
        public bool ThrowContentOnNonSemanticMethods = true;

        /// <summary>
        /// Determines if the HTTP server should dispose all <see cref="IDisposable"/> values in the <see cref="HttpContext"/> bag
        /// when an HTTP session is closed.
        ///     <para>
        ///         Default value: <see langword="true"></see>
        ///     </para>
        /// </summary>
        public bool DisposeDisposableContextValues = true;

        /// <summary>
        /// Determines if the HTTP server should prevent sending body contents in responses when the HTTP request 
        /// method prohibits it.
        ///     <para>
        ///         Default value: <see langword="true"></see>
        ///     </para>
        /// </summary>
        public bool PreventResponseContentsInProhibitedMethods = true;

        /// <summary>
        /// Determines if the HTTP server should handle requests asynchronously or if
        /// it should limit the request processing to one request per time.
        ///     <para>
        ///         Default value: <see langword="true"></see>
        ///     </para>
        /// </summary>
        public bool AsyncRequestProcessing = true;

        /// <summary>
        /// Determines the HTTP header name of the request ID.
        ///     <para>
        ///         Default value: <c>"X-Request-Id"</c>
        ///     </para>
        /// </summary>
        public string HeaderNameRequestId = HttpKnownHeaderNames.XRequestID;

        /// <summary>
        /// Determines if the HTTP server automatically should send CORS headers if set.
        ///     <para>
        ///         Default value: <see langword="true"></see>
        ///     </para>
        /// </summary>
        public bool SendCorsHeaders = true;

        /// <summary>
        /// Determines if the HTTP server should automatically send HTTP headers of an pre-processed GET response
        /// if the request is using HEAD method.
        ///     <para>
        ///         Default value: <see langword="true"></see>
        ///     </para>
        /// </summary>

        public bool TreatHeadAsGetMethod = true;

        /// <summary>
        /// Determines if the HTTP server should write log to OPTIONS requests.
        ///     <para>
        ///         Default value: <see cref="LogOutput.Both"/>
        ///     </para>
        /// </summary>
        public LogOutput OptionsLogMode = LogOutput.Both;

        /// <summary>
        /// Determines if the HTTP server should send the X-Powered-By header in all responses.
        ///     <para>
        ///         Default value: <see langword="true"></see>
        ///     </para>
        /// </summary>
        public bool SendSiskHeader = true;

        /// <summary>
        /// Determines the WebSocket buffer initial and max length.
        ///     <para>
        ///         Default value: <c>1024</c>
        ///     </para>
        /// </summary>
        public int WebSocketBufferSize = 1024;

        /// <summary>
        /// Specifies the size, in bytes, of the copy buffer of both streams (inbound and outgoing)
        /// of the response stream.
        ///     <para>
        ///         Default value: <c>81920</c>
        ///     </para>
        /// </summary>
        public int RequestStreamCopyBufferSize = 81920;

        /// <summary>
        /// Determines if the HTTP server should convert request headers encoding to the content encoding.
        ///     <para>
        ///         Default value: <see langword="false"></see>
        ///     </para>
        /// </summary>
        public bool NormalizeHeadersEncodings = false;

        /// <summary>
        /// Determines if the HTTP server should automatically rewrite GET requests to end their path with /. This is 
        /// non-applyable to Regex routes.
        ///     <para>
        ///         Default value: <see langword="false"></see>
        ///     </para>
        /// </summary>
        public bool ForceTrailingSlash = false;

        /// <summary>
        /// Determines the maximum amount of time an connection can keep alive without sending or receiving any
        /// data.
        ///     <para>
        ///         Default value: 120 seconds
        ///     </para>
        /// </summary>
        public TimeSpan IdleConnectionTimeout = TimeSpan.FromSeconds ( 120 );

        /// <summary>
        /// Determines if the new span-based multipart form reader should be used.
        /// <para>
        ///     Default value: <see langword="true"></see>
        /// </para>
        /// </summary>
        public bool EnableNewMultipartFormReader = true;

        /// <summary>
        /// Determines if the HTTP server should convert <see cref="IAsyncEnumerable{T}"/> object responses into
        /// an blocking <see cref="IEnumerable{T}"/>.
        /// <para>
        ///     Default value: <see langword="true"></see>
        /// </para>
        /// </summary>
        public bool ConvertIAsyncEnumerableIntoEnumerable = true;

        /// <summary>
        /// Creates an new <see cref="HttpServerFlags"/> instance with default flags values.
        /// </summary>
        public HttpServerFlags () {
        }
    }
}
