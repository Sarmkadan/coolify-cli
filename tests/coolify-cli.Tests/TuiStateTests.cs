#nullable enable
using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Tests for the TuiState class.
/// </summary>
public class TuiStateTests
{
    /// <summary>
    /// Verifies that moving down when at the start of the list increments the selected index.
    /// </summary>
    [Fact]
    public void MoveDown_WhenAtStart_IncrementsSelectedIndex()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-a" },
                new() { Id = 2, Name = "app-b" },
                new() { Id = 3, Name = "app-c" }
            }
        };

        state.MoveDown(state.Applications.Count);

        state.SelectedIndex.Should().Be(1);
    }

    /// <summary>
    /// Verifies that moving down when at the last item does not exceed the boundary.
    /// </summary>
    [Fact]
    public void MoveDown_WhenAtLastItem_DoesNotExceedBoundary()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-a" },
                new() { Id = 2, Name = "app-b" }
            },
            SelectedIndex = 1
        };

        state.MoveDown(state.Applications.Count);

        state.SelectedIndex.Should().Be(1);
    }

    /// <summary>
    /// Verifies that moving up when at the second item decrements the selected index.
    /// </summary>
    [Fact]
    public void MoveUp_WhenAtSecondItem_DecrementsSelectedIndex()
    {
        var state = new TuiState { SelectedIndex = 2 };

        state.MoveUp();

        state.SelectedIndex.Should().Be(1);
    }

    /// <summary>
    /// Verifies that moving up when at the first item remains at zero.
    /// </summary>
    [Fact]
    public void MoveUp_WhenAtFirstItem_RemainsAtZero()
    {
        var state = new TuiState { SelectedIndex = 0 };

        state.MoveUp();

        state.SelectedIndex.Should().Be(0);
    }

    /// <summary>
    /// Verifies that getting the selected app with a valid index returns the correct app.
    /// </summary>
    /// <param name="state">The TuiState instance to test.</param>
    [Fact]
    public void GetSelectedApp_WithValidIndex_ReturnsCorrectApp()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 10, Name = "alpha" },
                new() { Id = 20, Name = "beta" }
            },
            SelectedIndex = 1
        };

        var selected = state.GetSelectedApp();

        selected.Should().NotBeNull();
        selected!.Id.Should().Be(20);
        selected.Name.Should().Be("beta");
    }

    /// <summary>
    /// Verifies that getting the selected app with an empty list returns null.
    /// </summary>
    [Fact]
    public void GetSelectedApp_WithEmptyList_ReturnsNull()
    {
        var state = new TuiState();

        var selected = state.GetSelectedApp();

        selected.Should().BeNull();
    }

    /// <summary>
    /// Verifies that resetting the selection sets the index and offset to zero.
    /// </summary>
    [Fact]
    public void ResetSelection_SetsIndexAndOffsetToZero()
    {
        var state = new TuiState { SelectedIndex = 5, ScrollOffset = 3 };

        state.ResetSelection();

        state.SelectedIndex.Should().Be(0);
        state.ScrollOffset.Should().Be(0);
    }

    /// <summary>
    /// Verifies that getting the visible apps scrolls down when the selection exceeds the window.
    /// </summary>
    [Fact]
    public void GetVisibleApps_ScrollsDownWhenSelectionExceedsWindow()
    {
        var apps = Enumerable.Range(1, 10)
            .Select(i => new ApplicationDeployment { Id = i, Name = $"app-{i}" })
            .ToList();

        var state = new TuiState { Applications = apps, SelectedIndex = 7, ScrollOffset = 0 };

        var visible = state.GetVisibleApps(5);

        visible.Should().HaveCount(5);
        visible.First().Id.Should().Be(4); // scrolled so index 7 is visible at bottom of window
    }

    /// <summary>
    /// Verifies that getting the visible apps with zero rows returns an empty list.
    /// </summary>
    [Fact]
    public void GetVisibleApps_WithZeroRows_ReturnsEmptyList()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment> { new() { Id = 1 } }
        };

        var visible = state.GetVisibleApps(0);

        visible.Should().BeEmpty();
    }
}
