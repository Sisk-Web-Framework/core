﻿// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Router__CoreInvoker.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Internal;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;

namespace Sisk.Core.Routing;

public partial class Router
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsMethodMatching(string ogRqMethod, RouteMethod method)
    {
        if (method == RouteMethod.Any) return true;
        if (Enum.TryParse(ogRqMethod, true, out RouteMethod ogRqParsed))
        {
            return method.HasFlag(ogRqParsed);
        }
        return false;
    }

    private Internal.HttpStringInternals.PathMatchResult TestRouteMatchUsingRegex(Route route, string requestPath)
    {
        if (route.routeRegex is null)
        {
            route.routeRegex = new Regex(route.Path, this.MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        var test = route.routeRegex.Match(requestPath);
        if (test.Success)
        {
            NameValueCollection query = new NameValueCollection();
            for (int i = 0; i < test.Groups.Count; i++)
            {
                Group group = test.Groups[i];
                if (group.Index.ToString() == group.Name) continue;
                query.Add(group.Name, group.Value);
            }
            return new HttpStringInternals.PathMatchResult(true, query);
        }
        else
        {
            return new HttpStringInternals.PathMatchResult(false, new NameValueCollection());
        }
    }

    internal bool InvokeRequestHandlerGroup(RequestHandlerExecutionMode mode, Span<IRequestHandler> baseLists, Span<IRequestHandler> bypassList, HttpRequest request, HttpContext context, out HttpResponse? result, out Exception? exception)
    {
        for (int i = 0; i < baseLists.Length; i++)
        {
            var rh = baseLists[i];
            if (rh.ExecutionMode == mode)
            {
                HttpResponse? response = this.InvokeHandler(rh, request, context, bypassList, out exception);
                if (response is not null)
                {
                    result = response;
                    return true;
                }
            }
        }
        result = null;
        exception = null;
        return false;
    }

    internal HttpResponse? InvokeHandler(IRequestHandler handler, HttpRequest request, HttpContext context, Span<IRequestHandler> bypass, out Exception? exception)
    {
        for (int i = 0; i < bypass.Length; i++)
        {
            if (ReferenceEquals(handler, bypass[i]))
            {
                exception = null;
                return null;
            }
        }

        HttpResponse? result = null;
        try
        {
            result = handler.Execute(request, context);
        }
        catch (Exception ex)
        {
            exception = ex;
            if (!this.parentServer!.ServerConfiguration.ThrowExceptions)
            {
                if (this.CallbackErrorHandler is not null)
                {
                    result = this.CallbackErrorHandler(ex, context);
                }
                else { /* do nothing */ };
            }
            else throw;
        }

        exception = null;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal RouterExecutionResult Execute(HttpContext context)
    {
        // the line below ensures that _routesList will not be modified in this method
        if (this.parentServer is null) throw new InvalidOperationException(SR.Router_NotBinded);

        context.Router = this;
        HttpRequest request = context.Request;
        HttpServerFlags flag = this.parentServer!.ServerConfiguration.Flags;

        //  HttpContext._shared = context;

        Route? matchedRoute = null;
        RouteMatchResult matchResult = RouteMatchResult.NotMatched;

        // IsReadOnly ensures that no route will be added or removed from the list during the
        // span iteration
        // 
        Span<Route> rspan = CollectionsMarshal.AsSpan(this._routesList);
        ref Route rPointer = ref MemoryMarshal.GetReference(rspan);
        for (int i = 0; i < rspan.Length; i++)
        {
            ref Route route = ref Unsafe.Add(ref rPointer, i);

            // test path
            HttpStringInternals.PathMatchResult pathTest;
            string reqUrlTest = request.Path;

            if (route.UseRegex)
            {
                pathTest = this.TestRouteMatchUsingRegex(route, reqUrlTest);
            }
            else
            {
                pathTest = HttpStringInternals.IsReqPathMatch(route.Path, reqUrlTest, this.MatchRoutesIgnoreCase);
            }

            if (!pathTest.IsMatched)
            {
                continue;
            }

            matchResult = RouteMatchResult.PathMatched;
            bool isMethodMatched = false;

            // test method
            if (flag.SendCorsHeaders && request.Method == HttpMethod.Options)
            {
                matchResult = RouteMatchResult.OptionsMatched;
                break;
            }
            else if (flag.TreatHeadAsGetMethod && request.Method == HttpMethod.Head && route.Method == RouteMethod.Get)
            {
                isMethodMatched = true;
            }
            else if (this.IsMethodMatching(request.Method.Method, route.Method))
            {
                isMethodMatched = true;
            }

            if (isMethodMatched)
            {
                if (pathTest.Query is not null)
                {
                    var keys = pathTest.Query.Keys;

                    for (int j = 0; j < keys.Count; j++)
                    {
                        string? queryItem = keys[j];
                        if (string.IsNullOrEmpty(queryItem)) continue;

                        string? value = pathTest.Query[queryItem];
                        if (string.IsNullOrEmpty(value)) continue;
                        string valueDecoded = HttpUtility.UrlDecode(pathTest.Query[queryItem]) ?? string.Empty;

                        request.Query.SetItemInternal(queryItem, valueDecoded);
                        request.RouteParameters.SetItemInternal(queryItem, valueDecoded);
                    }

                    request.RouteParameters.MakeReadOnly();
                }

                matchResult = RouteMatchResult.FullyMatched;
                matchedRoute = route;
                break;
            }
        }

        if (matchResult == RouteMatchResult.NotMatched && this.NotFoundErrorHandler is not null)
        {
            return new RouterExecutionResult(this.NotFoundErrorHandler(context), null, matchResult, null);
        }
        else if (matchResult == RouteMatchResult.OptionsMatched)
        {
            HttpResponse corsResponse = new HttpResponse();
            corsResponse.Status = HttpStatusCode.OK;

            return new RouterExecutionResult(corsResponse, null, matchResult, null);
        }
        else if (matchResult == RouteMatchResult.PathMatched && this.MethodNotAllowedErrorHandler is not null)
        {
            context.MatchedRoute = matchedRoute;
            return new RouterExecutionResult(this.MethodNotAllowedErrorHandler(context), matchedRoute, matchResult, null);
        }
        else if (matchResult == RouteMatchResult.FullyMatched && matchedRoute is not null)
        {
            context.MatchedRoute = matchedRoute;
            HttpResponse? result = null;

            if (flag.ForceTrailingSlash && !matchedRoute.UseRegex && !request.Path.EndsWith('/') && request.Method == HttpMethod.Get)
            {
                HttpResponse res = new HttpResponse();
                res.Status = HttpStatusCode.TemporaryRedirect;
                res.Headers.Add("Location", request.Path + "/" + (request.QueryString ?? string.Empty));
                return new RouterExecutionResult(res, matchedRoute, matchResult, null);
            }

            this.parentServer?.handler.ContextBagCreated(context.RequestBag);

            #region Before-response handlers
            HttpResponse? rhResponse;
            Exception? rhException;
            if (this.InvokeRequestHandlerGroup(RequestHandlerExecutionMode.BeforeResponse, CollectionsMarshal.AsSpan(this.GlobalRequestHandlers), matchedRoute.BypassGlobalRequestHandlers, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            if (this.InvokeRequestHandlerGroup(RequestHandlerExecutionMode.BeforeResponse, matchedRoute.RequestHandlers, null, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            #endregion

            #region Route action

            if (matchedRoute.Action is null)
            {
                throw new ArgumentNullException(string.Format(SR.Router_NoRouteActionDefined, matchedRoute));
            }

            try
            {
                context.MatchedRoute = matchedRoute;
                object? actionResult;

                if (matchedRoute._callback is RouteAction _routeAction)
                {
                    actionResult = _routeAction(request);
                }
                else if (matchedRoute._callback is ParameterlessRouteAction _paramlessRouteAction)
                {
                    actionResult = _paramlessRouteAction();
                }
                else
                {
                    // it will probably never get here since the route
                    // ensures that the callback is one of RouteAction or ParameterlessRouteAction
                    throw new ArgumentNullException(SR.Router_Handler_ActionNullValue);
                }

                if (matchedRoute._isAsyncAction)
                {
                    if (actionResult is null)
                    {
                        throw new ArgumentNullException(SR.Router_Handler_ActionNullValue);
                    }

                    ref Task<object> actionTask = ref Unsafe.As<object, Task<object>>(ref actionResult);
                    actionResult = actionTask.GetAwaiter().GetResult();
                }

                result = this.ResolveAction(actionResult);
            }
            catch (Exception ex)
            {
                if (this.parentServer!.ServerConfiguration.ThrowExceptions == false && (ex is not HttpListenerException))
                {
                    if (this.CallbackErrorHandler is not null)
                    {
                        result = this.CallbackErrorHandler(ex, context);
                    }
                    else
                    {
                        result = new HttpResponse(HttpResponse.HTTPRESPONSE_UNHANDLED_EXCEPTION);
                        return new RouterExecutionResult(result, matchedRoute, matchResult, ex);
                    }
                }
                else throw;
            }
            finally
            {
                context.RouterResponse = result;
            }
            #endregion

            #region After-response global handlers
            if (this.InvokeRequestHandlerGroup(RequestHandlerExecutionMode.AfterResponse, CollectionsMarshal.AsSpan(this.GlobalRequestHandlers), matchedRoute.BypassGlobalRequestHandlers, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            if (this.InvokeRequestHandlerGroup(RequestHandlerExecutionMode.AfterResponse, matchedRoute.RequestHandlers, null, request, context, out rhResponse, out rhException))
            {
                return new RouterExecutionResult(rhResponse, matchedRoute, matchResult, rhException);
            }
            #endregion     
        }

        return new RouterExecutionResult(context.RouterResponse, matchedRoute, matchResult, null);
    }
}
