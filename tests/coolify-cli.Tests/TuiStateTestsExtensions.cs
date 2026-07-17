#nullable enable

using CoolifyCli.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CoolifyCli.Tests;

/// <summary>
/// Provides extension methods for <see cref="TuiStateTests"/> to facilitate testing of TUI state behavior.
/// </summary>
public static class TuiStateTestsExtensions
{
    /// <summary>
    /// Creates a new <see cref="TuiState"/> with the specified applications.
    /// </summary>
    /// <param name="applications">The list of applications to initialize the state with.</param>
    /// <returns>A new <see cref="TuiState"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="applications"/> is null.</exception>
    public static TuiState WithApplications(this TuiStateTests _, List<ApplicationDeployment> applications)
    {
        ArgumentNullException.ThrowIfNull(applications);
        return new TuiState { Applications = applications };
    }

    /// <summary>
    /// Creates a new <see cref="TuiState"/> with the specified selected index.
    /// </summary>
    /// <param name="selectedIndex">The index to select.</param>
    /// <returns>A new <see cref="TuiState"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="selectedIndex"/> is negative.</exception>
    public static TuiState WithSelectedIndex(this TuiStateTests _, int selectedIndex)
    {
        if (selectedIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(selectedIndex), "Selected index cannot be negative.");
        }

        return new TuiState { SelectedIndex = selectedIndex };
    }

    /// <summary>
    /// Creates a new <see cref="TuiState"/> with the specified scroll offset.
    /// </summary>
    /// <param name="scrollOffset">The scroll offset to set.</param>
    /// <returns>A new <see cref="TuiState"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="scrollOffset"/> is negative.</exception>
    public static TuiState WithScrollOffset(this TuiStateTests _, int scrollOffset)
    {
        if (scrollOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(scrollOffset), "Scroll offset cannot be negative.");
        }

        return new TuiState { ScrollOffset = scrollOffset };
    }

    /// <summary>
    /// Asserts that the selected index is at the expected position.
    /// </summary>
    /// <param name="state">The TUI state to check.</param>
    /// <param name="expectedIndex">The expected selected index.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expectedIndex"/> is negative.</exception>
    public static void SelectedIndexShouldBe(this TuiState state, int expectedIndex)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (expectedIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedIndex), "Expected index cannot be negative.");
        }

        state.SelectedIndex.Should().Be(expectedIndex, "because the selected index should match the expected value");
    }

    /// <summary>
    /// Asserts that the scroll offset is at the expected position.
    /// </summary>
    /// <param name="state">The TUI state to check.</param>
    /// <param name="expectedOffset">The expected scroll offset.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expectedOffset"/> is negative.</exception>
    public static void ScrollOffsetShouldBe(this TuiState state, int expectedOffset)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (expectedOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedOffset), "Expected offset cannot be negative.");
        }

        state.ScrollOffset.Should().Be(expectedOffset, "because the scroll offset should match the expected value");
    }

    /// <summary>
    /// Asserts that the selected application matches the expected application.
    /// </summary>
    /// <param name="state">The TUI state to check.</param>
    /// <param name="expectedAppId">The expected application ID.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="expectedAppId"/> is not positive.</exception>
    public static void SelectedAppShouldBe(this TuiState state, int expectedAppId)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (expectedAppId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedAppId), "Expected app ID must be positive.");
        }

        var selected = state.GetSelectedApp();
        selected.Should().NotBeNull("because there should be a selected application");
        selected!.Id.Should().Be(expectedAppId, "because the selected application ID should match the expected value");
    }

    /// <summary>
    /// Moves down by the specified number of positions and asserts the new index.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="moveCount">Number of positions to move down.</param>
    /// <param name="expectedIndex">Expected index after moving.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="moveCount"/> is negative.</exception>
    public static void MoveDownAndAssert(this TuiState state, int moveCount, int expectedIndex)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (moveCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(moveCount), "Move count cannot be negative.");
        }

        state.MoveDown(moveCount);
        state.SelectedIndexShouldBe(expectedIndex);
    }

    /// <summary>
    /// Moves up by the specified number of positions and asserts the new index.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="moveCount">Number of positions to move up.</param>
    /// <param name="expectedIndex">Expected index after moving.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="state"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="moveCount"/> is negative.</exception>
    public static void MoveUpAndAssert(this TuiState state, int moveCount, int expectedIndex)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (moveCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(moveCount), "Move count cannot be negative.");
        }

        for (var i = 0; i < moveCount; i++)
        {
            state.MoveUp();
        }
        state.SelectedIndexShouldBe(expectedIndex);
    }
}