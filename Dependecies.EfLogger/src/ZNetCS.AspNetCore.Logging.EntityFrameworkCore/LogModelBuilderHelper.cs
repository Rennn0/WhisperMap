// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogModelBuilderHelper.cs" company="Marcin Smółka">
//   Copyright (c) Marcin Smółka. All rights reserved.
// </copyright>
// <summary>
//   The model builder helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

/// <summary>
///     The model builder helper.
/// </summary>
public static class LogModelBuilderHelper
{
    /// <summary>
    ///     The build helper method.
    /// </summary>
    /// <param name="builder">
    ///     The builder.
    /// </param>
    public static void Build(EntityTypeBuilder<Log> builder)
    {
        Build<Log>(builder);
    }

    /// <summary>
    ///     The build helper method.
    /// </summary>
    /// <param name="builder">
    ///     The builder.
    /// </param>
    /// <typeparam name="TLog">
    ///     The type representing a log.
    /// </typeparam>
    public static void Build<TLog>(EntityTypeBuilder<TLog> builder)
        where TLog : Log<int>
    {
        Build<TLog, int>(builder);
    }

    /// <summary>
    ///     The build helper method.
    /// </summary>
    /// <param name="builder">
    ///     The builder.
    /// </param>
    /// <typeparam name="TLog">
    ///     The type representing a log.
    /// </typeparam>
    /// <typeparam name="TKey">
    ///     The type of the primary key for a log.
    /// </typeparam>
    public static void Build<TLog, TKey>(EntityTypeBuilder<TLog> builder)
        where TLog : Log<TKey>
        where TKey : IEquatable<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(255);
    }
}