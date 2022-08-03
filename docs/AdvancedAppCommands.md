# AdvancedAppCommands

Provides factory methods for creating specialized `Command` instances that encapsulate advanced operations for managing applications in the `coolify-cli` tool. These commands target restart, environment variable updates, scaling, and rollback scenarios, offering a consistent interface for executing application-level actions.

## API

### `Command CreateRestartCommand()`

Creates a command that restarts the target application. The resulting command encapsulates the restart logic and can be executed via the CLI or integrated into pipelines.

- **Parameters**: None
- **Return Value**: A `Command` instance configured to restart the application.
- **Exceptions**: May throw if the application state is invalid or restart is not permitted.

---

### `Command CreateSetEnvCommand(string name, string value)`

Creates a command that sets an environment variable for the target application.

- **Parameters**:
  - `name` (string): The name of the environment variable to set.
  - `value` (string): The value to assign to the environment variable.
- **Return Value**: A `Command` instance configured to set the specified environment variable.
- **Exceptions**:
  - Throws `ArgumentNullException` if `name` or `value` is `null`.
  - Throws `ArgumentException` if `name` is empty or whitespace.

---

### `Command CreateScaleCommand(int instances)`

Creates a command that scales the target application to the specified number of instances.

- **Parameters**:
  - `instances` (int): The desired number of application instances.
- **Return Value**: A `Command` instance configured to scale the application.
- **Exceptions**:
  - Throws `ArgumentOutOfRangeException` if `instances` is less than zero.

---
### `Command CreateRollbackCommand()`

Creates a command that performs a rollback of the target application to a previous stable state.

- **Parameters**: None
- **Return Value**: A `Command` instance configured to execute the rollback.
- **Exceptions**: May throw if no rollback targets are available or the application is in an unrecoverable state.

## Usage
