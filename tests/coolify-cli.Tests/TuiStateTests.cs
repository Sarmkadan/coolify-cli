#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using CoolifyCli.Models;
using FluentAssertions;
using Xunit;

namespace CoolifyCli.Tests;

public class TuiStateTests
{
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

    [Fact]
    public void MoveUp_WhenAtSecondItem_DecrementsSelectedIndex()
    {
        var state = new TuiState { SelectedIndex = 2 };

        state.MoveUp();

        state.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public void MoveUp_WhenAtFirstItem_RemainsAtZero()
    {
        var state = new TuiState { SelectedIndex = 0 };

        state.MoveUp();

        state.SelectedIndex.Should().Be(0);
    }

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

    [Fact]
    public void GetSelectedApp_WithEmptyList_ReturnsNull()
    {
        var state = new TuiState();

        var selected = state.GetSelectedApp();

        selected.Should().BeNull();
    }

    [Fact]
    public void ResetSelection_SetsIndexAndOffsetToZero()
    {
        var state = new TuiState { SelectedIndex = 5, ScrollOffset = 3 };

        state.ResetSelection();

        state.SelectedIndex.Should().Be(0);
        state.ScrollOffset.Should().Be(0);
    }

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
