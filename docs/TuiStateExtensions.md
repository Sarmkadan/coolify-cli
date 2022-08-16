# TuiStateExtensions

Provides extension methods for manipulating the state of a text‑based user interface (TUI) represented by the `TuiState` type. These methods encapsulate common UI navigation and status‑update operations, allowing callers to adjust selection and display messages in a concise, fluent manner.

## API

### MoveDown
```csharp
public static void MoveDown(this TuiState state)
```
Moves the selection down by one item in the underlying collection.

- **Parameters**  
  - `state`: The `TuiState` instance whose selection should be moved.
- **Return value**  
  - `void`.
- **Exceptions**  
  - `ArgumentNullException` if `state` is `null`.  
  - `InvalidOperationException` if the selection is already at the last item or the collection is empty.

### MoveUp
```csharp
public static void MoveUp(this TuiState state)
```
Moves the selection up by one item in the underlying collection.

- **Parameters**  
  - `state`: The `TuiState` instance whose selection should be moved.
- **Return value**  
  - `void`.
- **Exceptions**  
  - `ArgumentNullException` if `state` is `null`.  
  - `InvalidOperationException` if the selection is already at the first item or the collection is empty.

### SetSelection
```csharp
public static void SetSelection(this TuiState state, int index)
```
Sets the selection to the specified zero‑based index.

- **Parameters**  
  - `state`: The `TuiState` instance to modify.  
  - `index`: The zero‑based index of the item to select.
- **Return value**  
  - `void`.
- **Exceptions**  
  - `ArgumentNullException` if `state` is `null`.  
  - `ArgumentOutOfRangeException` if `index` is less than `0` or greater than or equal to the number of items in the collection.

### UpdateStatus
```csharp
public static void UpdateStatus(this TuiState state, string message)
```
Updates the status message shown in the TUI.

- **Parameters**  
  - `state`: The `TuiState` instance whose status should be updated.  
  - `message`: The new status text to display.
- **Return value**  
  - `void`.
- **Exceptions**  
  - `ArgumentNullException` if `state` or `message` is `null`.

## Usage

### Example 1: Navigating a list with keyboard input
```csharp
var uiState = new TuiState(items: myList);

while (running)
{
    var key = Console.ReadKey(true).Key;
    switch (key)
    {
        case ConsoleKey.DownArrow:
            uiState.MoveDown();
            break;
        case ConsoleKey.UpArrow:
            uiState.MoveUp();
            break;
        case ConsoleKey.Enter:
            ExecuteSelectedItem(uiState);
            break;
    }
    Render(uiState);
}
```

### Example 2: Programmatically selecting an item and showing feedback
```csharp
var uiState = new TuiState(items: options);

// Select the third option (index 2) and inform the user.
uiState.SetSelection(2);
uiState.UpdateStatus($"Selected: {options[2]}");

Render(uiState);
```

## Notes
- **Edge cases**  
  - If the collection managed by `TuiState` is empty, `MoveUp` and `MoveDown` will always throw `InvalidOperationException`.  
  - Calling `SetSelection` with an index that is valid at the time of invocation may become invalid if the collection is subsequently modified; callers should re‑validate or refresh the state as needed.  
  - `UpdateStatus` accepts any string, including empty strings; passing `null` will raise an exception.

- **Thread‑safety**  
  - These extension methods operate directly on the supplied `TuiState` instance and do not internal locking. Concurrent invocation from multiple threads on the same `TuiState` instance can lead to race conditions and inconsistent UI state. External synchronization (e.g., locking around calls) is required when shared state is accessed from multiple threads.
