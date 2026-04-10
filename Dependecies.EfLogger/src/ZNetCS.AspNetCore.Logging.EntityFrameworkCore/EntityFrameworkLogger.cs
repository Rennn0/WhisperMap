// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkLogger.cs" company="Marcin Smółka">
//   Copyright (c) Marcin Smółka. All rights reserved.
// </copyright>
// <summary>
//   Represents a new instance of an entity framework logger for the specified log.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

/// <summary>
///     Represents a new instance of an entity framework logger for the specified log.
/// </summary>
/// <typeparam name="TContext">
///     The type of the data context class used to access the store.
/// </typeparam>
public class EntityFrameworkLogger<TContext> : EntityFrameworkLogger<TContext, Log>
    where TContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityFrameworkLogger{TContext}" /> class.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The service provider to resolve dependency.
    /// </param>
    /// <param name="name">
    ///     The name of the logger.
    /// </param>
    /// <param name="filter">
    ///     The function used to filter events based on the log level.
    /// </param>
    /// <param name="creator">
    ///     The creator used to create new instance of log.
    /// </param>
    public EntityFrameworkLogger(
        IServiceProvider serviceProvider,
        string name,
        Func<string, LogLevel, bool> filter,
        Func<int, int, string, string, Log>? creator = null)
        : base(serviceProvider, name, filter, creator)
    {
    }
}

/// <summary>
///     Represents a new instance of an entity framework logger for the specified log.
/// </summary>
/// <typeparam name="TContext">
///     The type of the data context class used to access the store.
/// </typeparam>
/// <typeparam name="TLog">
///     The type representing a log.
/// </typeparam>
public class EntityFrameworkLogger<TContext, TLog> : EntityFrameworkLogger<TContext, TLog, int>
    where TContext : DbContext
    where TLog : Log<int>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityFrameworkLogger{TContext,TLog}" /> class.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The service provider to resolve dependency.
    /// </param>
    /// <param name="name">
    ///     The name of the logger.
    /// </param>
    /// <param name="filter">
    ///     The function used to filter events based on the log level.
    /// </param>
    /// <param name="creator">
    ///     The creator used to create new instance of log.
    /// </param>
    public EntityFrameworkLogger(
        IServiceProvider serviceProvider,
        string name,
        Func<string, LogLevel, bool> filter,
        Func<int, int, string, string, TLog>? creator = null)
        : base(serviceProvider, name, filter, creator)
    {
    }
}

/// <summary>
///     Represents a new instance of an entity framework logger for the specified log.
/// </summary>
/// <typeparam name="TContext">
///     The type of the data context class used to access the store.
/// </typeparam>
/// <typeparam name="TLog">
///     The type representing a log.
/// </typeparam>
/// <typeparam name="TKey">
///     The type of the primary key for a log.
/// </typeparam>
public class EntityFrameworkLogger<TContext, TLog, TKey> : ILogger
    where TContext : DbContext
    where TLog : Log<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     The function used to filter events based on the log level.
    /// </summary>
    private readonly Func<string, LogLevel, bool> filter;

    /// <summary>
    ///     The service provider to resolve dependency.
    /// </summary>
    private readonly IServiceProvider serviceProvider;


    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityFrameworkLogger{TContext,TLog,TKey}" /> class.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The service provider to resolve dependency.
    /// </param>
    /// <param name="name">
    ///     The name of the logger.
    /// </param>
    /// <param name="filter">
    ///     The function used to filter events based on the log level.
    /// </param>
    /// <param name="creator">
    ///     The creator used to create new instance of log.
    /// </param>
    public EntityFrameworkLogger(
        IServiceProvider? serviceProvider,
        string? name,
        Func<string, LogLevel, bool>? filter,
        Func<int, int, string, string, TLog>? creator = null)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.filter = filter ?? throw new ArgumentNullException(nameof(filter));

        Name = name ?? string.Empty;
        Creator = creator ?? DefaultCreator;
    }


    /// <summary>
    ///     Gets the function used to create new model instance for a log.
    /// </summary>
    protected virtual Func<int, int, string, string, TLog> Creator { get; }

    /// <summary>
    ///     Gets the name of the logger.
    /// </summary>
    protected virtual string Name { get; }


    /// <inheritdoc />
    public virtual bool IsEnabled(LogLevel logLevel)
    {
        // internal check to not log any Microsoft.EntityFrameworkCore. It won't work any way and cause StackOverflowException
        if (Name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase)) return false;

        return logLevel != LogLevel.None && filter(Name, logLevel);
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NoopDisposable.Instance;

    /// <inheritdoc />
    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        ArgumentNullException.ThrowIfNull(formatter);

        string message = formatter(state, exception);

        if (string.IsNullOrEmpty(message)) return;

        message = $"{message}";

        if (exception != null) message += $"{Environment.NewLine}{Environment.NewLine}{exception}";

        WriteMessage(message, logLevel, eventId.Id);
    }


    /// <summary>
    ///     Writes message to database.
    /// </summary>
    /// <param name="message">
    ///     The message to write.
    /// </param>
    /// <param name="logLevel">
    ///     The log level to write.
    /// </param>
    /// <param name="eventId">
    ///     The event id to write.
    /// </param>
    protected virtual void WriteMessage(string message, LogLevel logLevel, int eventId)
    {
        // create separate scope for DbContextOptions and DbContext
        using IServiceScope scope = serviceProvider.CreateScope();

        // create separate DbContext for adding log
        // normally we should rely on scope context, but in rare scenarios when DbContext is
        // registered as singleton, we should avoid this.
        using TContext context = ActivatorUtilities.CreateInstance<TContext>(scope.ServiceProvider);

        // create new log with resolving dependency injection
        TLog log = Creator((int)logLevel, eventId, Name, message);

        context.Set<TLog>().Add(log);

        try
        {
            context.SaveChanges();
        }
        catch
        {
            // if db cannot save error we should ignore it. To not cause additional connection errors.
        }
    }

    /// <summary>
    ///     The default log creator method.
    /// </summary>
    /// <param name="logLevel">
    ///     The log level.
    /// </param>
    /// <param name="eventId">
    ///     The event id.
    /// </param>
    /// <param name="logName">
    ///     The log name.
    /// </param>
    /// <param name="message">
    ///     The message.
    /// </param>
    private TLog DefaultCreator(int logLevel, int eventId, string logName, string message)
    {
        // create separate scope for Scope registered dependencies.
        using IServiceScope scope = serviceProvider.CreateScope();
        TLog log = ActivatorUtilities.CreateInstance<TLog>(scope.ServiceProvider);

        log.TimeStamp = DateTimeOffset.Now;
        log.Level = logLevel;
        log.EventId = eventId;
        log.Name = logName.Length > 255 ? logName[..255] : logName;
        log.Message = message;

        return log;
    }


    /// <summary>
    ///     The noop disposable.
    /// </summary>
    private class NoopDisposable : IDisposable
    {
        /// <summary>
        ///     The instance.
        /// </summary>
        public static readonly NoopDisposable Instance = new NoopDisposable();


        /// <summary>
        ///     The dispose.
        /// </summary>
        public void Dispose()
        {
        }
    }
}