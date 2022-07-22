# DateTimeExtensionsTests

Contains unit tests for the `DateTime` and `TimeSpan` extension methods provided by the `coolify-cli` project. Each test method verifies a specific formatting or calculation scenario, ensuring that the extensions produce the expected human-readable output or date boundary values.

## API

All test methods are parameterless, return `void`, and are intended to be run by a unit testing framework. They do not throw exceptions directly; instead, they use assertions to validate behavior. A test failure indicates a mismatch between the extension method’s output and the expected result.

| Method | Description |
|--------|-------------|
| `ToRelativeTime_WhenJustNow_ReturnsJustNow` | Verifies that a `DateTime` within the last few seconds returns `"just now"`. |
| `ToRelativeTime_WhenFewSecondsAgo_ReturnsSecondsAgo` | Verifies that a `DateTime` a few seconds in the past returns a string like `"X seconds ago"`. |
| `ToRelativeTime_WhenAboutOneMinuteAgo_ReturnsMinuteAgo` | Verifies that a `DateTime` approximately one minute ago returns `"a minute ago"`. |
| `ToRelativeTime_WhenSeveralHoursAgo_ReturnsHoursAgo` | Verifies that a `DateTime` several hours ago returns a string like `"X hours ago"`. |
| `ToRelativeTime_WhenSeveralDaysAgo_ReturnsDaysAgo` | Verifies that a `DateTime` several days ago returns a string like `"X days ago"`. |
| `ToRelativeTime_WhenTwoWeeksAgo_ReturnsWeeksAgo` | Verifies that a `DateTime` two weeks ago returns `"2 weeks ago"`. |
| `ToRelativeTime_WhenSeveralMonthsAgo_ReturnsMonthsAgo` | Verifies that a `DateTime` several months ago returns a string like `"X months ago"`. |
| `ToRelativeTime_WhenOverAYearAgo_ReturnsYearsAgo` | Verifies that a `DateTime` more than a year ago returns a string like `"X years ago"`. |
| `ToReadableDuration_WithOnlySeconds_ShowsSecondsComponent` | Verifies that a `TimeSpan` containing only seconds is formatted as `"X seconds"`. |
| `ToReadableDuration_WithHoursMinutesSeconds_ShowsAllComponents` | Verifies that a `TimeSpan` with hours, minutes, and seconds is formatted as `"Xh Ym Zs"`. |
| `ToReadableDuration_WithDays_IncludesDayComponent` | Verifies that a `TimeSpan` spanning days is formatted as `"Xd Yh Zm Zs"`. |
| `ToReadableDuration_WithZeroTimeSpan_ShowsZeroSeconds` | Verifies that a zero `TimeSpan` returns `"0 seconds"`. |
| `ToFormattedDuration_SubSecondRange_ShowsMilliseconds` | Verifies that a `TimeSpan` less than one second is formatted as `"Xms"`. |
| `ToFormattedDuration_SecondRange_ShowsSeconds` | Verifies that a `TimeSpan` between one second and one minute is formatted as `"Xs"`. |
| `ToFormattedDuration_MinuteRange_ShowsMinutes` | Verifies that a `TimeSpan` between one minute and one hour is formatted as `"Xm"`. |
| `StartOfDay_ReturnsDateWithZeroTime` | Verifies that `StartOfDay()` returns the same date with time set to `00:00:00.000`. |
| `EndOfDay_ReturnsLastTickOfDay` | Verifies that `EndOfDay()` returns the same date with time set to `23:59:59.9999999`. |
| `StartOfWeek_WithMondayStart_ReturnsPrecedingMonday` | Verifies that `StartOfWeek(DayOfWeek.Monday)` returns the preceding Monday (or the same day if already Monday). |
| `StartOfMonth_ReturnsFirstDayOfMonth` | Verifies that `StartOfMonth()` returns the first day of the month at midnight. |
| `EndOfMonth_ReturnsLastDayOfMonth` | Verifies that `EndOfMonth()` returns the last day of the month at the last tick of that day. |

## Usage

The following examples demonstrate how to call the actual extension methods that are tested by this class.

```csharp
using coolify_cli.Extensions;

// Example 1: Human-readable relative time
DateTime past = DateTime.UtcNow.AddMinutes(-45);
string relative = past.ToRelativeTime();
Console.WriteLine(relative); // Output: "45 minutes ago"

// Example 2: Formatting a duration
TimeSpan duration = new TimeSpan(2, 3, 15, 30); // 2 days, 3 hours, 15 minutes, 30 seconds
string readable = duration.ToReadableDuration();
Console.WriteLine(readable); // Output: "2d 3h 15m 30s"

// Example 3: Getting start and end of a month
DateTime today = DateTime.Today;
DateTime monthStart = today.StartOfMonth();
DateTime monthEnd = today.EndOfMonth();
Console.WriteLine($"From {monthStart:yyyy-MM-dd HH:mm:ss.fff} to {monthEnd:yyyy-MM-dd HH:mm:ss.fff}");
```

## Notes

- **Time‑sensitivity**: `ToRelativeTime()` depends on the current system clock. Tests that use this method may fail if the time delta between the test’s reference `DateTime` and the moment of execution exceeds the expected threshold. The test suite accounts for this by using fixed offsets.
- **Thread safety**: All extension methods are stateless and operate only on their input value. They are safe to call concurrently from multiple threads.
- **Edge cases**:
  - `StartOfDay` and `EndOfDay` preserve the `DateTimeKind` of the input (e.g., `Utc`, `Local`, `Unspecified`).
  - `StartOfWeek` uses the provided `DayOfWeek` parameter; the default overload (if any) is not covered by these tests.
  - `ToFormattedDuration` truncates to the largest unit (e.g., `"5m"` for 5 minutes 10 seconds) – sub‑second values are only shown when the total duration is less than one second.
  - `ToReadableDuration` always includes all non‑zero components from days down to seconds; zero components are omitted unless the entire `TimeSpan` is zero.
- **Time zone**: The tests assume `DateTime` values are in UTC or local time as appropriate. No time zone conversions are performed by the extension methods.
