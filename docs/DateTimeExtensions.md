# DateTimeExtensions

A utility class providing common date/time operations such as relative time formatting, duration calculations, and boundary normalization for `DateTime` values.

## API

### `public static string ToRelativeTime(DateTime date)`

Converts a `DateTime` value into a human-readable relative time string (e.g., "2 minutes ago", "in 3 hours").

- **Parameters**
  - `date` – The `DateTime` value to convert.
- **Return value**
  - A localized string representing the relative time.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `date` is outside the supported range for relative time formatting.

---

### `public static string ToReadableDuration(TimeSpan duration)`

Formats a `TimeSpan` into a human-readable duration string (e.g., "2 days, 3 hours").

- **Parameters**
  - `duration` – The `TimeSpan` to format.
- **Return value**
  - A localized string representing the duration.
- **Exceptions**
  - Throws `OverflowException` if the `TimeSpan` components exceed representable limits.

---

### `public static string ToFormattedDuration(TimeSpan duration, string format)`

Formats a `TimeSpan` using a custom format string (e.g., "dd' days 'hh' hours '").

- **Parameters**
  - `duration` – The `TimeSpan` to format.
  - `format` – A custom format string using placeholders like `dd`, `hh`, `mm`, `ss`.
- **Return value**
  - A string formatted according to the provided format.
- **Exceptions**
  - Throws `FormatException` if the format string is invalid.
  - Throws `OverflowException` if the `TimeSpan` components exceed representable limits.

---
### `public static string MillisecondsToReadable(long milliseconds)`

Converts a millisecond count into a human-readable duration string (e.g., "2.5 seconds").

- **Parameters**
  - `milliseconds` – The number of milliseconds to convert.
- **Return value**
  - A localized string representing the duration.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `milliseconds` is negative.

---
### `public static DateTime StartOfDay(DateTime date)`

Returns a `DateTime` representing the start of the given day (time set to 00:00:00).

- **Parameters**
  - `date` – The input `DateTime`.
- **Return value**
  - A `DateTime` at the start of the day in the same timezone.
- **Exceptions**
  - None.

---
### `public static DateTime EndOfDay(DateTime date)`

Returns a `DateTime` representing the end of the given day (time set to 23:59:59.999).

- **Parameters**
  - `date` – The input `DateTime`.
- **Return value**
  - A `DateTime` at the end of the day in the same timezone.
- **Exceptions**
  - None.

---
### `public static DateTime StartOfWeek(DateTime date, DayOfWeek startDay = DayOfWeek.Monday)`

Returns a `DateTime` representing the start of the week, based on the specified `startDay`.

- **Parameters**
  - `date` – The input `DateTime`.
  - `startDay` – The day of the week considered the start of the week (default: `Monday`).
- **Return value**
  - A `DateTime` at the start of the week.
- **Exceptions**
  - None.

---
### `public static DateTime StartOfMonth(DateTime date)`

Returns a `DateTime` representing the first moment of the month.

- **Parameters**
  - `date` – The input `DateTime`.
- **Return value**
  - A `DateTime` at the start of the month.
- **Exceptions**
  - None.

---
### `public static DateTime EndOfMonth(DateTime date)`

Returns a `DateTime` representing the last moment of the month.

- **Parameters**
  - `date` – The input `DateTime`.
- **Return value**
  - A `DateTime` at the end of the month.
- **Exceptions**
  - None.

---
### `public static bool IsPast(DateTime date)`

Determines whether the given `DateTime` is in the past relative to `DateTime.Now`.

- **Parameters**
  - `date` – The `DateTime` to check.
- **Return value**
  - `true` if `date` is earlier than `DateTime.Now`; otherwise, `false`.
- **Exceptions**
  - None.

---
### `public static bool IsFuture(DateTime date)`

Determines whether the given `DateTime` is in the future relative to `DateTime.Now`.

- **Parameters**
  - `date` – The `DateTime` to check.
- **Return value**
  - `true` if `date` is later than `DateTime.Now`; otherwise, `false`.
- **Exceptions**
  - None.

---
### `public static bool IsToday(DateTime date)`

Determines whether the given `DateTime` falls on the current day.

- **Parameters**
  - `date` – The `DateTime` to check.
- **Return value**
  - `true` if `date` is on the same day as `DateTime.Today`; otherwise, `false`.
- **Exceptions**
  - None.

---
### `public static string ToIso8601String(DateTime date)`

Formats a `DateTime` as an ISO 8601 string (e.g., "2024-05-20T14:30:00Z").

- **Parameters**
  - `date` – The `DateTime` to format.
- **Return value**
  - An ISO 8601-compliant string.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `date` is outside the valid range for ISO formatting.

---
### `public static int BusinessDaysBetween(DateTime start, DateTime end)`

Calculates the number of business days (excluding weekends and optionally custom holidays) between two dates.

- **Parameters**
  - `start` – The start date.
  - `end` – The end date.
- **Return value**
  - The count of business days between `start` and `end` (inclusive of `start`, exclusive of `end` if `start` == `end`).
- **Exceptions**
  - Throws `ArgumentException` if `start` is after `end`.

---
### `public static DateTime RoundToMinute(DateTime date)`

Rounds a `DateTime` to the nearest minute, rounding up at 30 seconds or more.

- **Parameters**
  - `date` – The `DateTime` to round.
- **Return value**
  - A `DateTime` rounded to the nearest minute.
- **Exceptions**
  - None.

---
### `public static DateTime FromUnixTimestamp(long timestamp)`

Converts a Unix timestamp (seconds since 1970-01-01T00:00:00Z) to a `DateTime` in UTC.

- **Parameters**
  - `timestamp` – The Unix timestamp in seconds.
- **Return value**
  - A `DateTime` representing the timestamp in UTC.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `timestamp` is outside the valid range for `DateTime`.

---
### `public static long ToUnixTimestamp(DateTime date)`

Converts a `DateTime` to a Unix timestamp (seconds since 1970-01-01T00:00:00Z).

- **Parameters**
  - `date` – The `DateTime` to convert.
- **Return value**
  - The Unix timestamp in seconds.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `date` is outside the valid range for Unix timestamps.

---

## Usage
