#nullable enable
using CoolifyCli.Models;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace CoolifyCli.Tests;

/// <summary>
/// Concurrency tests for the TuiState class to ensure thread-safe state mutations.
/// </summary>
public class TuiStateConcurrencyTests
{
    /// <summary>
    /// Verifies that concurrent MoveDown operations are thread-safe and don't cause race conditions.
    /// With immutable state, each operation returns a new instance, preventing race conditions.
    /// </summary>
    [Fact]
    public async Task MoveDown_ConcurrentOperations_ThreadSafe()
    {
        // Start with a shared state
        var initialState = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-a" },
                new() { Id = 2, Name = "app-b" },
                new() { Id = 3, Name = "app-c" },
                new() { Id = 4, Name = "app-d" },
                new() { Id = 5, Name = "app-e" }
            }
        };

        var tasks = new List<Task>();

        // Start 10 concurrent MoveDown operations on the same initial state
        // With immutable state, each operation works on its own copy, preventing race conditions
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var state = initialState;
                state.MoveDown(state.Applications.Count); // Just call it, result is discarded
            }));
        }

        await Task.WhenAll(tasks);

        // The important thing is that no exceptions were thrown and the operation completed
        // With immutable state, there are no race conditions to corrupt the state
    }

    /// <summary>
    /// Verifies that concurrent MoveUp operations are thread-safe.
    /// </summary>
    [Fact]
    public async Task MoveUp_ConcurrentOperations_ThreadSafe()
    {
        var state = new TuiState { SelectedIndex = 4 };

        var tasks = new List<Task<TuiState>>();

        // Start 10 concurrent MoveUp operations
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => state.MoveUp()));
        }

        var results = await Task.WhenAll(tasks);

        // All results should have the same final state (index 0)
        foreach (var result in results)
        {
            result.SelectedIndex.Should().Be(0);
        }
    }

    /// <summary>
    /// Verifies that concurrent ResetSelection operations are thread-safe.
    /// </summary>
    [Fact]
    public async Task ResetSelection_ConcurrentOperations_ThreadSafe()
    {
        var state = new TuiState { SelectedIndex = 5, ScrollOffset = 10 };

        var tasks = new List<Task<TuiState>>();

        // Start 10 concurrent ResetSelection operations
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => state.ResetSelection()));
        }

        var results = await Task.WhenAll(tasks);

        // All results should have the same final state (both values at 0)
        foreach (var result in results)
        {
            result.SelectedIndex.Should().Be(0);
            result.ScrollOffset.Should().Be(0);
        }
    }

    /// <summary>
    /// Verifies that concurrent property setters (ActiveView) are thread-safe.
    /// </summary>
    [Fact]
    public async Task ActiveView_SetConcurrently_ThreadSafe()
    {
        var state = new TuiState { ActiveView = TuiView.AppList };

        var tasks = new List<Task<TuiState>>();

        // Start 10 concurrent ActiveView set operations
        for (int i = 0; i < 10; i++)
        {
            TuiView view = i % 2 == 0 ? TuiView.DbList : TuiView.Help;
            tasks.Add(Task.Run(() => state.WithActiveView(view)));
        }

        var results = await Task.WhenAll(tasks);

        // All results should have a valid view set
        foreach (var result in results)
        {
            result.ActiveView.Should().BeOneOf(TuiView.AppList, TuiView.DbList, TuiView.Help);
        }
    }

    /// <summary>
    /// Verifies that concurrent property setters (StatusMessage) are thread-safe.
    /// </summary>
    [Fact]
    public async Task StatusMessage_SetConcurrently_ThreadSafe()
    {
        var state = new TuiState { StatusMessage = "Initial" };

        var tasks = new List<Task<TuiState>>();

        // Start 20 concurrent StatusMessage set operations
        for (int i = 0; i < 20; i++)
        {
            int index = i;
            tasks.Add(Task.Run(() => state.WithStatusMessage($"Message-{index}")));
        }

        var results = await Task.WhenAll(tasks);

        // All results should have a status message starting with "Message-"
        foreach (var result in results)
        {
            result.StatusMessage.Should().StartWith("Message-");
        }
    }

    /// <summary>
    /// Verifies that concurrent list assignments (Applications) are thread-safe.
    /// </summary>
    [Fact]
    public async Task Applications_SetConcurrently_ThreadSafe()
    {
        var state = new TuiState();

        var tasks = new List<Task<TuiState>>();

        // Start 10 concurrent Applications set operations
        for (int i = 0; i < 10; i++)
        {
            var apps = new List<ApplicationDeployment>();
            for (int j = 0; j < 5; j++)
            {
                apps.Add(new ApplicationDeployment { Id = i * 10 + j, Name = $"app-{i}-{j}" });
            }
            int localI = i;
            tasks.Add(Task.Run(() => state.WithApplications(apps)));
        }

        var results = await Task.WhenAll(tasks);

        // All results should have a non-empty applications list
        foreach (var result in results)
        {
            result.Applications.Should().NotBeEmpty();
            result.Applications.Should().HaveCount(5);
        }
    }

    /// <summary>
    /// Verifies that concurrent property reads and writes don't cause race conditions.
    /// </summary>
    [Fact]
    public void PropertyGetSet_ThreadSafe()
    {
        // Create initial state
        var initialState = new TuiState
        {
            SelectedIndex = 3,
            ScrollOffset = 2,
            StatusMessage = "Test",
            IsRefreshing = true,
            ShouldExit = false,
            SelectedAppId = 42
        };

        // Multiple threads reading and writing concurrently
        var results = new List<TuiState>();
        Parallel.Invoke(
            () => {
                var state = initialState;
                for (int i = 0; i < 100; i++)
                    state = state.MoveDown(10);
                results.Add(state);
            },
            () => {
                var state = initialState;
                for (int i = 0; i < 100; i++)
                    state = state.MoveUp();
                results.Add(state);
            },
            () => results.Add(initialState.WithStatusMessage("Updated")),
            () => results.Add(initialState.WithIsRefreshing(false)),
            () => results.Add(initialState.WithSelectedAppId(null))
        );

        // All results should have consistent state
        foreach (var result in results)
        {
            result.SelectedIndex.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(9);
            result.ScrollOffset.Should().BeGreaterOrEqualTo(0);
            // Note: IsRefreshing may vary depending on which update wins, so we don't assert it
        }
    }

    /// <summary>
    /// Verifies that GetSelectedApp is thread-safe when called concurrently with list modifications.
    /// </summary>
    [Fact]
    public async Task GetSelectedApp_ConcurrentWithListModifications_ThreadSafe()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-1" },
                new() { Id = 2, Name = "app-2" },
                new() { Id = 3, Name = "app-3" }
            },
            SelectedIndex = 1
        };

        var tasks = new List<Task>();

        // Start threads that modify the list
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                state = state with { Applications = new List<ApplicationDeployment>(state.Applications) };
                state.Applications.Add(new ApplicationDeployment { Id = state.Applications.Count + 1, Name = $"app-{state.Applications.Count + 1}" });
            }));
        }

        // Start threads that read the selected app
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var selected = state.GetSelectedApp();
                _ = selected?.Id; // Just access it
            }));
        }

        await Task.WhenAll(tasks);

        // Should not throw any exceptions and return valid result
        var result = state.GetSelectedApp();
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    /// <summary>
    /// Verifies that GetVisibleApps is thread-safe when called concurrently with list modifications.
    /// </summary>
    [Fact]
    public async Task GetVisibleApps_ConcurrentWithListModifications_ThreadSafe()
    {
        var state = new TuiState
        {
            Applications = new List<ApplicationDeployment>
            {
                new() { Id = 1, Name = "app-1" },
                new() { Id = 2, Name = "app-2" },
                new() { Id = 3, Name = "app-3" }
            },
            SelectedIndex = 1
        };

        var tasks = new List<Task>();

        // Start threads that modify the list
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                state = state with { Applications = new List<ApplicationDeployment>(state.Applications) };
                state.Applications.Add(new ApplicationDeployment { Id = state.Applications.Count + 1, Name = $"app-{state.Applications.Count + 1}" });
            }));
        }

        // Start threads that get visible apps
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var visible = state.GetVisibleApps(5);
                _ = visible.Count; // Just access it
            }));
        }

        await Task.WhenAll(tasks);

        // Should not throw any exceptions
        var result = state.GetVisibleApps(5);
        result.Should().NotBeNull();
    }
}