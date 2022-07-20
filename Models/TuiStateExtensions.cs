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
    public static void MoveDown(this TuiState state, int steps, int listSize)
    {
        if (listSize == 0) return;
        state.SelectedIndex = Math.Min(state.SelectedIndex + steps, listSize - 1);
    }

    /// <summary>
    /// Moves the selection cursor up by the specified number of rows, clamped to zero.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="steps">Number of rows to move up.</param>
    public static void MoveUp(this TuiState state, int steps)
    {
        state.SelectedIndex = Math.Max(state.SelectedIndex - steps, 0);
    }

    /// <summary>
    /// Sets the selection to a specific index, clamping it to valid range.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="index">The index to select.</param>
    /// <param name="listSize">Total number of items in the current list.</param>
    public static void SetSelection(this TuiState state, int index, int listSize)
    {
        if (listSize == 0) return;
        state.SelectedIndex = Math.Clamp(index, 0, listSize - 1);
    }

    /// <summary>
    /// Updates the status message with a formatted timestamp.
    /// </summary>
    /// <param name="state">The TUI state to modify.</param>
    /// <param name="message">The status message to set.</param>
    public static void UpdateStatus(this TuiState state, string message)
    {
        state.StatusMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
    }
}