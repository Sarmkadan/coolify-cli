# TextFormatter

`TextFormatter` provides a fluent, composable API for building richly formatted console output in the `coolify-cli` tool. It accumulates styled text segments, layout elements (indentation, separators, tables, panels), and progress indicators into an internal buffer, then renders the complete result via `ToString()` or clears it with `Clear()`. All write methods return the same `TextFormatter` instance, enabling method chaining.

## API

### `public TextFormatter WriteLine`
Appends a plain, unstyled line of text followed by a newline to the internal buffer.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteLineColored`
Appends a line of text rendered with the specified console color, followed by a newline.

- **Parameters:** `string text`, `ConsoleColor color`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteSuccess`
Appends a line prefixed with a success marker (typically a green checkmark) and styled in the success color.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteError`
Appends a line prefixed with an error marker (typically a red cross) and styled in the error color.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteWarning`
Appends a line prefixed with a warning marker (typically a yellow exclamation triangle) and styled in the warning color.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteInfo`
Appends a line prefixed with an info marker (typically a blue “i” icon) and styled in the info color.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteHeader`
Appends a prominent header line, typically rendered in a bold or highlighted style.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteSubheader`
Appends a secondary header line, visually subordinate to `WriteHeader`.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter Indent`
Increases the current indentation level by one step. All subsequent writes are offset by the cumulative indentation.

- **Parameters:** None.
- **Returns:** The current `TextFormatter` instance.

### `public TextFormatter Outdent`
Decreases the current indentation level by one step, but never below zero.

- **Parameters:** None.
- **Returns:** The current `TextFormatter` instance.

### `public TextFormatter WriteKeyValue`
Appends a line consisting of a key, a separator, and a value, typically used for configuration or property display. The key and value may receive distinct styling.

- **Parameters:** `string key`, `string value`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `key` or `value` is `null`.

### `public TextFormatter WriteListItem`
Appends a bulleted list item at the current indentation level.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteNumberedItem`
Appends a sequentially numbered list item. The number increments automatically with each call.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public TextFormatter WriteCodeBlock`
Appends a block of text formatted as code, typically with a monospace background or border, preserving whitespace and newlines.

- **Parameters:** `string code`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `code` is `null`.

### `public TextFormatter WriteSeparator`
Appends a horizontal separator line (e.g., a repeated dash or rule) spanning the current content width.

- **Parameters:** None.
- **Returns:** The current `TextFormatter` instance.

### `public TextFormatter WriteProgressBar`
Appends a textual progress bar representation for the given completion ratio.

- **Parameters:** `double progress` (a value between 0.0 and 1.0 inclusive)
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentOutOfRangeException` when `progress` is less than 0.0 or greater than 1.0.

### `public TextFormatter WriteTable`
Appends a formatted table from a collection of rows. Column widths are typically derived from the data.

- **Parameters:** `IEnumerable<string[]> rows`, optionally `string[] headers`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `rows` is `null`. Behavior is undefined if rows contain `null` elements.

### `public TextFormatter WritePanel`
Appends a bordered panel containing the given text, visually distinct from surrounding output.

- **Parameters:** `string text`
- **Returns:** The current `TextFormatter` instance.
- **Throws:** `ArgumentNullException` when `text` is `null`.

### `public void Clear`
Resets the internal buffer, indentation level, and any counters (such as the numbered list counter) to their initial state. After calling `Clear`, the instance behaves as if newly constructed.

### `public override string ToString`
Returns the complete formatted string accumulated in the internal buffer, including all ANSI escape sequences or markup required for console rendering.

## Usage

**Example 1: Status report with headers, key-value pairs, and a table**

```csharp
var formatter = new TextFormatter();

string output = formatter
    .WriteHeader("Deployment Status")
    .WriteSeparator()
    .WriteKeyValue("Environment", "production")
    .WriteKeyValue("Version", "2.4.1")
    .WriteSeparator()
    .WriteSubheader("Container Health")
    .WriteTable(
        new[] {
            new[] { "web", "healthy", "2" },
            new[] { "worker", "healthy", "4" },
            new[] { "cache", "degraded", "1" }
        },
        new[] { "Service", "Status", "Replicas" }
    )
    .WriteSeparator()
    .WriteSuccess("All core services operational")
    .WriteWarning("cache service latency above threshold")
    .ToString();

Console.WriteLine(output);
```

**Example 2: Indented task list with progress**

```csharp
var formatter = new TextFormatter();

string output = formatter
    .WriteHeader("Build Pipeline")
    .Indent()
        .WriteNumberedItem("Restore packages")
        .WriteNumberedItem("Compile source")
        .Indent()
            .WriteListItem("Core library")
            .WriteListItem("CLI frontend")
        .Outdent()
        .WriteNumberedItem("Run tests")
        .WriteProgressBar(0.66)
    .Outdent()
    .WriteSeparator()
    .WriteInfo("Build in progress — 2 of 3 stages complete")
    .ToString();

Console.WriteLine(output);
```

## Notes

- **Fluent design:** Every write method returns the same `TextFormatter` instance. Callers must avoid capturing intermediate references and continuing to mutate them after deriving the final string, as subsequent mutations affect all aliases.
- **Indentation scope:** `Indent` and `Outdent` affect all subsequent writes until the next `Indent`/`Outdent` or `Clear`. There is no automatic scope reset; mismatched pairs will leave the formatter in a permanently indented state for future uses unless `Clear` is called.
- **Numbered item counter:** The internal counter used by `WriteNumberedItem` persists across calls and is only reset by `Clear`. Reusing a formatter instance without clearing it will continue numbering from the previous sequence.
- **Thread safety:** `TextFormatter` is not thread-safe. Concurrent calls to write methods or `Clear` from multiple threads without external synchronization will corrupt the internal buffer and counters.
- **Null handling:** All methods accepting `string` parameters throw `ArgumentNullException` on `null`. Empty strings are permitted and produce correspondingly empty or marker-only lines.
- **Progress bar range:** `WriteProgressBar` strictly requires a value in [0.0, 1.0]. Values outside this range throw `ArgumentOutOfRangeException`. NaN and infinity values also fall outside the valid range and will throw.
- **Table data integrity:** `WriteTable` expects each row array to have the same number of elements as the header array (if provided). Mismatched row lengths may produce misaligned output; no automatic padding or truncation is guaranteed.
- **ToString idempotence:** Calling `ToString()` does not clear the buffer. Repeated calls return the same accumulated content until `Clear` or further writes are performed.
