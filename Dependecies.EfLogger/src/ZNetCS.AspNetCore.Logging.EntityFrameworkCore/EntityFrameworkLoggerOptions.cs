// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFrameworkLoggerOptions.cs" company="Marcin Smółka">
//   Copyright (c) Marcin Smółka. All rights reserved.
// </copyright>
// <summary>
//   The entity framework logger options.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

/// <summary>
///     The entity framework logger options.
/// </summary>
public class EntityFrameworkLoggerOptions : EntityFrameworkLoggerOptions<Log>
{
}

/// <summary>
///     The entity framework logger options.
/// </summary>
/// <typeparam name="TLog">
///     The log model type.
/// </typeparam>
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "OK")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Public API")]
public class EntityFrameworkLoggerOptions<TLog>
{
    /// <summary>
    ///     Gets or sets the creator.
    /// </summary>
    public Func<int, int, string, string, TLog>? Creator { get; set; }
}