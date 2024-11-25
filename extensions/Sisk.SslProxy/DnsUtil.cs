﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DnsUtil.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Net;
using System.Net.Sockets;

namespace Sisk.Ssl;

static class DnsUtil
{
    public static IPEndPoint ResolveEndpoint(ListeningPort port, bool onlyUseIPv4 = false)
    {
        var hostEntry = Dns.GetHostEntry(port.Hostname);

        if (hostEntry.AddressList.Length == 0)
            throw new InvalidOperationException($"Couldn't resolve any IP addresses for {port}.");

        IPAddress? resolvedAddress;

        if (onlyUseIPv4)
        {
            resolvedAddress =
                // only resolves IPv4
                hostEntry.AddressList.LastOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }
        else
        {
            resolvedAddress =
                // try to return the last IPv6, or the last IPv4 if no IPv6 was found (#16)
                hostEntry.AddressList.LastOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6)
                ?? hostEntry.AddressList.LastOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        }

        if (resolvedAddress is null)
            throw new InvalidOperationException($"Couldn't resolve any IP addresses for {port}.");

        return new IPEndPoint(resolvedAddress, port.Port);
    }
}
