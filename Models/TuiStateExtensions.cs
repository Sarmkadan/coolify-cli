#nullable enable

namespace CoolifyCli.Models;

/// <summary>
/// Provides useful extension methods for <see cref="TuiState"/> to enhance navigation and state management.
/// </summary>
public static class TuiStateExtensions
{
    /// <summary>
    /// Moves the selection cursor down by the specified number of rows, clamped to the list size.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="steps">Number of rows to move down.</param>
    /// <param name="listSize">Total number of items in the current list.</param>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="steps"/> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="listSize"/> is negative.</exception>
    public static TuiState MoveDown(this TuiState state, int steps, int listSize)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentOutOfRangeException.ThrowIfNegative(steps);
        ArgumentOutOfRangeException.ThrowIfNegative(listSize);

        if (listSize == 0)
            return state;

        var newIndex = Math.Min(state.SelectedIndex + steps, listSize - 1);
        return state.WithSelectedIndex(newIndex);
    }

    /// <summary>
    /// Moves the selection cursor up by the specified number of rows, clamped to zero.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="steps">Number of rows to move up.</param>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="steps"/> is negative.</exception>
    public static TuiState MoveUp(this TuiState state, int steps)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentOutOfRangeException.ThrowIfNegative(steps);
        var newIndex = Math.Max(state.SelectedIndex - steps, 0);
        return state.WithSelectedIndex(newIndex);
    }

    /// <summary>
    /// Sets the selection to a specific index, clamping it to valid range.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="index">The index to select.</param>
    /// <param name="listSize">Total number of items in the current list.</param>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="listSize"/> is negative.</exception>
    public static TuiState SetSelection(this TuiState state, int index, int listSize)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(listSize);

        if (listSize == 0)
            return state;

        var newIndex = Math.Clamp(index, 0, listSize - 1);
        return state.WithSelectedIndex(newIndex);
    }

    /// <summary>
    /// Updates the status message with a formatted timestamp.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="message">The status message to set.</param>
    /// <exception cref="ArgumentNullException"><paramref name="state"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
    public static TuiState UpdateStatus(this TuiState state, string message)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(message);

        return state.WithStatusMessage($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
