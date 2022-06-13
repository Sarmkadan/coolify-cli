# Changelog

## [2.0.2] - 2026-05-21
### Fixed
- Nullable dereference on `Console.ReadLine` in `env sync` confirmation prompt

## [2.0.0] - 2026-05-01
### Added
- Infrastructure-as-code commands: `iac validate`, `iac diff`, `iac apply`, `iac init`
- YAML template engine with `${VAR}` variable substitution
- Deployment diff preview: `app diff`
- Resource usage monitoring: `resources show`, `resources watch`
- Interactive TUI: `coolify-cli tui`
- `env sync` command for bulk environment variable sync from file

### Changed
- Improved error messages with actionable suggestions on all commands
- Color output auto-disabled when stdout is redirected or `NO_COLOR` is set

## [0.6.0] - 2025-06-16
### Added
- Output formatters: table, JSON, CSV (`--format` flag on list commands)
- Collection, DateTime, and Enum extension helpers

## [0.4.0] - 2025-04-07
### Added
- System health check (`coolify-cli health`)
- Database management: `db list`, `db health`, `db backup`, `db logs`

## [0.2.0] - 2025-02-03
### Added
- `DatabaseService` with list, health, backup, and restore operations
- Improved error output on API 4xx/5xx responses

## [0.1.0] - 2025-01-06
### Added
- Initial release: `app list/get/deploy/stop/restart`, `env get/set/delete`, `logs`, `version`
- `CoolifyApiClient`, `ApplicationService`, `EnvironmentVariableService`, `LogService`
- Configuration via `COOLIFY_API_KEY` / `COOLIFY_API_URL` environment variables
- Color-coded console output, `--verbose` flag
