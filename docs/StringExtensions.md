# StringExtensions

Provides commonly used string manipulation and validation extension methods for C# applications, including case conversion, truncation, repetition, validation, masking, and formatting utilities.

## API

### `public static string ToPascalCase(string? input)`

Converts a string to PascalCase by splitting on word boundaries and capitalizing each segment.

- **Parameters**
  - `input` – The string to convert; `null` returns `null`.
- **Return value**
  - The PascalCased string, or `null` if `input` is `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `input` is `null` and the method is called in a context that does not permit `null`.

### `public static string ToCamelCase(string? input)`

Converts a string to camelCase by splitting on word boundaries and capitalizing the first letter of each segment except the first.

- **Parameters**
  - `input` – The string to convert; `null` returns `null`.
- **Return value**
  - The camelCased string, or `null` if `input` is `null`.

### `public static string ToSnakeCase(string? input)`

Converts a string to snake_case by inserting underscores between word boundaries and converting to lowercase.

- **Parameters**
  - `input` – The string to convert; `null` returns `null`.
- **Return value**
  - The snake_cased string, or `null` if `input` is `null`.

### `public static string ToKebabCase(string? input)`

Converts a string to kebab-case by inserting hyphens between word boundaries and converting to lowercase.

- **Parameters**
  - `input` – The string to convert; `null` returns `null`.
- **Return value**
  - The kebab-cased string, or `null` if `input` is `null`.

### `public static string Truncate(string? input, int maxLength, string? ellipsis = "…")`

Truncates a string to a specified maximum length and appends an ellipsis if truncated.

- **Parameters**
  - `input` – The string to truncate; `null` returns `null`.
  - `maxLength` – The maximum allowed length of the result.
  - `ellipsis` – The string to append when truncating; defaults to `"…"`.
- **Return value**
  - The truncated string, or `null` if `input` is `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `maxLength` is negative.
  - Throws `ArgumentNullException` if `ellipsis` is `null`.

### `public static string Repeat(string? input, int count)`

Repeats a string a specified number of times.

- **Parameters**
  - `input` – The string to repeat; `null` returns `null`.
  - `count` – The number of repetitions.
- **Return value**
  - The repeated string, or `null` if `input` is `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `count` is negative.

### `public static bool IsValidEmail(string? input)`

Determines whether a string is a valid email address.

- **Parameters**
  - `input` – The string to validate; `null` returns `false`.
- **Return value**
  - `true` if the string is a valid email address; otherwise, `false`.

### `public static bool IsValidUrl(string? input)`

Determines whether a string is a valid URL.

- **Parameters**
  - `input` – The string to validate; `null` returns `false`.
- **Return value**
  - `true` if the string is a valid URL; otherwise, `false`.

### `public static bool IsValidIpAddress(string? input)`

Determines whether a string is a valid IP address (IPv4 or IPv4-mapped IPv6).

- **Parameters**
  - `input` – The string to validate; `null` returns `false`.
- **Return value**
  - `true` if the string is a valid IP address; otherwise, `false`.

### `public static string MaskSensitive(string? input, char mask = '*', int keepLeft = 0, int keepRight = 0)`

Masks sensitive portions of a string, preserving a specified number of characters at the beginning and end.

- **Parameters**
  - `input` – The string to mask; `null` returns `null`.
  - `mask` – The character used for masking; defaults to `'*'`.
  - `keepLeft` – The number of characters to leave unmasked at the start.
  - `keepRight` – The number of characters to leave unmasked at the end.
- **Return value**
  - The masked string, or `null` if `input` is `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `keepLeft` or `keepRight` is negative.
  - Throws `ArgumentException` if `keepLeft + keepRight` exceeds the length of `input`.

### `public static string ToQueryParameter(string? input)`

Encodes a string for safe use as a URL query parameter value.

- **Parameters**
  - `input` – The string to encode; `null` returns `null`.
- **Return value**
  - The URL-encoded string, or `null` if `input` is `null`.

### `public static string FromQueryParameter(string? input)`

Decodes a URL-encoded query parameter value.

- **Parameters**
  - `input` – The string to decode; `null` returns `null`.
- **Return value**
  - The decoded string, or `null` if `input` is `null`.

### `public static string[] SplitTrimmed(string? input, params char[] separators)`

Splits a string by specified separators and trims whitespace from each resulting segment.

- **Parameters**
  - `input` – The string to split; `null` returns an empty array.
  - `separators` – The characters used as delimiters.
- **Return value**
  - An array of trimmed substrings.

### `public static string ToIdentifier(string? input)`

Converts a string to a valid C# identifier by removing invalid characters and ensuring it starts with a letter or underscore.

- **Parameters**
  - `input` – The string to convert; `null` returns `null`.
- **Return value**
  - A valid C# identifier, or `null` if `input` is `null`.

### `public static string WithColor(string? input, ConsoleColor color)`

Wraps a string with ANSI color codes for console output.

- **Parameters**
  - `input` – The string to colorize; `null` returns `null`.
  - `color` – The console color to apply.
- **Return value**
  - The ANSI-colored string, or `null` if `input` is `null`.

### `public static string PadTo(string? input, int totalWidth, char paddingChar = ' ')`

Pads a string to a specified total width, aligning content to the left.

- **Parameters**
  - `input` – The string to pad; `null` returns `null`.
  - `totalWidth` – The total width of the resulting string.
  - `paddingChar` – The character used for padding; defaults to space.
- **Return value**
  - The padded string, or `null` if `input` is `null`.
- **Exceptions**
  - Throws `ArgumentOutOfRangeException` if `totalWidth` is negative.

## Usage
