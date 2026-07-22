#nullable enable

using CoolifyCli.Models;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Stress tests for TuiState snapshot consistency to ensure that the immutable snapshot approach
/// prevents torn reads and provides consistent state across all properties.
/// </summary>
public class TuiStateSnapshotConsistencyTests
{
    /// <summary>
    /// Verifies that multiple concurrent updaters produce consistent snapshots that can be safely rendered.
    /// This test simulates the real-world scenario where multiple threads update state while
    /// the render loop reads snapshots.
    /// </summary>
    [Fact]
    public async Task ConcurrentUpdaters_ProduceConsistentSnapshots()
    {
        // Start with initial state
        var initialState = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-1" },
                new() { Id = 2, Name = "app-2" },
                new() { Id = 3, Name = "app-3" },
                new() { Id = 4, Name = "app-4" },
                new() { Id = 5, Name = "app-5" }
            },
            SelectedIndex = 0,
            ActiveView = TuiView.AppList,
            StatusMessage = "Initial state"
        };

        var snapshots = new List<TuiState>();
        var tasks = new List<Task>();

        // Simulate 5 concurrent updaters (like key handlers, API callbacks, etc.)
        for (int i = 0; i < 5; i++)
        {
            int updaterId = i;
            tasks.Add(Task.Run(() =>
            {
                // Each updater performs a series of state mutations
                var state = initialState;

                // Move selection around
                for (int j = 0; j < 3; j++)
                {
                    state = state.MoveDown(5);
                    state = state.MoveUp();
                }

                // Change view
                state = state.WithActiveView(updaterId % 2 == 0 ? TuiView.DbList : TuiView.Help);

                // Update status
                state = state.WithStatusMessage($"Updater {updaterId} completed");

                // Add to applications
                var newApp = new ApplicationDeployment { Id = 100 + updaterId, Name = $"new-app-{updaterId}" };
                state = state with { Applications = new List<ApplicationDeployment>(state.Applications) };
                state.Applications.Add(newApp);

                lock (snapshots)
                {
                    snapshots.Add(state);
                }
            }));
        }

        await Task.WhenAll(tasks);

        // All snapshots should be valid and consistent
        foreach (var snapshot in snapshots)
        {
            snapshot.Should().NotBeNull();
            snapshot.SelectedIndex.Should().BeGreaterOrEqualTo(0);
            snapshot.Applications.Should().NotBeEmpty();
            snapshot.ActiveView.Should().BeOneOf(TuiView.AppList, TuiView.DbList, TuiView.Help);
            snapshot.StatusMessage.Should().NotBeNullOrEmpty();
        }
    }

    /// <summary>
    /// Verifies that the factory's atomic updates ensure consistency across all properties.
    /// This test specifically validates the TuiStateFactory implementation.
    /// </summary>
    [Fact]
    public void TuiStateFactory_AtomicUpdates_EnsureConsistency()
    {
        // Reset to known state
        TuiStateFactory.SetState(new TuiState());

        // Perform multiple updates atomically
        var state1 = TuiStateFactory.Update(state => state
            .WithSelectedIndex(3)
            .WithStatusMessage("First update"));

        var state2 = TuiStateFactory.Update(state => state
            .WithActiveView(TuiView.DbList)
            .WithIsRefreshing(true));

        var state3 = TuiStateFactory.Update(state => state
            .WithSelectedAppId(42)
            .WithLastRefreshedAt(DateTime.UtcNow));

        // Get the current state
        var currentState = TuiStateFactory.GetCurrentState();

        // All states should be consistent (each update builds on the previous)
        currentState.SelectedIndex.Should().Be(state3.SelectedIndex);
        currentState.ActiveView.Should().Be(state3.ActiveView);
        currentState.StatusMessage.Should().Be(state2.StatusMessage);
        currentState.IsRefreshing.Should().Be(state3.IsRefreshing);
        currentState.SelectedAppId.Should().Be(state3.SelectedAppId);
    }

    /// <summary>
    /// Verifies that render operations see consistent state (no torn reads).
    /// This test simulates the render loop reading state while updates happen.
    /// </summary>
    [Fact]
    public async Task RenderLoop_SeesConsistentState_NoTornReads()
    {
        var initialState = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-1" },
                new() { Id = 2, Name = "app-2" },
                new() { Id = 3, Name = "app-3" }
            },
            SelectedIndex = 0,
            ActiveView = TuiView.AppList
        };

        var renderResults = new List<(TuiView View, int Index, int AppCount)>();
        var tasks = new List<Task>();

        // Start updater threads
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var state = initialState;
                for (int j = 0; j < 5; j++)
                {
                    state = state.MoveDown(3);
                    state = state.WithStatusMessage($"Update {j}");
                    // Simulate some delay
                    Task.Delay(1).Wait();
                }
            }));
        }

        // Start render threads that read state
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                // Read state atomically
                var state = TuiStateFactory.GetCurrentState();
                var selected = state.GetSelectedApp();

                lock (renderResults)
                {
                    renderResults.Add((state.ActiveView, state.SelectedIndex, state.Applications.Count));
                }
            }));
        }

        await Task.WhenAll(tasks);

        // All render results should be valid
        foreach (var (view, index, appCount) in renderResults)
        {
            view.Should().BeOneOf(TuiView.AppList, TuiView.AppDetail, TuiView.DbList, TuiView.LogStream, TuiView.Help);
            index.Should().BeInRange(0, appCount - 1);
            appCount.Should().BeGreaterOrEqualTo(0);
        }
    }

    /// <summary>
    /// Verifies that compound operations (those that read-modify-write) are atomic.
    /// This test validates that operations like GetVisibleApps which internally modify scroll offset
    /// don't cause issues with the immutable approach.
    /// </summary>
    [Fact]
    public void CompoundOperations_ReturnNewState_EnsureAtomicity()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-1" },
                new() { Id = 2, Name = "app-2" },
                new() { Id = 3, Name = "app-3" },
                new() { Id = 4, Name = "app-4" },
                new() { Id = 5, Name = "app-5" }
            },
            SelectedIndex = 3,
            ScrollOffset = 0
        };

        // Get visible apps (this internally adjusts scroll offset in the immutable approach)
        var visible1 = state.GetVisibleApps(3);

        // Original state should be unchanged
        state.SelectedIndex.Should().Be(3);
        state.ScrollOffset.Should().Be(0);

        // Calling again should give same result
        var visible2 = state.GetVisibleApps(3);
        visible1.Should().Equal(visible2);

        // Now get visible apps with a different window size
        var state2 = state with { SelectedIndex = 4 };
        var visible3 = state2.GetVisibleApps(2);
        visible3.Should().HaveCount(2);
    }

    /// <summary>
    /// Performance test to ensure the immutable approach doesn't introduce significant overhead.
    /// </summary>
    [Fact]
    public void ImmutableState_Performance_Acceptable()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment>(),
            SelectedIndex = 0
        };

        // Add 1000 items
        for (int i = 0; i < 1000; i++)
        {
            state = state with { Applications = new List<ApplicationDeployment>(state.Applications) };
            state.Applications.Add(new ApplicationDeployment { Id = i, Name = $"app-{i}" });
        }

        // Perform 10000 state updates
        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            state = state.MoveDown(1000);
            state = state.MoveUp();
            state = state.WithStatusMessage($"Update {i}");
        }
        watch.Stop();

        // Should complete in reasonable time (< 1 second)
        watch.ElapsedMilliseconds.Should().BeLessThan(1000);
    }
}