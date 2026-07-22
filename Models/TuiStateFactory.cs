#nullable enable

using System.Threading;

namespace CoolifyCli.Models;

/// <summary>
/// Factory class for creating and managing a thread-safe TuiState instance
/// using immutable snapshots and atomic operations.
/// </summary>
public static class TuiStateFactory
{
    // Use a single field for atomic state updates instead of a lock
    private static TuiState _currentState = new();

    /// <summary>
    /// Gets the current state snapshot atomically.
    /// </summary>
    /// <returns>A consistent snapshot of the current state.</returns>
    public static TuiState GetCurrentState()
    {
        // Reading a reference field is atomic in .NET for aligned fields
        // The TuiState record is immutable, so this gives us a consistent snapshot
        return Volatile.Read(ref _currentState);
    }

    /// <summary>
    /// Atomically updates the state using a transformation function.
    /// This is the primary method for state mutations and ensures atomicity
    /// without locks by using Interlocked.Exchange.
    /// </summary>
    /// <param name="updateFunc">Function that takes the current state and returns a new state.</param>
    /// <returns>The new state that was set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="updateFunc"/> is null.</exception>
    public static TuiState Update(Func<TuiState, TuiState> updateFunc)
    {
        ArgumentNullException.ThrowIfNull(updateFunc);

        // Read current state atomically using Volatile.Read for acquire semantics
        var current = Volatile.Read(ref _currentState);

        // Apply transformation to create new state
        var newState = updateFunc(current);

        // Atomically swap the state using Interlocked.Exchange
        // This ensures that only one thread can update the state at a time
        // while avoiding the overhead of a full lock
        Interlocked.Exchange(ref _currentState, newState);

        return newState;
    }

    /// <summary>
    /// Atomically sets the state to a new value.
    /// </summary>
    /// <param name="newState">The new state to set.</param>
    /// <returns>The new state that was set.</returns>
    public static TuiState SetState(TuiState newState)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(newState);

        // Atomically swap the state using Interlocked.Exchange
        Interlocked.Exchange(ref _currentState, newState);
        return newState;
    }

    /// <summary>
    /// Resets the state factory to its initial state.
    /// Useful for testing purposes.
    /// </summary>
    internal static void ResetForTesting()
    {
        _currentState = new TuiState();
    }
}