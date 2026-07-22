#nullable enable
using System.Threading;

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
/// Immutable snapshot of the TUI state at a point in time.
/// </summary>
/// <param name="ActiveView">The currently active view panel.</param>
/// <param name="SelectedIndex">The zero-based index of the highlighted row in the current list.</param>
/// <param name="ScrollOffset">The zero-based scroll offset for lists taller than the terminal.</param>
/// <param name="SelectedAppId">The ID of the currently selected application, or null if none selected.</param>
/// <param name="Applications">The list of applications loaded into the TUI.</param>
/// <param name="Databases">The list of databases loaded into the TUI.</param>
/// <param name="StatusMessage">A status message displayed in the footer bar.</param>
/// <param name="IsRefreshing">A value indicating whether a background refresh is in progress.</param>
/// <param name="LastRefreshedAt">The timestamp of the last successful data refresh.</param>
/// <param name="ShouldExit">A value indicating whether the TUI should exit on the next tick.</param>
public record TuiState(
    TuiView ActiveView = TuiView.AppList,
    int SelectedIndex = 0,
    int ScrollOffset = 0,
    int? SelectedAppId = null,
    List<ApplicationDeployment> Applications = null!,
    List<DatabaseConfiguration> Databases = null!,
    string StatusMessage = "",
    bool IsRefreshing = false,
    DateTime LastRefreshedAt = default,
    bool ShouldExit = false)
{
    /// <summary>
    /// Initializes a new immutable TuiState with default values.
    /// </summary>
    public TuiState()
        : this(
            ActiveView: TuiView.AppList,
            SelectedIndex: 0,
            ScrollOffset: 0,
            SelectedAppId: null,
            Applications: new List<ApplicationDeployment>(),
            Databases: new List<DatabaseConfiguration>(),
            StatusMessage: string.Empty,
            IsRefreshing: false,
            LastRefreshedAt: DateTime.MinValue,
            ShouldExit: false)
    {
    }

    /// <summary>
    /// Creates a new state with the specified ActiveView.
    /// </summary>
    /// <param name="activeView">The view to set as active.</param>
    /// <returns>A new TuiState instance with the updated ActiveView.</returns>
    public TuiState WithActiveView(TuiView activeView) =>
        this with { ActiveView = activeView };

    /// <summary>
    /// Creates a new state with the specified SelectedIndex.
    /// </summary>
    /// <param name="selectedIndex">The index to set as selected.</param>
    /// <returns>A new TuiState instance with the updated SelectedIndex.</returns>
    public TuiState WithSelectedIndex(int selectedIndex) =>
        this with { SelectedIndex = selectedIndex };

    /// <summary>
    /// Creates a new state with the specified ScrollOffset.
    /// </summary>
    /// <param name="scrollOffset">The scroll offset to set.</param>
    /// <returns>A new TuiState instance with the updated ScrollOffset.</returns>
    public TuiState WithScrollOffset(int scrollOffset) =>
        this with { ScrollOffset = scrollOffset };

    /// <summary>
    /// Creates a new state with the specified SelectedAppId.
    /// </summary>
    /// <param name="selectedAppId">The application ID to select, or null to deselect.</param>
    /// <returns>A new TuiState instance with the updated SelectedAppId.</returns>
    public TuiState WithSelectedAppId(int? selectedAppId) =>
        this with { SelectedAppId = selectedAppId };

    /// <summary>
    /// Creates a new state with the specified Applications list.
    /// </summary>
    /// <param name="applications">The applications list to set.</param>
    /// <returns>A new TuiState instance with the updated Applications list.</returns>
    public TuiState WithApplications(List<ApplicationDeployment> applications) =>
        this with { Applications = applications };

    /// <summary>
    /// Creates a new state with the specified Databases list.
    /// </summary>
    /// <param name="databases">The databases list to set.</param>
    /// <returns>A new TuiState instance with the updated Databases list.</returns>
    public TuiState WithDatabases(List<DatabaseConfiguration> databases) =>
        this with { Databases = databases };

    /// <summary>
    /// Creates a new state with the specified StatusMessage.
    /// </summary>
    /// <param name="statusMessage">The status message to set.</param>
    /// <returns>A new TuiState instance with the updated StatusMessage.</returns>
    public TuiState WithStatusMessage(string statusMessage) =>
        this with { StatusMessage = statusMessage };

    /// <summary>
    /// Creates a new state with the specified IsRefreshing flag.
    /// </summary>
    /// <param name="isRefreshing">The refreshing flag to set.</param>
    /// <returns>A new TuiState instance with the updated IsRefreshing flag.</returns>
    public TuiState WithIsRefreshing(bool isRefreshing) =>
        this with { IsRefreshing = isRefreshing };

    /// <summary>
    /// Creates a new state with the specified LastRefreshedAt timestamp.
    /// </summary>
    /// <param name="lastRefreshedAt">The timestamp to set.</param>
    /// <returns>A new TuiState instance with the updated LastRefreshedAt timestamp.</returns>
    public TuiState WithLastRefreshedAt(DateTime lastRefreshedAt) =>
        this with { LastRefreshedAt = lastRefreshedAt };

    /// <summary>
    /// Creates a new state with the specified ShouldExit flag.
    /// </summary>
    /// <param name="shouldExit">The exit flag to set.</param>
    /// <returns>A new TuiState instance with the updated ShouldExit flag.</returns>
    public TuiState WithShouldExit(bool shouldExit) =>
        this with { ShouldExit = shouldExit };

    /// <summary>
    /// Moves the selection cursor down by one row, clamped to the list size.
    /// </summary>
    /// <param name="listSize">Total number of items in the current list.</param>
    /// <returns>A new TuiState instance with updated selection.</returns>
    public TuiState MoveDown(int listSize)
    {
        if (listSize == 0)
            return this;

        var newIndex = Math.Min(SelectedIndex + 1, listSize - 1);
        return this with { SelectedIndex = newIndex };
    }

    /// <summary>
    /// Moves the selection cursor up by one row, clamped to zero.
    /// </summary>
    /// <returns>A new TuiState instance with updated selection.</returns>
    public TuiState MoveUp()
    {
        var newIndex = Math.Max(SelectedIndex - 1, 0);
        return this with { SelectedIndex = newIndex };
    }

    /// <summary>
    /// Resets the selection cursor and scroll offset to the top of the list.
    /// </summary>
    /// <returns>A new TuiState instance with reset selection.</returns>
    public TuiState ResetSelection() =>
        this with { SelectedIndex = 0, ScrollOffset = 0 };

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

        var newScrollOffset = ScrollOffset;
        if (SelectedIndex < newScrollOffset)
            newScrollOffset = SelectedIndex;
        else if (SelectedIndex >= newScrollOffset + visibleRows)
            newScrollOffset = SelectedIndex - visibleRows + 1;

        return Applications
            .Skip(newScrollOffset)
            .Take(visibleRows)
            .ToList()
            .AsReadOnly();
    }
}