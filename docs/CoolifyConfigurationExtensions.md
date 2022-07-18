# CoolifyConfigurationExtensions

Extension methods for `CoolifyConfiguration` that provide convenient access to common configuration values and derived settings.

## API

### `GetApiUrlWithTrailingSlash(CoolifyConfiguration config)`

Returns the API URL from the configuration with a trailing slash appended if not already present.

- **Parameters**
  - `config` – The `CoolifyConfiguration` instance containing the API URL.
- **Return value**
  - The API URL with a trailing slash.
- **Exceptions**
  - Throws `ArgumentNullException` if `config` is `null`.

### `ShouldLogVerbose(CoolifyConfiguration config)`

Determines whether verbose logging is enabled based on the configuration.

- **Parameters**
  - `config` – The `CoolifyConfiguration` instance containing logging settings.
- **Return value**
  - `true` if verbose logging is enabled; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `config` is `null`.

### `GetRequestTimeoutMilliseconds(CoolifyConfiguration config)`

Returns the request timeout in milliseconds from the configuration.

- **Parameters**
  - `config` – The `CoolifyConfiguration` instance containing the timeout setting.
- **Return value**
  - The timeout value in milliseconds.
- **Exceptions**
  - Throws `ArgumentNullException` if `config` is `null`.
  - Throws `InvalidOperationException` if the timeout value is not a valid positive integer.

### `Clone(CoolifyConfiguration config)`

Creates a deep copy of the `CoolifyConfiguration` instance.

- **Parameters**
  - `config` – The `CoolifyConfiguration` instance to clone.
- **Return value**
  - A new `CoolifyConfiguration` instance with the same values.
- **Exceptions**
  - Throws `ArgumentNullException` if `config` is `null`.

## Usage
