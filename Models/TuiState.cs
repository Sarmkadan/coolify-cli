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
/// Encapsulates the mutable state of the interactive TUI session.
/// Tracks navigation position, selected resources, and active view.
/// </summary>
public class TuiState
{
    private readonly object _lock = new object();
    /// <summary>Gets or sets the currently active view panel.</summary>
    public TuiView ActiveView
    {
        get { lock (_lock) return _activeView; }
        set { lock (_lock) _activeView = value; }
    }
    private TuiView _activeView = TuiView.AppList;

    /// <summary>Gets or sets the zero-based index of the highlighted row in the current list.</summary>
    public int SelectedIndex
    {
        get { lock (_lock) return _selectedIndex; }
        set { lock (_lock) _selectedIndex = value; }
    }
    private int _selectedIndex = 0;

    /// <summary>Gets or sets the zero-based scroll offset for lists taller than the terminal.</summary>
    public int ScrollOffset
    {
        get { lock (_lock) return _scrollOffset; }
        set { lock (_lock) _scrollOffset = value; }
    }
    private int _scrollOffset = 0;

    /// <summary>Gets or sets the ID of the currently selected application, or null if none selected.</summary>
    public int? SelectedAppId
    {
        get { lock (_lock) return _selectedAppId; }
        set { lock (_lock) _selectedAppId = value; }
    }
    private int? _selectedAppId;

    /// <summary>Gets or sets the list of applications loaded into the TUI.</summary>
    public List<ApplicationDeployment> Applications
    {
        get { lock (_lock) return _applications; }
        set { lock (_lock) _applications = value; }
    }
    private List<ApplicationDeployment> _applications = new();

    /// <summary>Gets or sets the list of databases loaded into the TUI.</summary>
    public List<DatabaseConfiguration> Databases
    {
        get { lock (_lock) return _databases; }
        set { lock (_lock) _databases = value; }
    }
    private List<DatabaseConfiguration> _databases = new();

    /// <summary>Gets or sets a status message displayed in the footer bar.</summary>
    public string StatusMessage
    {
        get { lock (_lock) return _statusMessage; }
        set { lock (_lock) _statusMessage = value; }
    }
    private string _statusMessage = string.Empty;

    /// <summary>Gets or sets a value indicating whether a background refresh is in progress.</summary>
    public bool IsRefreshing
    {
        get { lock (_lock) return _isRefreshing; }
        set { lock (_lock) _isRefreshing = value; }
    }
    private bool _isRefreshing = false;

    /// <summary>Gets or sets the timestamp of the last successful data refresh.</summary>
    public DateTime LastRefreshedAt
    {
        get { lock (_lock) return _lastRefreshedAt; }
        set { lock (_lock) _lastRefreshedAt = value; }
    }
    private DateTime _lastRefreshedAt = DateTime.MinValue;

    /// <summary>Gets or sets a value indicating whether the TUI should exit on the next tick.</summary>
    public bool ShouldExit
    {
        get { lock (_lock) return _shouldExit; }
        set { lock (_lock) _shouldExit = value; }
    }
    private bool _shouldExit = false;

    /// <summary>
    /// Moves the selection cursor down by one row, clamped to the list size.
    /// </summary>
    /// <param name="listSize">Total number of items in the current list.</param>
    public void MoveDown(int listSize)
    {
        lock (_lock)
        {
            if (listSize == 0) return;
            _selectedIndex = Math.Min(_selectedIndex + 1, listSize - 1);
        }
    }

    /// <summary>
    /// Moves the selection cursor up by one row, clamped to zero.
    /// </summary>
    public void MoveUp()
    {
        lock (_lock)
        {
            _selectedIndex = Math.Max(_selectedIndex - 1, 0);
        }
    }

    /// <summary>
    /// Resets the selection cursor and scroll offset to the top of the list.
    /// </summary>
    public void ResetSelection()
    {
        lock (_lock)
        {
            _selectedIndex = 0;
            _scrollOffset = 0;
        }
    }

    /// <summary>
    /// Returns the application at the current selection index, or null if the list is empty.
    /// </summary>
    public ApplicationDeployment? GetSelectedApp()
    {
        lock (_lock)
        {
            if (_applications.Count == 0 || _selectedIndex < 0 || _selectedIndex >= _applications.Count)
                return null;
            return _applications[_selectedIndex];
        }
    }

    /// <summary>
    /// Computes the visible window of items given the terminal height, updating
    /// <see cref="ScrollOffset"/> so that <see cref="SelectedIndex"/> is always in view.
    /// </summary>
    /// <param name="visibleRows">Number of rows available for the list panel.</param>
    /// <returns>Slice of items that should be rendered.</returns>
    public IReadOnlyList<ApplicationDeployment> GetVisibleApps(int visibleRows)
    {
        lock (_lock)
        {
            if (visibleRows <= 0 || _applications.Count == 0)
                return Array.Empty<ApplicationDeployment>();

            if (_selectedIndex < _scrollOffset)
                _scrollOffset = _selectedIndex;
            else if (_selectedIndex >= _scrollOffset + visibleRows)
                _scrollOffset = _selectedIndex - visibleRows + 1;

            return _applications
                .Skip(_scrollOffset)
                .Take(visibleRows)
                .ToList();
        }
    }
}
