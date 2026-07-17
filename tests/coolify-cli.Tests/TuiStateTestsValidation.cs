#nullable enable
using System;
using System.Collections.Generic;
using CoolifyCli.Models;

namespace CoolifyCli.Tests;

/// <summary>
/// Provides validation helpers for <see cref="TuiState"/> instances.
/// </summary>
public static class TuiStateTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="TuiState"/> instance.
    /// </summary>
    /// <param name="value">The state to validate.</param>
    /// <returns>An immutable list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TuiState? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.ActiveView is not (TuiView.AppList or TuiView.AppDetail or TuiView.DbList or TuiView.LogStream or TuiView.Help))
        {
            return new[] { $"ActiveView must be a valid TuiView value, but was {value.ActiveView}." };
        }

        if (value.SelectedIndex < 0)
        {
            return new[] { $"SelectedIndex must be non-negative, but was {value.SelectedIndex}." };
        }

        if (value.ScrollOffset < 0)
        {
            return new[] { $"ScrollOffset must be non-negative, but was {value.ScrollOffset}." };
        }

        var problems = new List<string>();

        if (value.Applications is null)
        {
            problems.Add("Applications collection must not be null.");
        }

        if (value.Databases is null)
        {
            problems.Add("Databases collection must not be null.");
        }

        if (string.IsNullOrEmpty(value.StatusMessage))
        {
            problems.Add("StatusMessage must not be null or empty.");
        }

        if (value.LastRefreshedAt == default)
        {
            problems.Add("LastRefreshedAt must be set to a valid DateTime value.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="TuiState"/> instance is valid.
    /// </summary>
    /// <param name="value">The state to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this TuiState? value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="TuiState"/> instance is valid.
    /// </summary>
    /// <param name="value">The state to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the state is invalid, containing a list of problems.</exception>
    public static void EnsureValid(this TuiState? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"TuiState is invalid. Problems:\n{string.Join("\n", problems)}");
    }
}