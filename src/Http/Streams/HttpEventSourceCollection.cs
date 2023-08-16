﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpEventSourceCollection.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// Provides a managed object to manage <see cref="HttpRequestEventSource"/> connections.
    /// </summary>
    /// <definition>
    /// public sealed class HttpEventSourceCollection
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class HttpEventSourceCollection
    {
        internal List<HttpRequestEventSource> _eventSources = new List<HttpRequestEventSource>();

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpRequestEventSource"/> is registered in this collection.
        /// </summary>
        /// <definition>
        /// public event EventSourceRegistrationHandler? OnEventSourceRegistered;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event EventSourceRegistrationHandler? OnEventSourceRegistered;

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpRequestEventSource"/> is closed and removed from this collection.
        /// </summary>
        /// <definition>
        /// public event EventSourceUnregistrationHandler? OnEventSourceUnregistration;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event EventSourceUnregistrationHandler? OnEventSourceUnregistration;

        internal HttpEventSourceCollection()
        {
        }

        internal void UnregisterEventSource(HttpRequestEventSource eventSource)
        {
            lock (_eventSources)
            {
                if (_eventSources.Remove(eventSource) && OnEventSourceUnregistration != null)
                {
                    OnEventSourceUnregistration(this, eventSource);
                }
            }
        }

        internal void RegisterEventSource(HttpRequestEventSource src)
        {
            if (src.Identifier != null)
            {
                lock (_eventSources)
                {
                    HttpRequestEventSource[] toClose = Find(p => p == src.Identifier);
                    foreach (HttpRequestEventSource ev in toClose)
                    {
                        ev.Close();
                    }
                    _eventSources.Add(src);
                }
                if (OnEventSourceRegistered != null)
                    OnEventSourceRegistered(this, src);
            }
        }

        /// <summary>
        /// Gets an number indicating the amount of active event source connections.
        /// </summary>
        /// <definition>
        /// public int ActiveConnections { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int ActiveConnections { get => _eventSources.Count(ev => ev.IsActive); }

        /// <summary>
        /// Gets the event source connection for the specified identifier.
        /// </summary>
        /// <param name="identifier">The event source identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpRequestEventSource? GetByIdentifier(string identifier)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpRequestEventSource? GetByIdentifier(string identifier)
        {
            lock (_eventSources)
            {
                HttpRequestEventSource? src = _eventSources.Where(es => es.Identifier == identifier).FirstOrDefault();
                return src;
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpRequestEventSource"/> instances that matches their identifier predicate.
        /// </summary>
        /// <param name="predicate">The expression on the an non-empty event source identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpRequestEventSource[] Find(Func&lt;string, bool&gt; predicate)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpRequestEventSource[] Find(Func<string, bool> predicate)
        {
            lock (_eventSources)
            {
                return _eventSources.Where(e =>
                {
                    if (!e.IsActive || e.Identifier == null) return false;
                    return predicate(e.Identifier);
                }).ToArray();
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpRequestEventSource"/> instances.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public HttpRequestEventSource[] All()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpRequestEventSource[] All()
        {
            lock (_eventSources)
            {
                return _eventSources.Where(e => e.IsActive).ToArray();
            }
        }

        /// <summary>
        /// Closes and disposes all registered and active <see cref="HttpRequestEventSource"/> in this collections.
        /// </summary>
        /// <definition>
        /// public void DropAll()
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public void DropAll()
        {
            lock (_eventSources)
            {
                foreach (HttpRequestEventSource es in _eventSources) es.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpEventSourceCollection"/> registers an new event source connection.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpEventSourceCollection"/> object.</param>
    /// <param name="eventSource">Represents the registered <see cref="HttpRequestEventSource"/> event source connection.</param>
    /// <definition>
    /// public delegate void EventSourceRegistrationHandler(object sender, HttpRequestEventSource eventSource);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void EventSourceRegistrationHandler(object sender, HttpRequestEventSource eventSource);

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpEventSourceCollection"/> is removed and had their connection closed.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpEventSourceCollection"/> object.</param>
    /// <param name="eventSource">Represents the closed <see cref="HttpRequestEventSource"/> event source connection.</param>
    /// <definition>
    /// public delegate void EventSourceUnregistrationHandler(object sender, HttpRequestEventSource eventSource);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void EventSourceUnregistrationHandler(object sender, HttpRequestEventSource eventSource);
}
