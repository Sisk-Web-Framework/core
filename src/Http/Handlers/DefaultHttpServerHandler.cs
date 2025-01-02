﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DefaultHttpServerHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;

namespace Sisk.Core.Http.Handlers;

internal class DefaultHttpServerHandler : HttpServerHandler {
    internal Action<Router>? _routerSetup;
    internal List<Action> _serverBootstrapingFunctions = new List<Action> ();

    protected override void OnServerStarting ( HttpServer server ) {
        base.OnServerStarting ( server );
        foreach (var func in this._serverBootstrapingFunctions) {
            func ();
        }
    }

    protected override void OnSetupRouter ( Router router ) {
        base.OnSetupRouter ( router );
        if (this._routerSetup != null)
            this._routerSetup ( router );
    }
}
