# ConsoleLogger

A lightweight, console-bound logger that emits formatted messages to standard output. Designed for CLI tools and development scenarios where simplicity and immediate visibility of logs are prioritized over structured storage or remote delivery.

## API

### `public ConsoleLogger()`
Initializes a new instance of the `ConsoleLogger` with default formatting settings. The logger writes to `Console.Out` using a basic timestamp and log-level prefix.

### `public void Info(string message)`
Writes an informational message to the console.

- **Parameters**
  - `message`: The content to log. Must not be null.
- **Return value**
  - None.
- **Exceptions**
  - Throws `ArgumentNullException` if `message` is null.

### `public void Debug(string message)`
Writes a debug-level message to the console. Debug messages are typically suppressed in production builds or when verbose logging is disabled.

- **Parameters**
  - `message`: The content to log. Must not be null.
- **Return value**
  - None.
- **Exceptions**
  - Throws `ArgumentNullException` if `message` is null.

### `public void Warn(string message)`
Writes a warning-level message to the console, indicating a non-critical issue that may require attention.

- **Parameters**
  - `message`: The content to log. Must not be null.
- **Return value**
  - None.
- **Exceptions**
  - Throws `ArgumentNullException` if `message` is null.

### `public void Error(string message)`
Writes an error-level message to the console, indicating a failure that prevents normal operation but does not terminate the application.

- **Parameters**
  - `message`: The content to log. Must not be null.
- **Return value**
  - None.
- **Exceptions**
  - Throws `ArgumentNullException` if `message` is null.

### `public void Fatal(string message)`
Writes a fatal-level message to the console and terminates the application with exit code 1. Used for unrecoverable errors.

- **Parameters**
  - `message`: The content to log. Must not be null.
- **Return value**
  - None.
- **Exceptions**
  - Throws `ArgumentNullException` if `message` is null.

## Usage
