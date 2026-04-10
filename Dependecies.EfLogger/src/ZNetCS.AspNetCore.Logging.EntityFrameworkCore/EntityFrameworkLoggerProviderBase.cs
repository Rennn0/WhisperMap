// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkLoggerProviderBase.cs" company="Marcin Smółka">
//   Copyright (c) Marcin Smółka. All rights reserved.
// </copyright>
// <summary>
//   The entity framework logger provider base.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;

namespace ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

/// <summary>
///     The entity framework logger provider base.
/// </summary>
public abstract class EntityFrameworkLoggerProviderBase : ILoggerProvider
{
    /// <summary>
    ///     The true filter.
    /// </summary>
    protected static readonly Func<string, LogLevel, bool> TrueFilter = (_, _) => true;


    /// <summary>
    ///     The disposed flag.
    /// </summary>
    private bool disposed;


    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    /// <inheritdoc />
    public abstract ILogger CreateLogger(string categoryName);


    /// <summary>
    ///     Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">
    ///     True if managed resources should be disposed; otherwise, false.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        disposed = true;
    }

    /// <summary>
    ///     Throws if this class has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (disposed) throw new ObjectDisposedException(GetType().Name);
    }
}