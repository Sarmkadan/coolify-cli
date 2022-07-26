# TuiStateTests
The `TuiStateTests` class is a collection of test cases designed to verify the correctness of the `TuiState` class, which manages the state of a terminal user interface. These tests cover various scenarios, including navigation, selection, and scrolling, to ensure that the `TuiState` class behaves as expected under different conditions.

## API
The `TuiStateTests` class contains the following public members:
* `public void MoveDown_WhenAtStart_IncrementsSelectedIndex`: Tests that moving down from the start of the list increments the selected index.
* `public void MoveDown_WhenAtLastItem_DoesNotExceedBoundary`: Tests that moving down from the last item does not exceed the list boundary.
* `public void MoveUp_WhenAtSecondItem_DecrementsSelectedIndex`: Tests that moving up from the second item decrements the selected index.
* `public void MoveUp_WhenAtFirstItem_RemainsAtZero`: Tests that moving up from the first item remains at index zero.
* `public void GetSelectedApp_WithValidIndex_ReturnsCorrectApp`: Tests that getting the selected app with a valid index returns the correct app.
* `public void GetSelectedApp_WithEmptyList_ReturnsNull`: Tests that getting the selected app with an empty list returns null.
* `public void ResetSelection_SetsIndexAndOffsetToZero`: Tests that resetting the selection sets the index and offset to zero.
* `public void GetVisibleApps_ScrollsDownWhenSelectionExceedsWindow`: Tests that getting visible apps scrolls down when the selection exceeds the window.
* `public void GetVisibleApps_WithZeroRows_ReturnsEmptyList`: Tests that getting visible apps with zero rows returns an empty list.

## Usage
Here are two examples of using the `TuiStateTests` class:
```csharp
// Example 1: Testing navigation
TuiStateTests tests = new TuiStateTests();
tests.MoveDown_WhenAtStart_IncrementsSelectedIndex();
tests.MoveUp_WhenAtSecondItem_DecrementsSelectedIndex();

// Example 2: Testing selection and scrolling
TuiStateTests tests2 = new TuiStateTests();
tests2.GetSelectedApp_WithValidIndex_ReturnsCorrectApp();
tests2.GetVisibleApps_ScrollsDownWhenSelectionExceedsWindow();
```

## Notes
The `TuiStateTests` class is designed to be used in a testing environment, and its methods should not be called concurrently. The tests assume that the `TuiState` class is properly initialized and configured before being used. Additionally, the tests do not cover error handling scenarios, such as null or invalid input, which should be handled separately. The `TuiStateTests` class is not thread-safe, and its methods should not be called from multiple threads simultaneously.
