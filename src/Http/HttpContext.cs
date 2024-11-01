﻿// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpContext.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an context that is shared in a entire HTTP session.
    /// </summary>
    public sealed class HttpContext
    {
        [ThreadStatic]
        internal static HttpContext? _shared = null;

        /// <summary>
        /// Gets the current <see cref="HttpContext"/> of the current request on the server.
        /// </summary>
        public static HttpContext CurrentContext
        {
            get
            {
                return _shared ?? throw new InvalidOperationException(SR.HttpContext_SharedNotDefined);
            }
        }

        /// <summary>
        /// Gets or sets an <see cref="HttpHeaderCollection"/> indicating HTTP headers which
        /// will overwrite headers set by CORS, router response or request handlers.
        /// </summary>
        /// <remarks>
        /// This property replaces existing headers in the final response. Use <see cref="ExtraHeaders"/> to
        /// add headers without replacing existing ones.
        /// </remarks>
        public HttpHeaderCollection OverrideHeaders { get; set; } = new HttpHeaderCollection();

        /// <summary>
        /// Gets or sets the <see cref="HttpHeaderCollection"/> indicating HTTP headers which will
        /// be added (not overwritten) in the final response.
        /// </summary>
        public HttpHeaderCollection ExtraHeaders { get; set; } = new HttpHeaderCollection();

        /// <summary>
        /// Gets the <see cref="ListeningHost"/> instance of this HTTP context.
        /// </summary>
        public ListeningHost ListeningHost { get; private set; }

        /// <summary>
        /// Gets or sets a managed object that is accessed and modified by request handlers.
        /// </summary>
        public TypedValueDictionary RequestBag { get; set; } = new TypedValueDictionary();

        /// <summary>
        /// Gets the context <see cref="Http.HttpServer"/> instance.
        /// </summary>
        public HttpServer HttpServer { get; private set; }

        /// <summary>
        /// Gets the <see cref="Http.HttpResponse"/> for this context. This property acessible when a post-executing
        /// <see cref="IRequestHandler"/> was executed for this router context.
        /// </summary>
        public HttpResponse? RouterResponse { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Http.HttpRequest"/> which is contained in this HTTP context.
        /// </summary>
        public HttpRequest Request { get; private set; }

        /// <summary>
        /// Gets the matched <see cref="Routing.Route"/> for this context.
        /// </summary>
        public Route? MatchedRoute { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Routing.Router"/> where this context was
        /// created.
        /// </summary>
        public Router Router { get; internal set; }

        internal HttpContext(HttpServer httpServer, HttpRequest request, Route? matchedRoute, ListeningHost host)
        {
            this.Request = request;
            this.HttpServer = httpServer;
            this.MatchedRoute = matchedRoute;
            this.ListeningHost = host;
            this.Router = null!; // will be associated later in the router executor
        }
    }
}
