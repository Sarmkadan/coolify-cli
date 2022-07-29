# TuiState

`TuiState` is a mutable state container used by the terminal user interface (TUI) in `coolify-cli` to manage application and database listings, user navigation, and UI rendering state. It tracks the active view, selection indices, scroll position, and refresh status, while providing methods to manipulate the visible application list and query selections.

## API

### `public TuiView ActiveView`
Gets or sets the currently active view in the TUI. This determines which section of the UI (e.g., applications, databases, logs) is being interacted with. Changing this value triggers a re-render of the UI to reflect the new context.

### `public int SelectedIndex`
Gets or sets the zero-based index of the currently selected item within the visible list. This index is used to highlight the selected item in the UI and is adjusted when the user navigates up or down.

### `public int ScrollOffset`
Gets or sets the number of items to skip when rendering the visible list. This allows the UI to implement virtual scrolling for large lists by only rendering a subset of items at a time.

### `public int? SelectedAppId`
Gets or sets the ID of the currently selected application, or `null` if no application is selected. This is used to track the user’s selection across view changes and refreshes.

### `public List<ApplicationDeployment> Applications`
Gets or sets the full list of deployed applications. This list is filtered and sorted to produce the visible subset shown in the UI based on the current view and search criteria.

### `public List<DatabaseConfiguration> Databases`
Gets or sets the full list of database configurations. Similar to `Applications`, this list is filtered to produce the visible subset shown in the database view.

### `public string StatusMessage`
Gets or sets a message to display in the status bar of the TUI. This is typically used to show operation results, errors, or informational updates to the user.

### `public bool IsRefreshing`
Gets or sets a value indicating whether a background refresh operation is in progress. When `true`, the UI may display a loading indicator or disable interactive elements during refresh.

### `public DateTime LastRefreshedAt`
Gets or sets the timestamp of the last successful refresh operation. This is used to display the time of the last update in the UI and to avoid unnecessary refreshes.

### `public bool ShouldExit`
Gets or sets a value indicating whether the TUI should terminate and exit. When `true`, the main loop will break and the application will close gracefully.

### `public void MoveDown()`
Moves the selection down by one item. If the selection is at the end of the visible list, the behavior depends on the current view and scroll state. No exception is thrown; if no items are visible, the call has no effect.

### `public void MoveUp()`
Moves the selection up by one item. If the selection is at the top of the visible list, the behavior depends on the current view and scroll state. No exception is thrown; if no items are visible, the call has no effect.

### `public void ResetSelection()`
Resets the selection to the first item in the visible list and resets the scroll offset to zero. This is typically called when switching views or after a refresh to ensure a consistent starting point.

### `public ApplicationDeployment? GetSelectedApp()`
Returns the currently selected application deployment, or `null` if no application is selected or the selection is out of bounds. This method does not throw exceptions.

### `public IReadOnlyList<ApplicationDeployment> GetVisibleApps()`
Returns a read-only view of the currently visible application deployments, filtered and sorted according to the active view and any applied filters. The returned list is a snapshot and will not reflect subsequent changes to `Applications` or the filter state.
