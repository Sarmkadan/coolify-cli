# TuiStateTestsExtensions

Provides a set of extension methods for creating and validating `TuiState` objects in unit tests for the terminal user interface component. These methods enable fluent construction of test states and declarative assertions about selection and scroll positions.

## API

### WithApplications
**Purpose:** Returns a new `TuiState` instance with its applications collection replaced by the supplied values.  
**Parameters:**  
- `applications`: An `IEnumerable<TuiApplication>` containing the applications to assign to the state.  
**Return value:** A `TuiState` object whose `Applications` property reflects the provided collection.  
**Exceptions:** Throws `ArgumentNullException` if `applications` is `null`.

### WithSelectedIndex
**Purpose:** Returns a new `TuiState` instance with the selected index set to the specified value.  
**Parameters:**  
- `selectedIndex`: An `int` representing the zero‑based index of the selected application.  
**Return value:** A `TuiState` object whose `SelectedIndex` property equals `selectedIndex`.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if `selectedIndex` is less than zero or greater than or equal to the number of applications in the state.

### WithScrollOffset
**Purpose:** Returns a new `TuiState` instance with the vertical scroll offset set to the specified value.  
**Parameters:**  
- `scrollOffset`: An `int` indicating the number of lines scrolled from the top.  
**Return value:** A `TuiState` object whose `ScrollOffset` property equals `scrollOffset`.  
**Exceptions:** Throws `ArgumentOutOfRangeException` if `scrollOffset` is negative.

### SelectedIndexShouldBe
**Purpose:** Asserts that the `SelectedIndex` of the given state matches the expected value.  
**Parameters:**  
- `state`: The `TuiState` instance to inspect.  
- `expected`: The anticipated `SelectedIndex` value.  
**Return value:** None.  
**Exceptions:** Throws an `AssertFailedException` (or test framework‑specific equivalent) when `state.SelectedIndex` does not equal `expected`.

### ScrollOffsetShouldBe
**Purpose:** Asserts that the `ScrollOffset` of the given state matches the expected value.  
**Parameters:**  
- `state`: The `TuiState` instance to inspect.  
- `expected`: The anticipated `ScrollOffset` value.  
**Return value:** None.  
**Exceptions:** Throws an `AssertFailedException` when `state.ScrollOffset` does not equal `expected`.

### SelectedAppShouldBe
**Purpose:** Asserts that the currently selected application in the state equals the expected application.  
**Parameters:**  
- `state`: The `TuiState` instance to inspect.  
- `expected`: The `TuiApplication` instance that should be selected.  
**Return value:** None.  
**Exceptions:** Throws an `ArgumentException` if `state.SelectedIndex` is out of range; throws an `AssertFailedException` when the application at `state.SelectedIndex` is not reference‑equal to `expected`.

### MoveDownAndAssert
**Purpose:** Simulates a downward navigation action on the state and asserts that the selection has moved accordingly (wrapping to the first item when at the bottom).  
**Parameters:**  
- `state`: The `TuiState` instance to operate on.  
**Return value:** None.  
**Exceptions:** Throws an `InvalidOperationException` if the state contains no applications; throws an `AssertFailedException` when the resulting `SelectedIndex` does not match the expected value after the move.

### MoveUpAndAssert
**Purpose:** Simulates an upward navigation action on the state and asserts that the selection has moved accordingly (wrapping to the last item when at the top).  
**Parameters:**  
- `state`: The `TuiState` instance to operate on.  
**Return value:** None.  
**Exceptions:** Throws an `InvalidOperationException` if the state contains no applications; throws an `AssertFailedException` when the resulting `SelectedIndex` does not match the expected value after the move.

## Usage

```csharp
var apps = new[] { new App("coolify"), new App("nginx"), new App("redis") };
var state = new TuiState()
    .WithApplications(apps)
    .WithSelectedIndex(1)   // nginx is selected
    .WithScrollOffset(0);

// Assert the initial selection
state.SelectedIndexShouldBe(1);
state.SelectedAppShouldBe(apps[1]);

// Move down and verify selection changes to the third app
state.MoveDownAndAssert();
state.SelectedIndexShouldBe(2);
state.SelectedAppShouldBe(apps[2]);
```

```csharp
var emptyState = new TuiState().WithApplications(Array.Empty<App>());
try
{
    // Attempting to move selection on an empty list should fail
    emptyState.MoveDownAndAssert();
}
catch (InvalidOperationException)
{
    // Expected – no items to navigate
}
```

## Notes
- All extension methods are pure; they do not mutate the original `TuiState` instance if the type is immutable. When mutation occurs (e.g., in the move methods), the operation is performed on a copy and the original remains unchanged.
- The methods are stateless and thread‑safe; concurrent calls on distinct `TuiState` instances pose no risk of race conditions.
- Passing `null` for the `applications` argument to `WithApplications` or supplying an out‑of‑range index to the `With*` methods will result in an exception before any state is constructed.
- The assertion methods rely on reference equality for `SelectedAppShouldBe`; ensure that the expected object is the exact instance stored in the state’s applications list, or override equality semantics in `TuiApplication` if value‑based comparison is required.
