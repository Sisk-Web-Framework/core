﻿// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Route.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using System.Text.RegularExpressions;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an HTTP route to be matched by an <see cref="Router"/>.
    /// </summary>
    public class Route : IEquatable<Route>
    {
        internal Delegate? _callback;
        internal bool _isAsyncAction;
        internal Regex? routeRegex;
        private string path;

        /// <summary>
        /// Represents an route path which captures any URL path.
        /// </summary>
        public const string AnyPath = "/<<ANY>>";

        /// <summary>
        /// Gets or sets an <see cref="TypedValueDictionary"/> for this route, which can hold contextual variables
        /// for this <see cref="Route"/> object.
        /// </summary>
        public TypedValueDictionary Bag { get; set; } = new TypedValueDictionary(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets an boolean indicating if this <see cref="Route"/> action return is an asynchronous <see cref="Task"/>.
        /// </summary>
        public bool IsAsync { get => this._isAsyncAction; }

        /// <summary>
        /// Gets or sets how this route can write messages to log files on the server.
        /// </summary>
        public LogOutput LogMode { get; set; } = LogOutput.Both;

        /// <summary>
        /// Get or sets if this route should use regex to be interpreted instead of predefined templates.
        /// </summary>
        public bool UseRegex { get; set; }

        /// <summary>
        /// Gets or sets whether this route should send Cross-Origin Resource Sharing headers in the response.
        /// </summary>
        public bool UseCors { get; set; } = true;

        /// <summary>
        /// Gets or sets the matching HTTP method.
        /// </summary>
        public RouteMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }
            set
            {
                if (this.UseRegex && this.routeRegex != null)
                {
                    // routeRegex is created in the router invocation
                    this.routeRegex = null;
                }
                this.path = value;
            }
        }

        /// <summary>
        /// Gets or sets the route name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the function that is called after the route is matched with the request.
        /// </summary>
        public Delegate? Action
        {
            get => this._callback;
            set
            {
                if (value is null)
                {
                    this._callback = null;
                    this._isAsyncAction = false;
                    return;
                }
                else if (value is not ParameterlessRouteAction && value is not RouteAction)
                {
                    throw new ArgumentException(SR.Route_Action_InvalidDelegateType);
                }

                var isAsync = false;
                var retType = value.Method.ReturnType;

                if (retType.IsValueType)
                {
                    throw new NotSupportedException(SR.Route_Action_ValueTypeSet);
                }
                else if (retType.IsAssignableTo(typeof(Task))) // is an async return
                {
                    isAsync = true;

                    if (retType.GenericTypeArguments.Length == 0)
                    {
                        throw new InvalidOperationException(string.Format(SR.Route_Action_AsyncMissingGenericType, this));
                    }
                    else
                    {
                        Type genericAssignType = retType.GenericTypeArguments[0];
                        if (genericAssignType.IsValueType)
                        {
                            throw new NotSupportedException(SR.Route_Action_ValueTypeSet);
                        }
                    }
                }

                this._isAsyncAction = isAsync;
                this._callback = value;
            }
        }

        /// <summary>
        /// Gets or sets the request handlers instances to run before the route's Action.
        /// </summary>
        public IRequestHandler[] RequestHandlers { get; set; } = Array.Empty<IRequestHandler>();

        /// <summary>
        /// Gets or sets the global request handlers instances that will not run on this route.
        /// </summary>
        public IRequestHandler[] BypassGlobalRequestHandlers { get; set; } = Array.Empty<IRequestHandler>();

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="action">The function that is called after the route is matched with the request.</param>
        public Route(RouteMethod method, string path, RouteAction action)
        {
            this.Method = method;
            this.path = path;
            this.Action = action;
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="name">The route name. It allows it to be found by other routes and makes it easier to create links.</param>
        /// <param name="action">The function that is called after the route is matched with the request.</param>
        /// <param name="beforeCallback">The RequestHandlers to run before the route's Action.</param>
        public Route(RouteMethod method, string path, string? name, RouteAction action, IRequestHandler[]? beforeCallback)
        {
            this.Method = method;
            this.path = path;
            this.Name = name;
            this.Action = action;
            this.RequestHandlers = beforeCallback ?? Array.Empty<IRequestHandler>();
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with no parameters.
        /// </summary>
        public Route()
        {
            this.path = "/";
        }

        /// <summary>
        /// Gets an string notation for this <see cref="Route"/> object.
        /// </summary>
        public override string ToString()
        {
            return $"[{this.Method.ToString().ToUpper()} {this.path}] {this.Name ?? this.Action?.Method.Name}";
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(this.path, this.Method, this.UseRegex);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is Route r)
            {
                return this.Equals(r);
            }
            return false;
        }

        /// <inheritdoc/>
        public bool Equals(Route? other)
        {
            return this.GetHashCode() == other?.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(Route? left, Route? right)
        {
            if (left is null && right is null) return true;
            return left?.Equals(right) == true;
        }

        /// <inheritdoc/>
        public static bool operator !=(Route? left, Route? right)
        {
            if (left is null && right is null) return false;
            return !left?.Equals(right) == true;
        }
    }

    /// <summary>
    /// Determines the way the server can write log messages. This enumerator is for giving permissions for certain contexts
    /// to be able or not to write to the server logs, such as <see cref="Http.HttpServerConfiguration.AccessLogsStream"/> and <see cref="Http.HttpServerConfiguration.ErrorsLogsStream"/>.
    /// </summary>
    [Flags]
    public enum LogOutput
    {
        /// <summary>
        /// Determines that the context or the route can write log messages only to the access logs through <see cref="Http.HttpServerConfiguration.AccessLogsStream"/>.
        /// </summary>
        AccessLog = 1,

        /// <summary>
        /// Determines that the context or the route can write error messages only to the error logs through <see cref="Http.HttpServerConfiguration.ErrorsLogsStream"/>.
        /// </summary>
        ErrorLog = 2,

        /// <summary>
        /// Determines that the context or the route can write log messages to both error and access logs.
        /// </summary>
        Both = AccessLog | ErrorLog,

        /// <summary>
        /// Determines that the context or the route cannot write any log messages.
        /// </summary>
        None = 0
    }
}