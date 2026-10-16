# CoolifyOptions

Startup options for the Coolify API connection. They are bound through `IOptions<CoolifyOptions>`,
validated by `CoolifyOptionsValidator` on host start, and checked before any HTTP call is made.

## Properties

| Property | Default | Rule |
|----------|---------|------|
| `BaseUrl` | `https://api.coolify.io` | Required. Absolute `http` or `https` URL with no path, query, fragment, or credentials. |
| `ApiToken` | *(none)* | Required. Not a placeholder (see below). Never printed. |
| `TimeoutSeconds` | `30` | Required range: `1`–`600` inclusive. |
| `DefaultProjectUuid` | *(none)* | Optional. Not validated. |

### BaseUrl rules

- Must be absolute: `https://coolify.example.com`, or `http://localhost:8000`.
- Scheme must be `http` or `https`.
- Must not contain a path. The client calls `/api/v1/...` from the host root, so a value such as
  `https://coolify.example.com/api/v1` would have its path silently discarded. It is rejected instead.
- Must not contain `user:password@` credentials. Use `ApiToken` for authentication.
- Must not contain a query string or fragment.

### ApiToken rules

- Required. Whitespace-only counts as missing.
- Rejected when it looks like a placeholder: `changeme`, `replace-me`, `your-token-here`, `your_api_token`,
  `todo`, `xxx`, `<...>`, `${...}`, or `$NAME`. Matching is case-insensitive.

### TimeoutSeconds rules

- Must be between `1` and `600` inclusive. The message includes the value that was supplied.
- A non-numeric value, such as `COOLIFY_TIMEOUT=soon`, is reported as its own problem, because the binder
  cannot produce a number from it.

## Sources and precedence

Sources are listed from highest to lowest precedence. The first source that sets a value wins.

| Precedence | Source | Example |
|-----------|--------|---------|
| 1 (highest) | CLI flags | `--coolify-url https://c.example.com` or `--coolify-url=https://c.example.com` |
| 2 | Environment variables | `COOLIFY_URL=https://c.example.com` |
| 3 | JSON config file | `~/.config/coolify/config.json`, or the file named by `--coolify-config` |
| 4 (lowest) | Built-in defaults | the defaults in the table above |

An empty or whitespace-only environment variable is treated as unset and falls through to the next source.

### Names per source

| Property | CLI flag | Environment variable | Config key |
|----------|----------|----------------------|------------|
| `BaseUrl` | `--coolify-url` | `COOLIFY_URL` (legacy: `COOLIFY_API_URL`) | `Coolify:BaseUrl` |
| `ApiToken` | `--coolify-token` | `COOLIFY_TOKEN` (legacy: `COOLIFY_API_KEY`) | `Coolify:ApiToken` |
| `TimeoutSeconds` | `--coolify-timeout` | `COOLIFY_TIMEOUT` | `Coolify:TimeoutSeconds` |
| `DefaultProjectUuid` | `--coolify-project` | `COOLIFY_PROJECT_UUID` | `Coolify:DefaultProjectUuid` |
| *(config file path)* | `--coolify-config` | `COOLIFY_CONFIG_FILE` | — |

Legacy names are used only when the current name is unset. If both are set, the current name wins.

Only arguments that start with `--coolify-` are read. All other arguments are left to the command parser.

Passing a token as a flag puts it in shell history and in the process list. Prefer `COOLIFY_TOKEN`
or the config file, and keep the config file readable only by its owner (`chmod 600`).

### Config file

```json
{
  "Coolify": {
    "BaseUrl": "https://coolify.example.com",
    "ApiToken": "<your token>",
    "TimeoutSeconds": 30,
    "DefaultProjectUuid": "<optional project uuid>"
  }
}
```

A missing config file is not an error. A file that exists but is not valid JSON is reported as a configuration problem.

## Startup validation

`CoolifyStartupValidator.ValidateAsync(args)` builds the configuration and starts a minimal host, which runs
`ValidateOnStart`. It makes no HTTP calls. Call it before any command runs:

- Valid: returns `0` and writes nothing.
- Invalid: writes one message that lists **every** problem, and returns `3` (`Constants.ExitCodes.ConfigurationError`).

Example output for `COOLIFY_URL=ftp://x COOLIFY_TOKEN=changeme COOLIFY_TIMEOUT=0`:

```
Invalid Coolify configuration (3 problems):
  - BaseUrl must use the http or https scheme.
  - ApiToken looks like a placeholder value. Replace it with a real token from your Coolify instance.
  - TimeoutSeconds must be between 1 and 600 (got 0). Set COOLIFY_TIMEOUT, --coolify-timeout, or Coolify:TimeoutSeconds in the config file.
Set values with --coolify-* flags, COOLIFY_* environment variables, or the config file. See docs/CoolifyOptions.md.
```

Messages never contain the token value, and they do not repeat the base URL, so credentials placed in a URL are not echoed.
