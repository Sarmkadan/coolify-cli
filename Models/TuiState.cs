#nullable enable
namespace CoolifyCli.Models;

/// <summary>
/// Represents the current view mode within the interactive TUI.
/// </summary>
public enum TuiView
{
    AppList,
    AppDetail,
    DbList,
    LogStream,
    Help
}

/// <summary>
/// Encapsulates the mutable state of the interactive TUI session.
/// Tracks navigation position, selected resources, and active view.
/// </summary>
public class TuiState
{
    /// <summary>Gets or sets the currently active view panel.</summary>
    public TuiView ActiveView { get; set; } = TuiView.AppList;

    /// <summary>Gets or sets the zero-based index of the highlighted row in the current list.</summary>
    public int SelectedIndex { get; set; } = 0;

    /// <summary>Gets or sets the zero-based scroll offset for lists taller than the terminal.</summary>
    public int ScrollOffset { get; set; } = 0;

    /// <summary>Gets or sets the ID of the currently selected application, or null if none selected.</summary>
    public int? SelectedAppId { get; set; }

    /// <summary>Gets or sets the list of applications loaded into the TUI.</summary>
    public List<ApplicationDeployment> Applications { get; set; } = new();

    /// <summary>Gets or sets the list of databases loaded into the TUI.</summary>
    public List<DatabaseConfiguration> Databases { get; set; } = new();

    /// <summary>Gets or sets a status message displayed in the footer bar.</summary>
    public string StatusMessage { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether a background refresh is in progress.</summary>
    public bool IsRefreshing { get; set; } = false;

    /// <summary>Gets or sets the timestamp of the last successful data refresh.</summary>
    public DateTime LastRefreshedAt { get; set; } = DateTime.MinValue;

    /// <summary>Gets or sets a value indicating whether the TUI should exit on the next tick.</summary>
    public bool ShouldExit { get; set; } = false;

    /// <summary>
    /// Moves the selection cursor down by one row, clamped to the list size.
    /// </summary>
    /// <param name="listSize">Total number of items in the current list.</param>
    public void MoveDown(int listSize)
    {
        if (listSize == 0) return;
        SelectedIndex = Math.Min(SelectedIndex + 1, listSize - 1);
    }

    /// <summary>
    /// Moves the selection cursor up by one row, clamped to zero.
    /// </summary>
    public void MoveUp()
    {
        SelectedIndex = Math.Max(SelectedIndex - 1, 0);
    }

    /// <summary>
    /// Resets the selection cursor and scroll offset to the top of the list.
    /// </summary>
    public void ResetSelection()
    {
        SelectedIndex = 0;
        ScrollOffset = 0;
    }

    /// <summary>
    /// Returns the application at the current selection index, or null if the list is empty.
    /// </summary>
    public ApplicationDeployment? GetSelectedApp()
    {
        if (Applications.Count == 0 || SelectedIndex < 0 || SelectedIndex >= Applications.Count)
            return null;
        return Applications[SelectedIndex];
    }

    /// <summary>
    /// Computes the visible window of items given the terminal height, updating
    /// <see cref="ScrollOffset"/> so that <see cref="SelectedIndex"/> is always in view.
    /// </summary>
    /// <param name="visibleRows">Number of rows available for the list panel.</param>
    /// <returns>Slice of items that should be rendered.</returns>
    public IReadOnlyList<ApplicationDeployment> GetVisibleApps(int visibleRows)
    {
        if (visibleRows <= 0 || Applications.Count == 0)
            return Array.Empty<ApplicationDeployment>();

        if (SelectedIndex < ScrollOffset)
            ScrollOffset = SelectedIndex;
        else if (SelectedIndex >= ScrollOffset + visibleRows)
            ScrollOffset = SelectedIndex - visibleRows + 1;

        return Applications
            .Skip(ScrollOffset)
            .Take(visibleRows)
            .ToList();
    }
}
